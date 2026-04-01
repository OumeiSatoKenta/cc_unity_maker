# 技術設計: Game098_InfiniteLoop

## namespace
`Game098_InfiniteLoop`

## スクリプト構成

### InfiniteLoopGameManager.cs
- ゲーム状態管理（Playing / Clear）
- ループカウント、ステージ（変化発見数）管理
- UI通知
- `[SerializeField]` で `LoopManager` と `InfiniteLoopUI` を参照

### LoopManager.cs
- コアメカニクス担当
- 6つのオブジェクトの生成と管理
- ステージごとの変化適用ロジック
- 入力処理（タップ判定）: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint`
- 正解/不正解の判定とコールバック
- `[SerializeField]` で `_gameManager`, 各スプライトを参照

### InfiniteLoopUI.cs
- ループカウンター表示
- ステージ進行テキスト
- クリアパネル表示
- 全フィールド `[SerializeField, Tooltip]`

## オブジェクト配置（部屋レイアウト）
6つのオブジェクトを2行3列のグリッドに配置:
```
(-2.5, 1.5)  (0, 1.5)  (2.5, 1.5)   ← 時計, 花瓶, 絵画
(-2.5, -1.5) (0, -1.5) (2.5, -1.5)  ← 本棚, 窓, ランプ
```

各オブジェクトは SpriteRenderer + BoxCollider2D を持つ。

## 変化ロジック
- ステージ1: オブジェクト0（時計）の色を白→赤に変更
- ステージ2: オブジェクト1（花瓶）の位置を(0, 1.5)→(0.3, 1.7)に移動
- ステージ3: オブジェクト2（絵画）のスケールXを反転（-1）
- ステージ4: オブジェクト3（本棚）を非表示にし代わりに空の本棚を表示
- ステージ5: オブジェクト4（窓）の色を白→青に変更

## 状態遷移
```
Start → Playing（ステージ1, ループ1）
  → タップ正解 → ステージ+1、フェードアウト→フェードイン（ループ演出）
  → タップ不正解 → ループ+1（ペナルティ）、画面シェイク
  → ステージ5正解 → Clear
```

## SceneSetup 配線リスト

### InfiniteLoopGameManager
- `_loopManager`: LoopManager
- `_ui`: InfiniteLoopUI

### LoopManager
- `_gameManager`: InfiniteLoopGameManager
- `_objectSprites`: Sprite[]（6つのオブジェクトスプライト）
- `_altSprites`: Sprite[]（変化後スプライト、本棚用）

### InfiniteLoopUI
- `_loopCountText`: TextMeshProUGUI
- `_stageText`: TextMeshProUGUI
- `_hintText`: TextMeshProUGUI
- `_clearPanel`: GameObject
- `_clearText`: TextMeshProUGUI
- `_clearRetryButton`: Button
