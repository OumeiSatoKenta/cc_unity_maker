# Design: Game051v2_DrawBridge

## namespace

`Game051v2_DrawBridge`

## スクリプト構成

| クラス | ファイル | 役割 |
|-------|---------|-----|
| `DrawBridgeGameManager` | DrawBridgeGameManager.cs | ゲーム状態管理・StageManager統合・InstructionPanel |
| `DrawingManager` | DrawingManager.cs | 線描画・インク管理・物理EdgeCollider2D生成・入力処理 |
| `BallController` | BallController.cs | ボールのRigidbody2D制御・GOボタン起動 |
| `DrawBridgeUI` | DrawBridgeUI.cs | UI表示（インクゲージ・スコア・ステージ・各パネル） |

## クラス詳細

### DrawBridgeGameManager.cs

```
[SerializeField] StageManager _stageManager
[SerializeField] InstructionPanel _instructionPanel
[SerializeField] DrawingManager _drawingManager
[SerializeField] BallController _ballController
[SerializeField] DrawBridgeUI _ui

GameState: Idle / Drawing / Rolling / StageClear / AllClear / GameOver

Start():
  _instructionPanel.Show("051v2", "DrawBridge", description, controls, goal)
  _instructionPanel.OnDismissed += StartGame

StartGame():
  SetConfigs(5 stage configs)
  _stageManager.OnStageChanged += OnStageChanged
  _stageManager.OnAllStagesCleared += OnAllStagesCleared
  _stageManager.StartFromBeginning()

OnStageChanged(int stageIndex):
  State = Drawing
  config = _stageManager.GetCurrentStageConfig()
  _drawingManager.SetupStage(config, stageIndex+1)
  _ballController.ResetBall()
  _ui.UpdateStage(stageIndex+1, 5)

OnGoPressed():
  State = Rolling
  _drawingManager.FinalizeLines() (EdgeCollider2D確定)
  _ballController.Launch()

OnBallReachedGoal():
  State = StageClear
  Score += 残りインク × 100 × comboMultiplier
  _stageManager.CompleteCurrentStage()相当

OnBallFell():
  State = GameOver
  _ui.ShowGameOver()
```

### DrawingManager.cs

**入力処理**: `Mouse.current.leftButton` + `Mouse.current.position.ReadValue()`  
using `UnityEngine.InputSystem;`

```
float _inkRemaining (0.0 ~ 1.0)
float _inkCostPerUnit (ステージで変化)
List<LineRenderer> _drawnLines
List<GameObject> _lineColliders

Update():
  if wasPressedThisFrame → StartStroke()
  if isPressed → ContinueStroke() (インク消費, LineRenderer更新)
  if wasReleasedThisFrame → EndStroke() (EdgeCollider2D生成)

SetupStage(StageConfig, stageNumber):
  maxInk = stage1:1.0, stage2:0.8, stage3:0.8, stage4:0.6, stage5:0.6
  _inkRemaining = maxInk
  windForce = stage3以降: Vector2(-2, 0)
  breakableThreshold = stage4以降: 0.5f
  _isActive = true

ClearLines(): LineRenderer + Collider全削除

FinalizeLines(): EdgeCollider2Dを確定 (GOボタン押下時)

EdgeCollider2D生成:
  GameObject lineObj
  EdgeCollider2D ec
  Vector2[] points = strokePoints.ToArray()
  ec.points = points
  LineRenderer lr (視覚表現)
```

### レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;  // 5f
float camWidth = camSize * Camera.main.aspect; // ~2.8f (portrait)
// ゲーム領域: y=-1.5 ~ y=2.5 (崖が左右に配置)
// 左崖: x = -camWidth + 0.8f
// 右崖: x = camWidth - 0.8f
// ゴール: 右崖の先端 x = camWidth - 0.3f, y=崖上部
// ボール: 左崖の上 x = -camWidth + 0.8f, y = 崖高さ + 0.3f
// 描画エリア: 崖の間 y = -0.5f ~ 2.0f
// 下部マージン: 2.8u (UI用)
```

### BallController.cs

```
Rigidbody2D _rb
Vector2 _startPosition
bool _isLaunched

