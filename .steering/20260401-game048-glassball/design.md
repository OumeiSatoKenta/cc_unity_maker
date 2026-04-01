# 設計書: Game048 GlassBall

## namespace
`Game048_GlassBall`

## スクリプト構成

### GlassBallGameManager.cs
- ゲーム状態管理（Playing / Clear / GameOver）
- SerializeField: `_railManager`, `_ui`, `_ball(Transform)`
- 衝撃ゲージ管理（`_impactGauge` 0~1）
- `OnBallReachedGoal()`, `OnBallBroken()`, `OnBallFallen()`
- `AddImpact(float)` — 衝突時に衝撃を加算
- `IsPlaying` プロパティ

### RailManager.cs
- コアメカニクス（入力処理・レール描画・管理）
- SerializeField: `_gameManager`, `_railMaterial(Material)`, `_inkMax(100f)`
- 入力: `Mouse.current` でドラッグ → ワールド座標変換 → LineRenderer にポイント追加
- ドラッグ開始で新しい Rail GameObject 生成（LineRenderer + EdgeCollider2D）
- ドラッグ中にポイント追加（最小距離0.2f間隔）
- インク残量を消費（ポイント間距離分）
- インク0で描画停止

### GlassBall.cs
- ボールの個別制御（Dynamic Rigidbody2D）
- `OnCollisionEnter2D` で衝撃計算 → GameManager.AddImpact()
- `OnTriggerEnter2D` でゴール検知（gameObject.name == "Goal"）
- FallZone検知（gameObject.name == "FallZone"）

### GlassBallUI.cs
- SerializeField: `_impactSlider`, `_inkSlider`, `_clearPanel`, `_clearTimeText`, `_clearRetryButton`, `_gameOverPanel`, `_gameOverRetryButton`, `_menuButton`
- `UpdateImpact(float)`, `UpdateInk(float)`, `ShowClear(float)`, `ShowGameOver()`

## 状態遷移フロー

### クリアフロー
1. ボールが Goal トリガーに接触
2. `OnBallReachedGoal()` → `_isPlaying = false`, `_railManager.StopGame()`
3. `_ui.ShowClear(elapsedTime)`

### ゲームオーバーフロー
1. 衝撃ゲージ100%到達 or FallZone接触
2. `OnBallBroken()` or `OnBallFallen()` → `_isPlaying = false`, `_railManager.StopGame()`
3. `_ui.ShowGameOver()`

## SceneSetup 配線対象フィールド

### GlassBallGameManager
- `_railManager` → RailManager
- `_ui` → GlassBallUI
- `_ball` → Ball Transform

### RailManager
- `_gameManager` → GlassBallGameManager
- `_inkMax` → 100f

### GlassBallUI
- `_impactSlider`, `_inkSlider` → Slider
- `_clearPanel`, `_clearTimeText`, `_clearRetryButton`
- `_gameOverPanel`, `_gameOverRetryButton`
- `_menuButton`

## シーン構成
- Ball: 画面上部に配置（Dynamic Rigidbody2D, CircleCollider2D）
- Goal: 画面右下に配置（BoxCollider2D, isTrigger=true）
- FallZone: 画面下部（BoxCollider2D, isTrigger=true）
- 固定プラットフォーム: Ball開始位置に小さな足場
- レール: ランタイムで動的生成

## 入力処理フロー
1. `wasPressedThisFrame` → 新Rail生成、ドラッグ開始
2. `isPressed` → ワールド座標でポイント追加、インク消費
3. `wasReleasedThisFrame` → レール確定
