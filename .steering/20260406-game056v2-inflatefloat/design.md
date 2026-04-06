# Design: Game056v2 InflateFloat

## Namespace
`Game056v2_InflateFloat`

## スクリプト構成

### InflateFloatGameManager.cs
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] BalloonController _balloon`
- `[SerializeField] CourseManager _courseManager`
- `[SerializeField] InflateFloatUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed → StartGame
- StartGame(): StageManager購読 + StartFromBeginning()
- OnStageChanged(int stage): コース再生成、スコアリセット
- OnAllStagesCleared(): ゲームクリア
- 状態: Idle/Playing/StageClear/GameClear/GameOver

### BalloonController.cs
- 風船の物理挙動管理
- サイズ: minSize=0.4, maxSize=1.2, currentSize
- 膨張速度: inflateSpeed（ステージで変わる）
- 収縮速度: deflateSpeed
- 浮力: liftForce = currentSize * liftMultiplier
- 入力: Mouse.current.leftButton.isPressed (長押し判定)
- ドラッグ: Mouse.current.delta → 左右移動
- 100%でExplode()
- CircleCollider2D radius = currentSize * 0.5
- レスポンシブ配置: Camera.main.orthographicSize から初期Y位置計算
- `SetupStage(StageConfig config)`: inflateSpeed, deflateSpeed調整

### CourseManager.cs
- コース（障害物・コイン・ゴール）の生成・スクロール
- 障害物はプール管理（ObjectPool）
- スクロール速度: scrollSpeed（ステージで変わる）
- コインはObstacle近くに配置（リスクvsリワード）
- `SetupStage(StageConfig config, int stageNum)`: パラメータ適用
- Stage2以降: 上下移動障害物
- Stage3: 狭い隙間区間
- Stage4: WindZone発生
- Stage5: 針障害物追加

### InflateFloatUI.cs
- InflateGauge: 膨張率(0-100%)表示、100%近くで赤ゾーン
- StageText, ScoreText, DistanceSlider
- ComboText, StageClearPanel, GameClearPanel, GameOverPanel
- 「次のステージへ」「リトライ」「メニューへ」ボタン

## StageManager統合
- OnStageChanged購読でコース再構築
- ステージ別パラメータ:
  - Stage1: speed=2.0, inflateSpeed=1.0, gapSize=3.5, obstacles=5
  - Stage2: speed=2.5, inflateSpeed=1.1, gapSize=3.0, obstacles=8, movingObstacles=true
  - Stage3: speed=2.8, inflateSpeed=1.2, gapSize=2.2, obstacles=10, narrowSection=true
  - Stage4: speed=3.0, inflateSpeed=1.2, gapSize=2.8, obstacles=12, windZone=true
  - Stage5: speed=3.5, inflateSpeed=1.3, gapSize=2.0, obstacles=15, spikes=true

## InstructionPanel内容
- title: "InflateFloat"
- description: "風船を膨らませて障害物をかわしながら空を飛ぼう！"
- controls: "長押しで膨らます・離すと縮む・ドラッグで左右移動"
- goal: "ゴールフラッグまで無事に到達しよう！"

## ビジュアルフィードバック
1. コイン取得時: スケールパルス (1.0→1.4→1.0, 0.15秒) + 黄色フラッシュ
2. 破裂/ゲームオーバー時: 赤フラッシュ + カメラシェイク
3. ゴール到達時: 虹色グラデーション点滅 + スケールアップ

## スコアシステム
- 基本: コイン × 100
- スピードボーナス: 残り時間概念なし → 障害物通過ボーナス × 50
- パーフェクトボーナス: 全コイン取得 → ×2.0
- コンボ: 3個以上連続 → ×1.5

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5.5
float bottomMargin = 2.8f; // Canvas UI領域
float topMargin = 1.5f;    // HUD領域
// 風船初期Y: camSize * -0.5（中央やや下）
// コース: 全体をcamWidthで計算
```

## SceneSetup配線が必要なフィールド
- GameManager: stageManager, instructionPanel, balloon, courseManager, ui
- BalloonController: gameManager参照
- CourseManager: gameManager参照
- UI: gameManager参照、各テキスト/スライダー/パネル

## ステージ別新ルール表
- Stage1: 基本のみ（広い隙間・ゆっくり）
- Stage2: 障害物が上下にsin波移動
- Stage3: 特定区間でgapSizeが2.0まで縮小（要縮小操作）
- Stage4: 画面左右から横風が周期的に発生
- Stage5: 針障害物（接触で即破裂）+ 全要素複合
