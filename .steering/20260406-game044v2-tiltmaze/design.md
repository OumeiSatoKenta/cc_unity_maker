# Design: Game044v2 TiltMaze

## namespace
Game044v2_TiltMaze

## スクリプト構成

### TiltMazeGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- スコア・ライフ管理
- StageManager + InstructionPanel統合
- SerializeField: _stageManager, _instructionPanel, _mechanic, _ui

### TiltMazeMechanic.cs
- MazeRoot回転（z軸、maxAngle制限）
- ドラッグ入力処理: Mouse.current.leftButton + Mouse.current.position.ReadValue()
- 長押し判定: _holdTimer でブレーキ発動
- ボール物理: Rigidbody2D、ステージごとの摩擦変更
- 穴判定・ゴール判定・コイン判定（OnTriggerEnter2D）
- 動く壁制御（ステージ3以降）
- ワープホール制御（ステージ4以降）
- 氷エリア制御（ステージ5）
- SetupStage(StageManager.StageConfig config) でステージ再構築
- カメラのorthographicSizeから動的に迷路サイズ計算

### TiltMazeUI.cs
- HUD: タイマー・ライフ・コイン数・ブレーキゲージ・ステージ表示
- パネル: StageClearPanel, FinalClearPanel, GameOverPanel

## StageManager統合

- OnStageChanged(int stage) → TiltMazeMechanic.SetupStage() + UI更新
- OnAllStagesCleared() → 全クリア表示
- StageConfig.speedMultiplier → 制限時間（1.0=60秒、1.5=40秒相当に利用）
- StageConfig.complexityFactor → maxAngle・摩擦パラメータとして利用

## InstructionPanel内容

- title: "TiltMaze"
- description: "迷路を傾けてボールをゴールへ転がそう"
- controls: "画面をドラッグして迷路を傾ける。長押しでブレーキ"
- goal: "穴に落ちずにボールをゴールまで届けよう"

## ビジュアルフィードバック設計

1. **コイン取得時**: コインをスケールパルス(1.0→1.5→0)してからDestroyするアニメーション
2. **穴落下時**: ボールを赤フラッシュ(SpriteRenderer.color)してから元位置リスポーン
3. **ゴール到達時**: ゴールオブジェクトを緑フラッシュ + ボールスケールアップ
4. **ブレーキ中**: ボールのSpriteRenderer.colorを青に変更

## スコアシステム

- 基本: クリア500 + 残り時間×100 + コイン×200
- 全コイン収集: 最終スコア×2倍
- ノーミス: +1000
- ステージコンボ乗算: Stage1=1.0, 2=1.5, 3=2.0, 4=2.5, 5=3.0

## ステージ別新ルール表

- Stage 1: 単純迷路（壁3本, 穴2個）、maxAngle=30
- Stage 2: コイン3枚追加（全取り×2倍ボーナス）、maxAngle=35
- Stage 3: 動く壁1本（往復1.5秒周期）、穴4個
- Stage 4: ワープホール1ペア（入口→出口テレポート）、穴4個
- Stage 5: 氷エリア（摩擦0.05）+ 全要素複合、穴5個

## 判断ポイント実装設計

- コイン存在時にコインに向かうと迷路の曲がり角が増加→穴リスク上昇
- 動く壁はMoveWall.cs内でTime.timeで周期計算、壁位置を毎フレーム更新
- ブレーキゲージ: 100%から使用、1秒長押しで20%消費、停止で3秒で10%回復

## SceneSetup構成

- Assets/Editor/SceneSetup/Setup044v2_TiltMaze.cs
- MenuItem: "Assets/Setup/044v2 TiltMaze"
- カメラ orthographicSize=5
- MazeRoot（空GameObject、ゲームエリア中央）
- Ball prefab配置（MazeRootの子）
- Wall, Hole, Goal オブジェクト（MazeRootの子、ステージ1レイアウト）
- GameManager（StageManager子, TiltMazeMechanic子, TiltMazeUI子）
- Canvas（InstructionPanel, HUD, 各種パネル）
- レスポンシブ: ゲームエリア = camSize*2 - topMargin(1.2) - bottomMargin(2.8)

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize; // 5.0
float camWidth = camSize * Camera.main.aspect; // ~2.81 (16:9)
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 2.8f; // Canvas UIボタン領域
float mazeSize = (camSize * 2f) - topMargin - bottomMargin; // ~6.0
// MazeRoot配置: Y = camSize - topMargin - mazeSize/2 = 0.8
```

## スプライト一覧

- background.png (1024x1024, 濃い緑グラデーション)
- ball.png (128x128, 赤系グラデーション+ハイライト)
- wall.png (256x64, 茶系グラデーション+アウトライン)
- hole.png (128x128, 黒渦巻き風)
- goal.png (128x128, 緑フラグ風)
- coin.png (128x128, 金色グラデーション)
- ice_floor.png (256x128, 水色半透明)
- warp_in.png (128x128, 紫渦巻き)
- warp_out.png (128x128, 黄渦巻き)