Launch(): _rb.simulated = true; _rb.gravityScale = 1f
ResetBall(): 位置リセット, _rb.simulated = false
OnTriggerEnter2D(Collider2D): ゴールタグ判定
OnCollisionEnter2D(Collision2D): 崖下落判定 (y < bottomLimit)
```

## StageManager統合

```csharp
_stageManager.SetConfigs(new StageManager.StageConfig[]
{
  // Stage1: 基本, inkMultiplier=1.0
  new StageManager.StageConfig { stageName="Stage 1", speedMultiplier=1.0f, countMultiplier=1, complexityFactor=0.0f },
  // Stage2: 岩障害物追加, inkMultiplier=0.8
  new StageManager.StageConfig { stageName="Stage 2", speedMultiplier=1.0f, countMultiplier=1, complexityFactor=0.2f },
  // Stage3: 風追加, inkMultiplier=0.8
  new StageManager.StageConfig { stageName="Stage 3", speedMultiplier=1.2f, countMultiplier=1, complexityFactor=0.4f },
  // Stage4: 壊れやすい橋, inkMultiplier=0.6
  new StageManager.StageConfig { stageName="Stage 4", speedMultiplier=1.2f, countMultiplier=2, complexityFactor=0.6f },
  // Stage5: 2段崖+コイン, inkMultiplier=0.6
  new StageManager.StageConfig { stageName="Stage 5", speedMultiplier=1.5f, countMultiplier=2, complexityFactor=0.8f },
});
```

complexityFactor: 0.0=追加要素なし, 0.2=岩, 0.4=岩+風, 0.6=岩+風+壊れ, 0.8=全要素+2段

## InstructionPanel内容

```csharp
_instructionPanel.Show(
  "051v2",
  "DrawBridge",
  "橋を描いてボールをゴールへ届けよう",
  "画面をドラッグして橋を描き、GOボタンでボールを転がそう。消しゴムで描き直しもできるよ",
  "ボールが対岸のゴールに到達したらクリア！残りインクが多いほど高得点"
)
```

## ビジュアルフィードバック設計

1. **ゴール到達時**: ゴールオブジェクトのスケールポップ (1.0 → 1.3 → 1.0, 0.3秒) + 色フラッシュ(金色)
2. **ボール落下時**: カメラシェイク (0.2秒, 振幅0.1) + SpriteRenderer赤フラッシュ
3. **描画中インク残少**: インクゲージの赤フラッシュ (残量20%以下)
4. **橋破壊時 (Stage4)**: LineRendererの色が赤→消滅のフェードアニメーション

## スコアシステム

- 基本スコア: `残りインク(%) × 100`
- 効率ボーナス: 描画総長が最短推定の1.5倍以内 → ×2.0
- スピードボーナス: 到達時間が基準(5秒)以下 → ×1.5
- コンボ乗算: 連続ステージクリア → ×1.1, ×1.2, ×1.3, ×1.4, ×1.5

## ステージ別新ルール表

| ステージ | 新要素 | complexityFactor |
|---------|-------|-----------------|
| Stage 1 | 基本ルール（直線橋チュートリアル） | 0.0 |
| Stage 2 | 岩障害物（EdgeCollider2Dで物理判定） | 0.2 |
| Stage 3 | 風（`Physics2D.gravity` + 横Forceをボールに追加） | 0.4 |
| Stage 4 | 重量制限（線の長さが閾値超えると線が消滅） | 0.6 |
| Stage 5 | 2段構成（中間島 + 2つの崖を渡る） | 0.8 |

## SceneSetup構成方針

`Setup051v2_DrawBridge.cs`

- MenuItem: `"Assets/Setup/051v2 DrawBridge"`
- カメラ: orthographic, size=5, bg=#E8F5E9 (淡緑)
- スプライト生成: Background, LeftCliff, RightCliff, Ball, Goal, Rock (Pillow)
- GameManager (DrawBridgeGameManager)
  - StageManager (子オブジェクト)
  - DrawingManager
  - BallController (Ball GameObjectに)
- Canvas (ScreenSpaceOverlay)
  - InstructionPanel (フルスクリーンオーバーレイ)
  - HUDPanel: Stage/Score/Ink表示
  - GOButton (下段右)
  - EraseButton (下段左)
  - StageClearPanel
  - GameOverPanel
  - BackToMenuButton (最下部)
- EventSystem (InputSystemUIInputModule)
- ゴールオブジェクト (tag="Goal")
- 崖オブジェクト (PolygonCollider2D)

## Buggy Code防止チェック

- `_isActive` ガード: DrawingManager.Update()で `if (!_isActive) return;`
- Texture2D/Sprite はOnDestroy()でDestroyする
- ボール判定: `collision.gameObject.CompareTag("Goal")` を使用
- レスポンシブ: 全座標を `Camera.main.orthographicSize` から計算
