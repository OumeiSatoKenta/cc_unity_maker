# Design: Game034_DropZone

## Namespace

`Game034_DropZone`

## スクリプト構成

### DropZoneGameManager.cs

- **参照取得**: `[SerializeField]` で DropManager・DropZoneUI
- **フィールド**: `_totalItems=20`, `_maxMisses=3`
- **メソッド**:
  - `Start()` → `_manager.StartGame()`
  - `OnCorrectDrop(int combo)` → スコア加算(100 + combo*50), 全完了→Clear
  - `OnWrongDrop()` → ミス++, 3回→GameOver
  - `RestartGame()` → シーンリロード
- **状態遷移**:
  - 全20個正解 → Clear
  - ミス3回 → GameOver

### DropManager.cs

- **役割**: アイテム生成・ドラッグ入力・ゾーン判定
- **参照取得**: `[SerializeField]` で gameManager, itemSprites(6種), zoneSprites(3種)
- **主要フィールド**:
  - `_currentItem: DropItem` — 現在落下中のアイテム
  - `_combo: int`
  - `_isDragging: bool`
  - `_spawnInterval: float` — 徐々に短くなる
- **入力処理**:
  - `Mouse.current.leftButton.wasPressedThisFrame` → ドラッグ開始
  - `Mouse.current.leftButton.isPressed` → X座標追従
  - `Mouse.current.leftButton.wasReleasedThisFrame` → リリース（即落下加速）
- **アイテム生成**: ランダムに6種から選択、上部中央にスポーン
- **ゾーン判定**: アイテムのY座標がゾーンY以下になった時、X座標で最も近いゾーンを判定

### DropItem.cs

- **役割**: アイテムのデータ保持・落下動作
- **フィールド**: `_category: int` (0=フルーツ, 1=ゴミ, 2=リサイクル), `_fallSpeed: float`
- **メソッド**: `Initialize(sprite, category, fallSpeed)`, `SetXPosition(float x)`
- **Update**: Y座標を `_fallSpeed * Time.deltaTime` で下げる

### DropZoneUI.cs

- **参照取得**: `[SerializeField]` で各TMP・Panel・Button
- **メソッド**: `UpdateScore(int)`, `UpdateMisses(int,int)`, `UpdateRemaining(int)`, `ShowClear()`, `ShowGameOver()`

## 入力処理フロー

```
Mouse press   → _isDragging = true (アイテムが画面内にいる場合)
Mouse held    → currentItem.SetXPosition(mouseWorldX) (clamped)
Mouse release → _isDragging = false, fallSpeed *= 3 (加速落下)
                → アイテムがゾーンYに到達 → 判定
```

## 画面レイアウト

```
Camera: orthographicSize=5.5
Y=5.5: アイテムスポーン位置
Y=-3.5: ゾーン（箱）3つ
背景: 倉庫風
```

## SceneSetup 配線一覧

**DropZoneGameManager:**
- `_manager` → DropManager
- `_ui` → DropZoneUI
- `_totalItems = 20`
- `_maxMisses = 3`

**DropManager:**
- `_gameManager` → DropZoneGameManager
- `_itemSprites[0..5]` → apple, banana, paper, can, bottle, glass
- `_zonePositions` — コード内定数で定義（SerializeField不要）

**DropZoneUI:**
- `_scoreText` → ScoreText
- `_missesText` → MissesText
- `_remainingText` → RemainingText
- `_clearPanel` → ClearPanel
- `_clearRetryButton` → RetryButton
- `_gameOverPanel` → GameOverPanel
- `_gameOverRetryButton` → RetryButton
- `_menuButton` → MenuButton
