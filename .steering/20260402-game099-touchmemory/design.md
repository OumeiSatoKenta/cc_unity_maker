# 技術設計: Game099_TouchMemory

## namespace
`Game099_TouchMemory`

## スクリプト構成

### TouchMemoryGameManager.cs
- ゲーム状態管理（Showing / Input / Clear / GameOver）
- ラウンド管理、パターン配列管理
- `[SerializeField]` で `MemoryManager` と `TouchMemoryUI` を参照

### MemoryManager.cs
- コアメカニクス担当
- 4パネルの生成・管理
- パターン再生（コルーチンで順番に光らせる）
- 入力処理: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint`
- 正解/不正解判定
- `[SerializeField]` で `_gameManager`, `_panelSprite` を参照

### TouchMemoryUI.cs
- ラウンドテキスト
- ステータステキスト（「見て覚えて！」「タップして！」）
- クリアパネル、ゲームオーバーパネル
- 全フィールド `[SerializeField, Tooltip]`

## パネル配置
```
(-1.5, 1.5) 赤   (1.5, 1.5) 青
(-1.5, -1.5) 緑  (1.5, -1.5) 黄
```
各パネル: SpriteRenderer + BoxCollider2D、サイズ2.5x2.5

## パターンロジック
- `List<int> _pattern` に 0-3 のインデックスを蓄積
- ラウンド開始時に `Random.Range(0,4)` を1つ追加
- 提示: コルーチンで0.5秒間隔で光らせる（明度UP→戻す）
- 入力: `_inputIndex` で追跡、正解なら次、全正解なら次ラウンド

## 状態遷移
```
Start → Showing（ラウンド1、パターン提示）
  → 提示完了 → Input（プレイヤー入力待ち）
  → 正解タップ → _inputIndex++
    → 全正解 → ラウンド+1
      → ラウンド11 → Clear
      → else → Showing（次パターン提示）
  → 不正解タップ → GameOver
```

## SceneSetup 配線リスト

### TouchMemoryGameManager
- `_memoryManager`: MemoryManager
- `_ui`: TouchMemoryUI

### MemoryManager
- `_gameManager`: TouchMemoryGameManager
- `_panelSprite`: Sprite

### TouchMemoryUI
- `_roundText`: TextMeshProUGUI
- `_statusText`: TextMeshProUGUI
- `_clearPanel`: GameObject
- `_clearText`: TextMeshProUGUI
- `_clearRetryButton`: Button（不要 → SceneSetupで直接配線）
- `_gameOverPanel`: GameObject
- `_gameOverText`: TextMeshProUGUI
- `_gameOverRetryButton`: Button（不要 → SceneSetupで直接配線）
