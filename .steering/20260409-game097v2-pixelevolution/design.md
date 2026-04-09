# Design: Game097v2 PixelEvolution

## スクリプト構成

### namespace: `Game097v2_PixelEvolution`

| クラス | ファイル | 役割 |
|--------|---------|------|
| `PixelEvolutionGameManager` | `PixelEvolutionGameManager.cs` | 状態管理・StageManager/InstructionPanel統合・スコア |
| `EvolutionManager` | `EvolutionManager.cs` | コアメカニクス（環境・進化・分岐ロジック） |
| `PixelEvolutionUI` | `PixelEvolutionUI.cs` | UI表示・パネル制御 |

## 盤面・ステージデータ設計

### 進化レベル
- 0: 原始ピクセル（単体ドット）
- 1: クラスター（数個のドット集合）
- 2: 単純生命体（形状が出来てくる）
- 3: 複合生命体（複数のパーツ）
- 4: 複雑生命体（対称性・模様）
- 5: 最終形態（目標）★クリア

### 環境パラメータ
```csharp
public enum EnvLevel { Low = 0, Mid = 1, High = 2 }
public struct Environment {
    public EnvLevel Temperature;  // 低・中・高
    public EnvLevel Humidity;     // 低・中・高
    public EnvLevel Light;        // 低・中・高
}
```

### 進化ルール（各ステージ共通基盤）
進化方向は `EnvLevel` の組み合わせで決まる:
```
温度:High + 湿度:Mid + 光量:High → 進化レベル+1（最適環境）
温度:Low + 湿度:Low + 光量:Low → Stage3以降、退化リスク（20%で-1）
特定組み合わせ → 隠し分岐（Stage4以降）
```

### 分岐選択
- 特定の世代（Level変化のタイミング）で分岐パネルが出現
- 各選択肢はアイコン（Sprite）+ 説明テキスト
- 選択後、次世代の「遺伝子方向」が決定される

## 入力処理フロー
- 環境ボタン：UIボタンのOnClickイベント → `EvolutionManager.SetTemperature/Humidity/Light()`
- 世代交代ボタン：UIボタン → `EvolutionManager.AdvanceGeneration()`
- 分岐選択：タップ（UIボタン）→ `EvolutionManager.SelectBranch(int index)`

## SceneSetup の構成方針
- `Setup097v2_PixelEvolution.cs`
- メニュー: `[MenuItem("Assets/Setup/097v2 PixelEvolution")]`
- PixelEvolutionGameManager → StageManager（子）、EvolutionManager（子）
- Canvas → HUD、EnvironmentPanel、EvolutionDisplay、各パネル
- InstructionPanel をフルスクリーンオーバーレイとして生成・配線

## StageManager統合

### StageConfig パラメータ活用
- `speedMultiplier`: 世代交代ボタンの応答速度（アニメーション速度）
- `countMultiplier`: 分岐の選択肢数（2 or 3）
- `complexityFactor`: 退化リスク係数（0.0=退化なし、1.0=高リスク）

### ステージ別パラメータ表
| Stage | speedMultiplier | countMultiplier | complexityFactor | 世代制限 |
|-------|----------------|----------------|-----------------|---------|
| 1 | 1.0 | 2 | 0.0 | 20 |
| 2 | 1.0 | 3 | 0.0 | 15 |
| 3 | 1.2 | 3 | 0.3 | 12 |
| 4 | 1.4 | 3 | 0.6 | 10 |
| 5 | 1.6 | 3 | 1.0 | 8 |

## InstructionPanel内容
```
title: "PixelEvolution"
description: "ピクセル生命体を進化させよう"
controls: "ボタンで環境変更・世代交代、タップで進化方向を選択"
goal: "世代制限内に目標の最終形態まで進化させよう"
```

## ビジュアルフィードバック設計
1. **進化成功時（EvolutionDisplay）**: スケールパルス（1.0→1.3→1.0、0.25秒）+ 緑フラッシュ
2. **退化時**: 赤フラッシュ + カメラシェイク（0.3秒）
3. **分岐発見時**: 黄金色グロウエフェクト（スケール1.0→1.2→1.0、0.3秒）
4. **世代交代アニメ**: EvolutionDisplay のフェードアウト→フェードイン（0.2秒）

## スコアシステム
- 基本スコア: 進化成功ごとに `100 × (ステージ+1) × speedMultiplier`
- コンボボーナス: 3連続「最適環境設定」で×1.5倍
- 隠し分岐発見: +200pt
- 最短世代クリア: スコア×2倍（世代制限の半分以下で完了）
- 退化ペナルティ: なし（退化は状態変化のみ）

## ステージ別新ルール表
- Stage 1: 基本ルール（温度のみが進化に影響）
- Stage 2: 環境複合効果（温度×湿度の組み合わせが重要、単一パラメータ無効化）
- Stage 3: 退化リスク（低環境設定で一定確率で退化、`complexityFactor × 20%`の確率）
- Stage 4: 隠し分岐（特定の環境パターン[温度:H+湿度:H+光量:H]で3択目の隠しルート出現）
- Stage 5: 突然変異（各世代交代で `complexityFactor × 10%` 確率でランダム進化変化）

## 判断ポイントの実装設計
- **トリガー条件**: 毎ターン（世代交代前）に環境パラメータを変更
- **選択の報酬**: 最適環境 → 進化+1、コンボ×1.5
- **選択のペナルティ**: 最悪環境（Stage3以降） → 退化-1、コンボリセット
- **分岐点トリガー**: EvolutionLevel が奇数（1,3）の世代交代後

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;  // 6.0
float camWidth = camSize * Camera.main.aspect;

// ゲーム領域（生命体表示）: 中央
// UI領域（環境ボタン）: 下部 Y=-60〜-70 (Canvas)
// HUD: 上部 Y=top-margin

// 生命体表示エリア（ワールド座標）
float displaySize = 4.0f;  // 4x4ユニットの表示領域（中央）
// Canvas UIは下部2.5〜3ユニット分を確保
```

## EvolutionManager クラス設計
```csharp
public class EvolutionManager : MonoBehaviour {
    // 状態
    int _evolutionLevel;      // 0-5
    int _generation;          // 現在世代
    int _generationLimit;     // 世代制限
    EnvLevel _temperature, _humidity, _light;
    int _consecutiveOptimalCount;  // コンボカウンター
    bool _hasMutation;        // 突然変異フラグ
    bool _hiddenBranchFound;  // 隠し分岐発見フラグ
    
    // コールバック
    public System.Action<int> OnEvolutionLevelChanged;   // 進化レベル変化
    public System.Action<int[]> OnBranchChoiceRequired;  // 分岐選択が必要
    public System.Action OnEvolutionComplete;             // 最終形態到達
    public System.Action OnGenerationLimitReached;        // 世代制限に達した
    public System.Action<bool> OnDevolve;                 // 退化発生
    
    public void SetupStage(StageManager.StageConfig config, int stageIndex);
    public void SetTemperature(int level);   // 0=Low, 1=Mid, 2=High
    public void SetHumidity(int level);
    public void SetLight(int level);
    public void AdvanceGeneration();         // 世代交代ボタン
    public void SelectBranch(int index);    // 分岐選択
    
    // ゲームプレイ計算
    bool IsOptimalEnvironment();
    bool IsDangerousEnvironment();
    bool IsHiddenBranchCondition();
    int CalculateEvolutionDirection();      // +1/-1/0
    void TriggerMutation();
}
```
