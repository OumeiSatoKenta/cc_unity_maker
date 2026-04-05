# Design: Game096_DualControl

## namespace
`Game096_DualControl`

## スクリプト構成

### DualControlGameManager.cs
- ゲーム状態管理（Playing/Clear/GameOver）
- タイマー管理
- `Start()`: DualManager.StartGame() を呼ぶ
- `Update()`: タイマー更新
- `UpdateStage(int left, int right, int goal)`: UI更新ブリッジ
- `OnGoalReached()`: クリア処理
- `OnCharacterHit()`: ゲームオーバー処理
- `RestartGame()`: シーンリロード
- SerializeField: `_dualManager`, `_ui`

### DualManager.cs（コアメカニクス）
- 入力処理・キャラクター移動・障害物管理を一元管理
- `StartGame()`, `StopGame()`
- `Update()`: HandleInput → UpdateSpawning → UpdateObstacles
- `HandleInput()`: Mouse + Touchscreen 両対応
- `ProcessScreenInput(Vector2 sp)`: 左半分=左キャラ、右半分=右キャラ移動
- `SpawnObstacle(int side)`: 指定レーンにランダム列で障害物生成
- `UpdateObstacles()`: 落下 + ヒット判定 + 回避カウント
- `CheckGoal()`: 両側 >= goalCount でクリア
- ObstacleData: 内部クラス (obj, side, col)
- 列X座標: LeftColX[]={-2.4,-1.6,-0.8}, RightColX[]={0.8,1.6,2.4}
- CharY=-3.5, SpawnY=6.0, DestroyY=-4.8, HitThreshold=0.45
- SerializeField: `_gameManager`, `_characterSprite`, `_obstacleSprite`, `_goalCount`, `_initialSpeed`, `_maxSpeed`, `_initialInterval`, `_minInterval`

### DualControlUI.cs
- `UpdateTimer(float t)`
- `UpdateStage(int left, int right, int goal)`
- `ShowClear(float time)`
- `ShowGameOver()`
- SerializeField: `_timerText`, `_stageLeftText`, `_stageRightText`, `_clearPanel`, `_clearScoreText`, `_clearRetryButton`, `_gameOverPanel`, `_gameOverRetryButton`, `_menuButton`

## 入力処理フロー
```
MouseButton押下 or Touch.press押下
 → ProcessScreenInput(screenPos)
   → sp.x < Screen.width/2: leftCol = floor(sp.x / (Screen.width/6))
   → sp.x >= Screen.width/2: rightCol = floor((sp.x - halfW) / (Screen.width/6))
   → UpdateCharVisuals()
```

## 障害物フロー
```
_leftSpawnTimer / _rightSpawnTimer -= deltaTime
 → 0以下になったらSpawnObstacle(side)
 → SpawnInterval -= 0.05f (min: _minInterval)
 → _obstacleSpeed += 0.002f/frame (max: _maxSpeed)

UpdateObstacles():
 → 各obstacleをVector3.down * speed * deltaTime で移動
 → y < CharY + HitThreshold かつ col == charCol → OnCharacterHit()
 → y < DestroyY → Destroy + avoid count++ → UpdateStage()
```

## クリア/ゲームオーバー状態遷移
- **クリア**: leftAvoided >= goalCount AND rightAvoided >= goalCount → _isActive=false → gm.OnGoalReached() → ShowClear()
- **ゲームオーバー**: hit検出 → _isActive=false → gm.OnCharacterHit() → ShowGameOver()

## SceneSetup の構成

### Setup096_DualControl.cs のフィールド配線リスト
```
DualManager:
  _gameManager        → GameManager (DualControlGameManager)
  _characterSprite    → player.png
  _obstacleSprite     → obstacle.png
  _goalCount          → 8
  _initialSpeed       → 2.5
  _maxSpeed           → 5.0
  _initialInterval    → 1.8
  _minInterval        → 0.7

DualControlUI:
  _timerText          → TimerText (TMP)
  _stageLeftText      → StageLeftText (TMP)
  _stageRightText     → StageRightText (TMP)
  _clearPanel         → ClearPanel
  _clearScoreText     → ClearPanel/CS (TMP)
  _clearRetryButton   → ClearPanel/RetryButton
  _gameOverPanel      → GameOverPanel
  _gameOverRetryButton → GameOverPanel/RetryButton
  _menuButton         → MenuButton

DualControlGameManager:
  _dualManager        → DualManager
  _ui                 → DualControlUI

Button events:
  ClearPanel/RetryButton.onClick → gm.RestartGame
  GameOverPanel/RetryButton.onClick → gm.RestartGame
  MenuButton → BackToMenuButton コンポーネント追加
```

## ビジュアル構成
- 背景: background.png (dark blue gradient)
- 左レーン列背景: tile.png × 3 (青半透明, x=-2.4,-1.6,-0.8, scale=(0.7,11,1))
- 右レーン列背景: tile.png × 3 (橙半透明, x=0.8,1.6,2.4, scale=(0.7,11,1))
- 中央仕切り: tile.png (白半透明, x=0, scale=(0.03,11,1))
- 左キャラ: player.png (青色 0.3,0.7,1.0, scale=0.9)
- 右キャラ: player.png (橙色 1.0,0.6,0.2, scale=0.9)
- 障害物(左): obstacle.png (赤 0.9,0.2,0.2)
- 障害物(右): obstacle.png (紫 0.7,0.2,0.9)

## スプライト一覧
| ファイル | サイズ | 内容 |
|---------|--------|------|
| background.png | 1080×1920 | 暗い青グラデーション背景 |
| player.png | 100×100 | 白い円（色はコードでtint） |
| obstacle.png | 100×100 | 白い四角（色はコードでtint） |
| tile.png | 100×100 | 白い塗りつぶし正方形（列BG用） |
