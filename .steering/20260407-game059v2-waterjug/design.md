# Design: Game059v2_WaterJug

## namespace
`Game059v2_WaterJug`

## スクリプト構成
| クラス | ファイル | 役割 |
|-------|---------|------|
| `WaterJugGameManager` | WaterJugGameManager.cs | ゲーム状態・スコア・ステージ管理 |
| `JugController` | JugController.cs | ジャグオブジェクト・水量・タップ検出 |
| `WaterJugUI` | WaterJugUI.cs | UI表示・パネル制御 |

## 盤面・ステージデータ設計

### StageConfig活用
`StageManager.StageConfig` のフィールド:
- `speedMultiplier`: 使用しない
- `countMultiplier`: ジャグ数の倍率（1.0=2個, 1.5=3個, 2.0=4個）
- `complexityFactor`: 難易度パラメータ（初期水量・複数目標フラグ）

### ステージ別パラメータ表
| Stage | jugCount | capacities | targets | maxMoves | initialAmounts | note |
|-------|---------|-----------|---------|---------|---------------|------|
| 1 | 2 | [3,5] | [4] (jug1) | 10 | [0,0] | チュートリアル |
| 2 | 2 | [3,7] | [5] (jug1) | 8 | [0,0] | 手数制限強め |
| 3 | 3 | [3,5,8] | [6] (jug2) | 10 | [0,0,0] | 3ジャグ |
| 4 | 3 | [4,7,10] | [3] (jug0) | 12 | [2,0,0] | 初期水量あり |
| 5 | 4 | [3,5,7,11] | [4,6] (jug0,jug1) | 15 | [0,0,0,0] | 2目標 |

## 入力処理フロー
- `WaterJugGameManager.Update()` でマウスクリック検出（InputSystem使用）
- `Physics2D.OverlapPoint` でどのジャグをタップしたか判定
- 蛇口/排水口ボタンはUI Button を使用（OnClick）
- 入力モード: Normal / Faucet(蛇口選択中) / Drain(排水選択中)

## JugController設計
```
JugController:
  - int capacity (ジャグ容量)
  - float currentAmount (現在の水量)
  - bool isTarget (目標ジャグか)
  - int targetAmount (目標量)
  - SpriteRenderer jugSprite
  - SpriteRenderer waterFill (水のビジュアル)
  - TextMeshProUGUI amountText
  - BoxCollider2D collider2D
  
  メソッド:
  - SetupJug(int cap, float initial, bool isTarget, int targetAmt)
  - AddWater(float amount) → 実際に追加できた量を返す
  - RemoveWater(float amount) → 実際に取り除いた量を返す
  - IsEmpty / IsFull プロパティ
  - CheckTargetAchieved() bool
  - SetHighlight(bool on) - タップ選択時のハイライト
  - SetWaterVisual() - currentAmountに基づきwaterFillのスケールを更新
```

