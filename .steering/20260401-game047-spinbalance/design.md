# 設計書: Game047 SpinBalance

## namespace
`Game047_SpinBalance`

## スクリプト構成

### SpinBalanceGameManager.cs
- ゲーム状態管理（Playing / Clear / GameOver）
- SerializeField: `_balanceManager`, `_ui`, `_timeLimit(30f)`
- タイマー管理、クリア/ゲームオーバー判定
- コマ残数の追跡
- `OnPieceFallen()` — コマ落下時のコールバック
- `IsPlaying` プロパティ

### BalanceManager.cs
- コアメカニクス（入力処理・盤面回転・コマ生成）
- SerializeField: `_gameManager`, `_board(Transform)`, `_pieceSprite`, `_boardSprite`
- `GetComponentInParent` は使わない → `SerializeField` で参照
- 入力: `Mouse.current.leftButton` + `Mouse.current.position.ReadValue()` でドラッグ方向検出
- ドラッグの水平方向差分でボードをZ軸回転
- 回転速度: ドラッグ量 × 感度係数
- 初期コマ数: 3、一定間隔(3秒)で1つずつ追加（最大8個）
- コマ生成: Dynamic Rigidbody2D + CircleCollider2D
- FallZone トリガー検知用の Piece スクリプトを各コマに付与

### Piece.cs
- コマの個別制御
- `OnTriggerEnter2D` で FallZone 検知 → コールバック発火
- データ保持のみ（入力処理なし）

### SpinBalanceUI.cs
- SerializeField: `_timerText`, `_pieceCountText`, `_clearPanel`, `_clearScoreText`, `_clearRetryButton`, `_gameOverPanel`, `_gameOverRetryButton`, `_menuButton`
- `UpdateTimer(float)`, `UpdatePieceCount(int)`, `ShowClear(float)`, `ShowGameOver()`

## 状態遷移フロー

### クリアフロー
1. タイマーが0に到達
2. コマが1つ以上残っている
3. `_isPlaying = false`
4. `_ui.ShowClear(score)` — スコア = 残りコマ数 × 保持時間ボーナス

### ゲームオーバーフロー
1. コマが FallZone に接触
2. `OnPieceFallen()` でコマ数を減算
3. コマ数が0になったら `_isPlaying = false`
4. `_ui.ShowGameOver()`

## SceneSetup 配線対象フィールド

### SpinBalanceGameManager
- `_balanceManager` → BalanceManager コンポーネント
- `_ui` → SpinBalanceUI コンポーネント
- `_timeLimit` → 30f

### BalanceManager
- `_gameManager` → SpinBalanceGameManager コンポーネント
- `_board` → Board の Transform
- `_pieceSprite` → piece.png スプライト
- `_boardSprite` → board.png スプライト

### SpinBalanceUI
- `_timerText`, `_pieceCountText` → TextMeshProUGUI
- `_clearPanel`, `_clearScoreText`, `_clearRetryButton`
- `_gameOverPanel`, `_gameOverRetryButton`
- `_menuButton`

## 盤面設計
- Board: 横長の長方形（BoxCollider2D）、Kinematic Rigidbody2D
- 回転中心: Board の中央
- コマ: Circle（Dynamic Rigidbody2D, CircleCollider2D）
- FallZone: 画面下部に配置する大きなBoxCollider2D（isTrigger=true）
- 物理マテリアル: コマの摩擦を適度に設定

## 入力処理フロー
1. `Mouse.current.leftButton.wasPressedThisFrame` → ドラッグ開始位置記録
2. `Mouse.current.leftButton.isPressed` → 現在位置との水平差分で回転角度計算
3. `Mouse.current.leftButton.wasReleasedThisFrame` → ドラッグ終了
4. Board の `transform.rotation` を Quaternion.Euler(0, 0, angle) で更新
