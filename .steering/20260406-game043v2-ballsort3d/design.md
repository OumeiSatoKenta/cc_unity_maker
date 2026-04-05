# Design: Game043v2_BallSort3D

## namespace
`Game043v2_BallSort3D`

## スクリプト構成

### BallSort3DGameManager.cs
- **役割**: ゲーム全体の状態管理、StageManager・InstructionPanel統合
- **フィールド**:
  - `[SerializeField] StageManager _stageManager`
  - `[SerializeField] InstructionPanel _instructionPanel`
  - `[SerializeField] BallSort3DMechanic _mechanic`
  - `[SerializeField] BallSort3DUI _ui`
- **状態**: WaitingInstruction / Playing / StageClear / Clear / GameOver
- **Start()**: InstructionPanel.Show → OnDismissed += StartGame
- **StartGame()**: StageManager.OnStageChanged += OnStageChanged, StartFromBeginning()
- **OnStageChanged(int stage)**: mechanic.SetupStage(stage), ui更新
- **OnAllStagesCleared()**: Clear状態、FinalClearPanel表示

### BallSort3DMechanic.cs
- **役割**: チューブ・ボール管理、入力処理、ゲームロジック
- **主要フィールド**:
  - `[SerializeField] BallSort3DGameManager _gameManager`
  - `[SerializeField] Sprite _spriteTube, _spriteBall_R, _spriteBall_G, _spriteBall_B, _spriteBall_Y, _spriteBall_M`
  - `[SerializeField] Sprite _spriteLidClosed, _spriteLidOpen, _spriteLock`
- **チューブデータ**:
  ```csharp
  class TubeData { List<BallData> balls; bool hasCover; bool isSelected; }
  class BallData { int colorId; bool isLocked; int lockCount; }
  ```
- **入力**: Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint
- **GetCurrentStageConfig() 対応**:
  - stageIndex 0: 4チューブ/2色、ロック無し
  - stageIndex 1: 5チューブ/3色、ロックボール有り
  - stageIndex 2: 6チューブ/4色、蓋付きチューブ有り
  - stageIndex 3: 7チューブ/4色、回転チューブ有り
  - stageIndex 4: 8チューブ/5色、タイマー制限+全要素
- **レスポンシブ配置**:
  ```csharp
  float camSize = Camera.main.orthographicSize; // 5f
  float camWidth = camSize * Camera.main.aspect;
  float topMargin = 1.5f;  // HUD用
  float bottomMargin = 2.8f; // ボタン用
  float availableHeight = camSize * 2f - topMargin - bottomMargin;
  float tubeSpacing = camWidth * 2f / (tubeCount + 1);
  float tubeHeight = Mathf.Min(availableHeight, 4.0f);
  ```
- **チューブの配置**: X軸均等割り、Y軸中央
- **ボールの配置**: チューブ内で下から積み上げ（Y座標計算）
- **ビジュアルフィードバック**:
  - 選択時: スケールパルス 1.0 → 1.3 → 1.0（0.15秒）、黄色ハイライト
  - 正解配置: ポップアニメーション 1.0 → 1.4 → 1.0（0.2秒）+ 緑フラッシュ
  - 間違い: 赤フラッシュ + カメラシェイク（0.15秒）
  - ステージクリア時: 全ボール順番にスケールパルス（波紋エフェクト）
- **コンボシステム**:
  - 同色連続配置でコンボカウント増加
  - コンボ乗算: 1.0x → 1.5x → 2.0x（3コンボ以上）
  - Undo使用でコンボリセット
- **Undoスタック**: List<TubeState[]> で手順履歴を管理
- **デッドロック検出**: 全移動可能手を列挙して0なら検出

### BallSort3DUI.cs
- **役割**: UI表示管理
- **表示要素**:
  - `Text ステージ表示「Stage X / 5」`
  - `Text スコア表示（コンボ乗算込み）`
  - `Text 手数表示`
  - `Text コンボ表示`
  - `Text タイマー表示（Stage5のみ）`
  - `Panel ステージクリアパネル`（「次のステージへ」ボタン）
  - `Panel 最終クリアパネル`（全5ステージ完了）
  - `Panel ゲームオーバーパネル`

## スコアシステム
- クリア基本点: 1000点
- 最少手数ボーナス: (最少手数 ÷ 実手数) × 2000点
- コンボ乗算: ×1.5
- Undo未使用ボーナス: 500点
- ステージクリアボーナス: ステージ番号 × 200点

## ステージ別パラメータ表

| Stage | tubeCount | colorCount | hasLock | hasCover | hasRotation | hasTimer | timerSec |
|-------|-----------|-----------|---------|---------|-------------|---------|----------|
| 1 (idx=0) | 4 | 2 | false | false | false | false | - |
| 2 (idx=1) | 5 | 3 | true | false | false | false | - |
| 3 (idx=2) | 6 | 4 | false | true | false | false | - |
| 4 (idx=3) | 7 | 4 | false | false | true | false | - |
| 5 (idx=4) | 8 | 5 | true | true | true | true | 120 |

StageManager.StageConfig.complexityFactor で stage index を取得して判定。

## SceneSetup構成方針 (Setup043v2_BallSort3D.cs)
- MenuItem: `"Assets/Setup/043v2 BallSort3D"`
- Camera: orthographic, size=5, 暗背景(#0A1628)
- Background: Sprites/Game043v2_BallSort3D/background.png
- GameManager > StageManager（子）
- GameManager > BallSort3DMechanic（子）
- Canvas > 全UI（InstructionPanel, StageClearPanel, FinalClearPanel, GameOverPanel, HUD）
- Sprite読み込み: File.WriteAllBytes → AssetDatabase.ImportAsset → LoadAssetAtPath<Sprite>

## InstructionPanel 内容
- **gameId**: "043v2"
- **title**: "BallSort3D"
- **description**: "色付きボールを同じ色のチューブに揃えよう"
- **controls**: "チューブをタップしてボールを移動。同色か空のチューブに入れられる"
- **goal**: "全チューブを同じ色のボールだけにしたらクリア"

## ビジュアルフィードバック設計
1. **選択時**: 選択したボールSpriteRenderer のスケールパルス(1.0→1.3→1.0, 0.15秒) + 黄色Outline風カラー変化
2. **正解配置**: 配置されたボールのポップアニメ(1.0→1.4→1.0, 0.2秒) + 緑フラッシュ
3. **誤操作**: 赤フラッシュ(0.1秒) + カメラshake(0.15秒, 0.1振幅)
4. **ステージクリア**: 全ボール順番にスケールパルス（100ms間隔）

## カメラ配置（レスポンシブ）
- orthographicSize = 5
- チューブ配置領域: Y=-0.5〜3.5（HUD上部、ボタン下部を避ける）
- 各チューブ: X軸均等配置

## Buggy Code 防止
- `_isActive` フラグで Update() ガード
- Texture2D / Sprite は OnDestroy() でDestroyImmediate
- Tag比較には gameObject.name ではなくcolorId（int）を使用
