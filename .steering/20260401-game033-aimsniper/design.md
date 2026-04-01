# Design: Game033_AimSniper

## Namespace

`Game033_AimSniper`

## スクリプト構成

### AimSniperGameManager.cs

- **役割**: ゲーム状態管理（Playing / Clear / GameOver）
- **参照取得**: `[SerializeField]` で SniperManager・AimSniperUI
- **主要フィールド**: `_totalTargets=5`, `_maxBullets=8`
- **主要メソッド**:
  - `Start()` → `_manager.StartStage()`
  - `AddHit()` → 命中カウント++, 全滅チェック→クリア
  - `OnShot()` → 弾数--, 弾切れ+残敵→GameOver
  - `RestartGame()` → シーンリロード
- **状態遷移**:
  - 全ターゲット撃破 → Clear（命中率から★計算）
  - 弾0 + 残敵あり → GameOver

### SniperManager.cs

- **役割**: コアメカニクス（スコープ操作・射撃・ターゲット管理）
- **参照取得**: `[SerializeField]` で gameManager, scopeTransform, crosshairSprite, targetSprite
- **入力処理**:
  - `Mouse.current.leftButton.isPressed` → スコープをマウス位置に追従（+Perlin揺れ）
  - `Mouse.current.leftButton.wasPressedThisFrame` → 射撃
  - 射撃判定: `Physics2D.OverlapCircleAll(scopeCenter, hitRadius)` で Target 検索
- **Perlin揺れ**:
  - `_swayAmplitude = 0.15f`
  - X,Y に `Mathf.PerlinNoise(Time.time * freq, seed)` で微小オフセット
- **敵配置**: 固定5体を `new GameObject` で生成

### Target.cs

- **役割**: ターゲットの左右往復移動・被弾処理
- **参照**: `Initialize(sprite, speed, minX, maxX, onKilled)` で設定
- **動作**: `Update` で左右往復（Mathf.PingPong）
- **被弾**: `Hit()` → `_isDead = true` → コールバック → Destroy

### AimSniperUI.cs

- **役割**: UI表示（弾数・命中率・クリア/ゲームオーバーパネル）
- **参照取得**: `[SerializeField]` で各TMP・Panel・Button
- **メソッド**: `UpdateBullets(int)`, `UpdateAccuracy(float)`, `ShowClear(int stars)`, `ShowGameOver()`

## 入力処理フロー

```
Mouse leftButton isPressed → スコープ追従（ドラッグ） + Perlin揺れ
Mouse leftButton wasPressedThisFrame → 射撃
  ↓ OverlapCircleAll(scopeCenter, 0.3f)
  ↓ Target.Hit()
  ↓ GameManager.AddHit() / OnShot()
```

## 画面レイアウト

```
Camera: orthographicSize=5.5, background=暗い屋外色
スコープ: スプライト（十字線+円）sortingOrder=10
ターゲット: 赤い的スプライト sortingOrder=2
背景: 森/野原風 sortingOrder=-10
```

## SceneSetup 配線一覧

**AimSniperGameManager:**
- `_manager` → SniperManager
- `_ui` → AimSniperUI
- `_totalTargets = 5`
- `_maxBullets = 8`

**SniperManager:**
- `_gameManager` → AimSniperGameManager
- `_scopeTransform` → Scope (Transform)
- `_crosshairSprite` → crosshair.png
- `_targetSprite` → target.png

**AimSniperUI:**
- `_bulletsText` → BulletsText (TMP)
- `_accuracyText` → AccuracyText (TMP)
- `_clearPanel` → ClearPanel
- `_clearStarText` → ClearStarText (TMP)
- `_clearRetryButton` → RetryButton (Button)
- `_gameOverPanel` → GameOverPanel
- `_gameOverRetryButton` → RetryButton (Button)
- `_menuButton` → MenuButton (Button)
