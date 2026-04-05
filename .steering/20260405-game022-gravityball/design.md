# Design: Game022v2_GravityBall

## namespace
`Game022v2_GravityBall`

## スクリプト構成

### GravityBallGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] GravityBallController _controller
- [SerializeField] GravityBallUI _ui
- コンボ管理、スコア管理
- StageManager.OnStageChanged 購読でコントローラーに SetupStage(config) を渡す
- StageManager.OnAllStagesCleared で全クリア処理

### GravityBallController.cs
- ボール物理（重力方向・速度・位置更新）
- 横スクロール（Camera や背景のスクロール）
- 障害物生成・プール管理
- 入力処理（Mouse.current.leftButton.wasPressedThisFrame）
- ステージ設定（SetupStage(StageConfig config)）
- カメラ座標からワールド座標を動的計算（固定値禁止）

### GravityBallUI.cs
- Stage X/5 表示
- スコア表示（距離 + ボーナス）
- コンボ倍率表示
- ステージクリアパネル
- 全クリアパネル
- ゲームオーバーパネル

## 盤面・ステージデータ設計

ステージ設定（StageManager.StageConfig から利用）:
- speedMultiplier → スクロール速度係数
- countMultiplier → 障害物密度係数
- complexityFactor → 新要素の出現率・強度

| Stage | speedMultiplier | countMultiplier | complexityFactor | 目標距離 |
|-------|----------------|----------------|-----------------|---------|
| 1     | 1.0            | 1.0            | 0.0             | 100m    |
| 2     | 1.25           | 1.2            | 0.3             | 200m    |
| 3     | 1.4            | 1.3            | 0.6             | 300m    |
| 4     | 1.6            | 1.5            | 0.8             | 400m    |
| 5     | 1.83           | 1.8            | 1.0             | 500m    |

## 入力処理フロー

```
Update()
  ├─ if _isActive && Mouse.current.leftButton.wasPressedThisFrame
  │    └─ FlipGravity()  → _gravityDir *= -1
  ├─ Ball位置更新（_gravityVelocity += _gravityDir * gravityAccel * dt）
  ├─ スクロール更新（_distanceTraveled += scrollSpeed * dt）
  ├─ 障害物スポーン判定
  └─ 衝突判定（Bounds重複チェック）
```

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;         // 5.0
float camWidth = camSize * Camera.main.aspect;        // ~2.8
float topMargin = 1.2f;    // HUD用
float bottomMargin = 2.8f; // Canvas UIボタン用
float gameAreaTop = camSize - topMargin;              // 3.8
float gameAreaBottom = -camSize + bottomMargin;       // -2.2
float gameAreaHeight = gameAreaTop - gameAreaBottom;  // 6.0

// ボール初期Y = 0
// 天井 = gameAreaTop, 床 = gameAreaBottom
// 障害物隙間幅 = gapWidth（ステージにより変動）
```

## SceneSetup の構成方針

- MenuItem: `Assets/Setup/022v2 GravityBall`
- カメラ背景色: ダーク青（#0A0D1E）
- スプライト: `Resources/Sprites/Game022v2_GravityBall/`
  - Background.png, Ball.png, Obstacle.png, GravityZone.png, TriggerObstacle.png
- GameManager → GravityBallController, GravityBallUI, StageManager, InstructionPanel を全配線
- CanvasScaler: 1080x1920, ScaleWithScreenSize

## StageManager統合

- OnStageChanged(int stage) 購読
- stage 0-based → SetupStage(config) でコントローラーを再構成
- 距離が目標に達したら GameManager.OnStageClear() → _stageManager.CompleteCurrentStage()
- OnAllStagesCleared で全クリアパネル表示

## InstructionPanel内容

- title: "GravityBall"
- description: "重力を反転させながら障害物をよけよう"
- controls: "画面をタップして重力を上下に反転させる"
- goal: "各ステージの目標距離に到達しよう"

## ビジュアルフィードバック設計

1. **重力反転時のボールパルス**: localScale 1.0 → 1.3 → 1.0（0.15秒）
2. **衝突時の赤フラッシュ**: SpriteRenderer.color を赤(1,0.2,0.2) → 白へフェード（0.3秒）
3. **狭隙間ボーナス時のコンボテキスト**: スケール 0→1.2→1.0 + 黄色点滅

## スコアシステム

- 基本: 1m走行 = 1pt
- 狭隙間通過ボーナス: +50pt × コンボ倍率（隙間幅が基準の70%以下）
- パーフェクト通過ボーナス: +100pt × コンボ倍率（中央通過判定）
- コンボ倍率: 3連続x1.5 / 6連続x2.0 / 10連続x3.0
- ミス（衝突）でコンボリセット

## ステージ別新ルール表

- Stage 1: 基本ルール（固定障害物のみ、隙間3.0u）
- Stage 2: 移動障害物追加（上下スライド、30%の確率で出現）
- Stage 3: 重力加速ゾーン（特定区間で重力1.5倍、背景色変化で予告）
- Stage 4: 連続障害物ペア（2〜3個が密集して出現）
- Stage 5: 反転トリガー障害物（触れると強制重力反転、赤色で区別）

## 判断ポイントの実装設計

- **反転タイミング**: 障害物が画面右端から出現し、到達まで約1〜2秒。プレイヤーは画面中央〜左で反転判断
- **狭隙間ボーナス**: 狭い隙間（通常の70%）を通ると +50pt × 倍率。広い隙間は安全だがボーナスなし
- **重力ゾーン（Stage3）**: 背景色変化で予告 → プレイヤーは通常より早めに反転する必要あり

## 衝突判定設計

Physics2D を使わず、Bounds重複チェック（軽量）:
```csharp
var ballBounds = new Bounds(ball.position, ballSize);
foreach (var obs in activeObstacles)
{
    if (ballBounds.Intersects(obs.bounds)) { OnCollision(); break; }
}
// 天井・床チェック
if (ball.y > gameAreaTop || ball.y < gameAreaBottom) OnCollision();
```
