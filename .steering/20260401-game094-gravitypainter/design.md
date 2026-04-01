# Design: Game094_GravityPainter

## namespace
`Game094_GravityPainter`

## スクリプト構成

### GravityPainterGameManager.cs
- **役割**: ゲーム状態管理
- **GameManager参照**: SerializeField
- フィールド:
  - `[SerializeField] PaintManager _paintManager`
  - `[SerializeField] GravityPainterUI _ui`
  - `[SerializeField] int _maxMoves = 8`
  - `[SerializeField] float _clearThreshold = 0.6f`
  - `int _movesUsed`, `bool _isPlaying`
- メソッド:
  - `Start()`: 初期化、PaintManager.StartGame()、UI更新
  - `OnPaintDropped()`: moves++、UI更新、残0で判定
  - `CheckResult()`: 一致率取得 → クリア or GameOver
  - `RestartGame()`: SceneManager.LoadScene

### PaintManager.cs
- **役割**: キャンバス管理・重力シミュレーション・入力なし
- **GameManager参照**: SerializeField
- フィールド:
  - `[SerializeField] GravityPainterGameManager _gameManager`
  - `[SerializeField] Sprite _cellSprite`
  - `int _gridSize = 8`
  - `Color _selectedColor`
  - `Color[] _canvasColors` (64要素)
  - `Color[] _targetColors` (64要素)
  - `GameObject[] _cellObjects` (64要素)
  - `SpriteRenderer[] _cellRenderers` (64要素)
  - `bool _isActive`
- 色定数: `static readonly Color[] PaintColors = {白, 赤, 青, 緑, 黄}`
  - 0=White, 1=Red(0.9,0.2,0.2), 2=Blue(0.2,0.4,0.9), 3=Green(0.2,0.8,0.3), 4=Yellow(0.95,0.85,0.1)
- メソッド:
  - `StartGame()`: グリッド生成、お手本設定、プレビュー生成
  - `StopGame()`: _isActive=false
  - `SelectColor(int idx)`: _selectedColor設定（0=赤,1=青,2=緑,3=黄）
  - `DropPaint(int direction)`: 方向に塗る(0=上,1=下,2=左,3=右)
    - dir=0(上): 各列c, r=0から下に探索→最初の白セルを塗る
    - dir=1(下): 各列c, r=7から上に探索→最初の白セルを塗る
    - dir=2(左): 各行r, c=0から右に探索→最初の白セルを塗る
    - dir=3(右): 各行r, c=7から左に探索→最初の白セルを塗る
    - 実際に塗られたセルがあれば _gameManager.OnPaintDropped()
  - `CalculateMatchRate()`: float (0-1)
  - `SetupTarget(int patternIdx)`: お手本設定
  - `CreatePreview()`: 右上に4x4でお手本を縮小表示

### GravityPainterUI.cs
- **役割**: UI表示管理
- フィールド:
  - `[SerializeField] TextMeshProUGUI _matchText`
  - `[SerializeField] TextMeshProUGUI _movesText`
  - `[SerializeField] GameObject _clearPanel`
  - `[SerializeField] TextMeshProUGUI _clearScoreText`
  - `[SerializeField] Button _clearRetryButton`
  - `[SerializeField] GameObject _gameOverPanel`
  - `[SerializeField] TextMeshProUGUI _gameOverScoreText`
  - `[SerializeField] Button _gameOverRetryButton`
  - `[SerializeField] Button _menuButton`
- メソッド:
  - `UpdateMatch(float rate)`: "一致率: 00%"
  - `UpdateMoves(int remaining)`: "残り 8 回"
  - `ShowClear(float rate, int stars)`: clearPanel表示、★評価
  - `ShowGameOver(float rate)`: gameOverPanel表示

## SceneSetup 配線一覧 (Setup094_GravityPainter.cs)
- `cm._gameManager` → gm
- `cm._cellSprite` → cellSprite
- `ui._matchText` → matchText
- `ui._movesText` → movesText
- `ui._clearPanel` → clearPanel
- `ui._clearScoreText` → clearScoreText
- `ui._clearRetryButton` → clearRetryBtn
- `ui._gameOverPanel` → goPanel
- `ui._gameOverScoreText` → goScoreText
- `ui._gameOverRetryButton` → goRetryBtn
- `ui._menuButton` → menuBtn
- `gm._paintManager` → cm
- `gm._ui` → ui
- ボタンリスナー:
  - 色ボタン4個: cm.SelectColor(0/1/2/3) → AddIntPersistentListener
  - 重力ボタン4個: cm.DropPaint(0/1/2/3) → AddIntPersistentListener
  - clearRetryBtn: gm.RestartGame
  - goRetryBtn: gm.RestartGame

## グリッド配置
- セルサイズ: 0.55f
- 8x8グリッド中央配置: startX = -_gridSize/2 * cellSize + cellSize/2
- グリッドY位置: 0f (画面中央)

## カメラ設定
- backgroundColor: (0.1, 0.1, 0.12)
- orthographicSize: 6f

## UI レイアウト (1080x1920)
- 上部左: matchText (サイズ200x40, アンカー上左)
- 上部右: movesText (サイズ200x40, アンカー上右)
- 左側: 色ボタン4個 縦並び (サイズ100x100, アンカー左中央, x=-470)
- 下部中央: 重力ボタン十字配置
  - 上ボタン: "↑", y=180
  - 下ボタン: "↓", y=60
  - 左ボタン: "←", x=-75
  - 右ボタン: "→", x=75
- メニューボタン: 下部左
- クリア/GameOverパネル: 中央