## GameManager設計
```
WaterJugGameManager:
  [SerializeField] StageManager _stageManager
  [SerializeField] InstructionPanel _instructionPanel
  [SerializeField] WaterJugUI _ui
  [SerializeField] Transform _jugContainer
  
  ゲーム状態: Idle / Playing / StageClear / GameClear / GameOver
  
  入力状態: 
    enum InputMode { Normal, FaucetSelected, DrainSelected }
    JugController _selectedJug (注ぎ元として選択中)
    
  Undoスタック: Stack<UndoState> (各ジャグの水量スナップショット)
  struct UndoState { float[] amounts; }
  
  統計:
    int _moveCount (手数)
    int _undoCount (Undo使用回数)
    int _score
    int _totalScore
    int _comboMultiplier (連続ステージクリア)
    
  OnStageChanged(int stage):
    - 既存ジャグオブジェクト破棄
    - ステージ設定に基づきジャグ生成・配置（動的計算）
    - _moveCount = 0, _undoCount = 0
    - _ui.OnStageChanged(stage+1, maxMoves, targets)
    
  TryPourFrom(JugController from, JugController to):
    - Undoスタックにpush
    - _moveCount++
    - 水量計算・移し替え実行
    - 目標達成チェック
    - 手数超過チェック
    
  OnFaucetModeSelected():
    _inputMode = FaucetSelected
    
  OnDrainModeSelected():
    _inputMode = DrainSelected
    
  Undo():
    - スタックからポップして各ジャグ水量を復元
    - _undoCount++
    - _moveCount-- (最低0)
```

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;   // 6.0
float camWidth = camSize * Camera.main.aspect;  // ~3.37
float topMargin = 1.5f;    // HUD領域
float bottomMargin = 3.0f; // UI領域（ボタン類）
float availableHeight = camSize * 2f - topMargin - bottomMargin; // 7.5
// ジャグ数に応じてX軸に均等配置
float jugSpacing = camWidth * 2f / (jugCount + 1);
float jugY = -camSize + bottomMargin * 0.8f; // ジャグのY位置
```

## SceneSetup構成方針
- ジャグオブジェクトはGameManager下の`JugContainer`に動的生成（SceneSetupでは配置しない）
- SceneSetupでは: Camera, Background, GameManager, StageManager, WaterJugUI, InstructionPanel, Canvas, EventSystem を配置・配線
- ジャグのスプライトはResourcesから読み込み

## StageManager統合
- `OnStageChanged` 購読: `OnStageChanged(int stage)` でジャグ再生成・ステージパラメータ適用
- `OnAllStagesCleared` 購読: `OnAllStagesCleared()` で最終クリア処理
- ステージ遷移時: 既存ジャグを全破棄 → `GetCurrentStageConfig()` でパラメータ取得 → ジャグ再生成

## InstructionPanel内容
- title: "WaterJug"
- description: "ジャグを傾けて指定量の水を正確に計ろう！"
- controls: "ジャグをタップして注ぎ元→注ぎ先を選択。蛇口で満杯、排水口で空にできる"
- goal: "目標ジャグにぴったりの量の水を入れてステージクリア！"

## ビジュアルフィードバック設計
1. **水移動アニメーション**: `currentAmount` を Lerp で補間（0.3秒かけて変化）、`waterFill` スケールY更新
2. **正解演出**: ターゲットジャグが達成時、スケールパルス (1.0→1.3→1.0、0.25秒) + 色を金色に変化
3. **ジャグ選択ハイライト**: 選択中ジャグを黄色アウトライン（SpriteRendererのcolor変更）
4. **ミス/手数超過**: カメラシェイク（0.3秒） + 選択ジャグを赤フラッシュ
5. **ステージクリア**: 全ジャグが金色に輝くパルス演出

## スコアシステム
```
基本スコア = (制限手数 - 使用手数) × 100
最適解ボーナス: 最少手数でクリア → ×3.0
ノーアンドゥボーナス: Undo未使用 → ×1.5
連続クリアコンボ: ×1.1〜×1.5 (comboMultiplier)

計算式: baseScore × optimalBonus × noUndoBonus × comboMultiplier
```

## ステージ別新ルール表
| Stage | 新要素 |
|-------|-------|
| 1 | 基本ルール (蛇口・移し替え・排水の全操作) |
| 2 | 手数制限の厳格化（パズルとしての難易度導入）|
| 3 | 3個目のジャグ追加（3者間移し替えが必要）|
| 4 | 初期水量あり（逆算思考が必要）|
| 5 | 2つの目標量同時達成（複合チャレンジ）|

## 判断ポイントの実装設計
- **選択トリガー**: 注ぎ元タップ後、注ぎ先を選ぶ際に「どこに移すか」の判断
- **Undoコスト**: Undoを使うと `_undoCount++` でノーアンドゥボーナスが消える
- **手数超過ペナルティ**: `_moveCount >= maxMoves` でゲームオーバー
- **最適解報酬**: `_moveCount == minMovesForStage` で×3.0ボーナス（Stage1: 6手、Stage2: 6手、Stage3: 7手、Stage4: 8手、Stage5: 10手が想定最少）

## カラーパレット（casualカテゴリ）
- メイン: `#4CAF50` (緑)
- サブ: `#FFEB3B` (黄)
- アクセント: `#E8F5E9` (淡緑)
- 水の色: `#2196F3` (青) / `#00BCD4` (水色)
- ターゲットジャグ枠: `#FFEB3B` (黄色)
