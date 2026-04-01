# 設計書: Game049 CloudHop

## namespace
`Game049_CloudHop`

## スクリプト構成

### CloudHopGameManager.cs
- ゲーム状態管理（Playing / Clear / GameOver）
- SerializeField: `_hopManager`, `_ui`, `_player(Transform)`, `_goalHeight(50f)`
- 高度追跡（プレイヤーY座標ベース）
- カメラ追従（プレイヤーが上昇したらカメラも追従）

### HopManager.cs
- コアメカニクス（入力処理・雲管理・プレイヤー制御）
- SerializeField: `_gameManager`, `_player(Rigidbody2D)`, `_cloudSprite`, `_jumpForce(10f)`
- 入力: タップでジャンプ（接地時のみ）、ドラッグで左右移動
- 雲の自動生成: プレイヤー上方に一定間隔で雲をスポーン
- 雲の消滅: 生成後一定時間（3秒）で消える
- FallZone（カメラ下方）でゲームオーバー検知

### Cloud.cs
- 雲の個別制御
- 生成後一定時間で点滅→消滅
- Kinematic Rigidbody2D + BoxCollider2D（上面のみ衝突）
- PlatformEffector2D で片方向衝突

### CloudHopUI.cs
- SerializeField: `_heightText`, `_scoreText`, `_clearPanel`, `_clearScoreText`, `_clearRetryButton`, `_gameOverPanel`, `_gameOverRetryButton`, `_menuButton`

## 状態遷移フロー

### クリアフロー
1. プレイヤーY座標がgoalHeight以上に到達
2. `_isPlaying = false`, `_hopManager.StopGame()`
3. `_ui.ShowClear(score)`

### ゲームオーバーフロー
1. プレイヤーがカメラ下端より下に落下
2. `_isPlaying = false`, `_hopManager.StopGame()`
3. `_ui.ShowGameOver()`

## SceneSetup 配線対象フィールド
### CloudHopGameManager
- `_hopManager`, `_ui`, `_player`, `_goalHeight(50f)`

### HopManager
- `_gameManager`, `_player(Rigidbody2D)`, `_cloudSprite`, `_jumpForce(10f)`

### CloudHopUI
- `_heightText`, `_scoreText`, `_clearPanel`, `_clearScoreText`, `_clearRetryButton`, `_gameOverPanel`, `_gameOverRetryButton`, `_menuButton`

## 入力処理フロー
1. `wasPressedThisFrame` → 接地判定 → ジャンプ力付与
2. `isPressed` → ドラッグ水平差分でプレイヤー左右移動
3. 接地判定: `Physics2D.Raycast` で下方チェック
