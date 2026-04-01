# Design: Game032_SpinCutter

## Namespace

`Game032_SpinCutter`

## スクリプト構成

### SpinCutterGameManager.cs

- **役割**: ゲーム状態管理（Playing / Clear / GameOver）、スコア、★評価
- **参照取得**: `[SerializeField]` で SpinCutterManager・SpinCutterUI を受け取る
- **主要メソッド**:
  - `Start()` → `_manager.StartStage()` を呼ぶ
  - `AddKill()` → 撃破カウントを加算し、全滅チェック
  - `OnLaunchUsed()` → 発射回数を減らし、残り0かつ残敵があればGameOver
  - `RestartGame()` → シーンリロード
- **状態遷移**:
  - 初期: Playing
  - 全敵撃破 → Clear（`_ui.ShowClear(stars)`）
  - 発射0回+残敵あり → GameOver（`_ui.ShowGameOver()`）

### SpinCutterManager.cs

- **役割**: ステージ管理、刃発射制御、スライダー・ボタン入力処理
- **参照取得**: `[SerializeField]` で gameManager, bladePrefab, enemySprites, pivot, sliderRadius, sliderSpeed, launchButton, orbitPreview を受け取る
- **主要フィールド**:
  - `_remainingLaunches = 3`
  - `_enemies: List<Enemy>`
  - `_blade: BladeController`
  - `_isLaunched: bool`
- **主要メソッド**:
  - `StartStage()` → 敵生成、刃生成（非アクティブ）
  - `OnLaunchButtonPressed()` → `_blade.Launch(radius, speed)`, スライダー無効化
  - `OnBladeFinished()` → 発射完了コールバック: 刃をリセット、スライダー再有効化、発射回数−1
  - `OnEnemyKilled(Enemy)` → `_enemies.Remove()`, `_gameManager.AddKill()`
  - `UpdateOrbitPreview()` → スライダー値に応じて LineRenderer で円を描く
- **敵生成**: 固定座標8体を `new GameObject` で生成

### BladeController.cs

- **役割**: 刃の軌道運動と敵衝突検出
- **参照取得**: `Initialize(pivot, radius, speed, onFinished)` で受け取る
- **動作**:
  - `_isActive == false`: 非アクティブ（発射待ち）
  - `Launch(radius, speed)`: `_isActive = true`, 回転開始
  - `FixedUpdate`: `_angle += _speed * Time.fixedDeltaTime`, 位置更新
    ```
    transform.position = _pivot + new Vector2(cos(_angle), sin(_angle)) * _radius;
    ```
  - 敵検出: `Physics2D.OverlapCircleAll(transform.position, WorldRadius)` で Enemy を検索し `enemy.Hit()`
  - 1周半（`_angle >= Mathf.PI * 3`）で回転終了 → `_onFinished?.Invoke()`
- **衝突防止**: Enemy の `_isDead` フラグで多重 Hit を防ぐ

### Enemy.cs

- **役割**: 敵の状態管理（HP・死亡エフェクト）
- **参照**: `Initialize(sprite, onKilled)` で設定
- **主要フィールド**: `_isDead: bool`
- **メソッド**:
  - `Hit()`: `if (_isDead) return; _isDead = true; _onKilled?.Invoke(this); Destroy(gameObject)`

### SpinCutterUI.cs

- **役割**: UI表示（発射残数・撃破数・クリア/ゲームオーバーパネル）
- **参照取得**: `[SerializeField]` で各TextMeshProUGUI・Panel・Button を受け取る
- **メソッド**:
  - `UpdateLaunches(int remaining)`
  - `UpdateKills(int killed, int total)`
  - `ShowClear(int stars)`
  - `ShowGameOver()`

## 入力処理フロー

```
UI Slider (radius) ──→ SpinCutterManager._currentRadius
UI Slider (speed)  ──→ SpinCutterManager._currentSpeed
                        ↓ UpdateOrbitPreview()
Launch Button      ──→ SpinCutterManager.OnLaunchButtonPressed()
                        ↓ _blade.Launch(radius, speed)
```

- Slider の `onValueChanged` は SceneSetup の SerializedObject で配線せず、
  `Start()` 内で `_radiusSlider.onValueChanged.AddListener()` で登録する

## 盤面設計

```
Camera: orthographicSize=5.5, background=(0.05, 0.02, 0.12) [濃紺]
Pivot位置: (0, 0.5)
軌道プレビュー: LineRenderer, pointCount=64, loop=true
刃スプライト: blade.png (回転する歯車形状, 64×64px)
敵スプライト: enemy.png (赤い丸形状, 48×48px)

スライダーUI配置:
  - 半径スライダー: Canvas下部左 (anchoredPosition 0, 250)
  - 速度スライダー: Canvas下部右 (anchoredPosition 0, 180)
  - 発射ボタン: Canvas下部中央 (anchoredPosition 0, 80)
```

## SceneSetup 構成方針 (Setup032_SpinCutter.cs)

### 生成するオブジェクト一覧

| オブジェクト | 備考 |
|---|---|
| Camera | orthographicSize=5.5 |
| Background | SpriteRenderer, sortingOrder=-10 |
| Pivot | 空のGameObject at (0, 0.5) |
| OrbitPreview | LineRendererで円を描く |
| GameManager | SpinCutterGameManager |
| SpinCutterManager | GameManagerの子 |
| Canvas | CanvasScaler 1080x1920 |
| UI各要素 | スライダー2本・ボタン1本・テキスト3本・パネル2枚 |
| SpinCutterUI | GameManagerの子 |
| EventSystem | InputSystemUIInputModule |

### SerializeField 配線一覧 (SceneSetup で必ず設定する)

**SpinCutterGameManager:**
- `_manager` → SpinCutterManager
- `_ui` → SpinCutterUI
- `_totalEnemies = 8`
- `_maxLaunches = 3`

**SpinCutterManager:**
- `_gameManager` → SpinCutterGameManager
- `_pivot` → Pivot Transform
- `_radiusSlider` → Canvas/RadiusSlider
- `_speedSlider` → Canvas/SpeedSlider
- `_launchButton` → Canvas/LaunchButton
- `_orbitPreview` → OrbitPreview (LineRenderer)
- `_bladeSprite` → blade.png
- `_enemySprites[0]` → enemy.png

**SpinCutterUI:**
- `_launchesText` → LaunchesText
- `_killsText` → KillsText
- `_clearPanel` → ClearPanel
- `_clearStarText` → ClearStarText
- `_clearRetryButton` → RetryButton (inside ClearPanel)
- `_gameOverPanel` → GameOverPanel
- `_gameOverRetryButton` → RetryButton (inside GameOverPanel)
- `_menuButton` → MenuButton
