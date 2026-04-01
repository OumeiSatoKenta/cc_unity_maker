# Design: Game095_SilentWorld

## namespace
`Game095_SilentWorld`

## スクリプト構成

### SilentWorldGameManager.cs
- **役割**: ゲーム状態管理
- **参照**: SerializeField
- フィールド:
  - `[SerializeField] WorldManager _worldManager`
  - `[SerializeField] SilentWorldUI _ui`
  - `[SerializeField] int _totalItems = 3`
  - `int _collectedItems`, `float _elapsedTime`, `bool _isPlaying`
- メソッド:
  - `Start()`: 初期化、WorldManager.StartGame()
  - `Update()`: タイマー更新
  - `OnItemCollected()`: _collectedItems++、UI更新
  - `OnExitReached()`: 全収集済みならクリア、未収集なら何もしない(WorldManagerが制御)
  - `OnTrapHit()`: ゲームオーバー
  - `RestartGame()`

### WorldManager.cs
- **役割**: グリッド管理・プレイヤー移動・入力処理
- **参照**: SerializeField
- フィールド:
  - `[SerializeField] SilentWorldGameManager _gameManager`
  - `[SerializeField] Sprite _cellSprite`, `_playerSprite`, `_itemSprite`, `_trapSprite`, `_exitSprite`
  - `const int GridSize = 5`, `const float CellSize = 0.9f`
  - `enum CellType { Empty, Item, Trap, Exit }`
  - `CellType[] _grid` (25要素)
  - `int _playerRow, _playerCol`
  - `SpriteRenderer[] _cellRenderers` (25)
  - `GameObject _playerObj`
  - `Camera _mainCamera`, `bool _isActive`
  - 色定数: Normal/Hint色
- メソッド:
  - `StartGame()`: グリッド生成、配置設定
  - `StopGame()`: _isActive=false
  - `UseHint()`: アイテム/トラップ/出口を1秒間光らせる (コルーチン)
  - `Update()`: Mouse.current.leftButton.wasPressedThisFrame → Physics2D.OverlapPoint → 移動判定
  - `TryMove(int r, int c)`: 隣接チェック → 移動実行 → セル種別判定
  - `SetupGrid()`: 5x5生成、アイテム/トラップをランダム配置
  - `IEnumerator ShowHint()`: 1秒間光らせてから戻す

### SilentWorldUI.cs
- **役割**: UI表示
- フィールド:
  - `[SerializeField] TextMeshProUGUI _timerText`
  - `[SerializeField] TextMeshProUGUI _itemText`
  - `[SerializeField] TextMeshProUGUI _hintText`
  - `[SerializeField] GameObject _clearPanel`
  - `[SerializeField] TextMeshProUGUI _clearScoreText`
  - `[SerializeField] Button _clearRetryButton`
  - `[SerializeField] GameObject _gameOverPanel`
  - `[SerializeField] Button _gameOverRetryButton`
  - `[SerializeField] Button _menuButton`
- メソッド:
  - `UpdateTimer(float t)`, `UpdateItems(int c, int total)`, `UpdateHint(int remaining)`
  - `ShowClear(float time)`, `ShowGameOver()`

## SceneSetup 配線一覧
- `wm._gameManager` → gm
- `wm._cellSprite`, `_playerSprite`, `_itemSprite`, `_trapSprite`, `_exitSprite` → 各スプライト
- `ui._timerText`, `_itemText`, `_hintText`, `_clearPanel`, `_clearScoreText`, `_clearRetryButton`, `_gameOverPanel`, `_gameOverRetryButton`, `_menuButton` → 各UI要素
- `gm._worldManager` → wm, `gm._ui` → ui
- ボタンリスナー: ヒントボタン→wm.UseHint, clearRetry→gm.RestartGame, goRetry→gm.RestartGame

## グリッド配置
- CellSize = 0.9f
- 中央配置: startX = -GridSize/2 * CellSize + CellSize/2
- グリッドY: 0.5f（画面やや上）

## 色定義
- 通常マス: (0.18, 0.18, 0.22)
- プレイヤー位置: (0.9, 0.9, 1.0)
- 出口: (0.2, 0.6, 0.3)（常時表示）
- アイテム(ヒント): (0.95, 0.85, 0.1)
- トラップ(ヒント): (0.85, 0.2, 0.2)
- 収集済みアイテスマス: (0.3, 0.4, 0.35)

## カメラ設定
- backgroundColor: (0.08, 0.08, 0.1)
- orthographicSize: 5.5f

## UI レイアウト (1080x1920)
- 上部左: timerText (アンカー上左)
- 上部右: itemText (アンカー上右)
- 上部右下: hintText
- 下部: ヒントボタン(中央), メニューボタン(左下)
- クリア/GameOverパネル: 中央
