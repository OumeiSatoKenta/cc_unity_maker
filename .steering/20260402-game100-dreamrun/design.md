# 技術設計: Game100_DreamRun

## namespace
`Game100_DreamRun`

## スクリプト構成

### DreamRunGameManager.cs
- ゲーム状態管理（Playing / Clear / GameOver）
- ライフ、距離、断片数管理
- `[SerializeField]` で `RunManager` と `DreamRunUI` を参照

### RunManager.cs
- コアメカニクス担当
- キャラクター移動（3レーン切替）
- ジャンプ処理
- 障害物・オーブのスポーンと移動
- 入力処理: タップ=ジャンプ、画面上半分/下半分タップでレーン移動
- 衝突判定（距離ベース）
- `[SerializeField]` で `_gameManager`, スプライト群を参照

### DreamRunUI.cs
- 距離・断片数・ライフ表示
- ストーリーテキスト表示（フェードイン/アウト）
- クリア/ゲームオーバーパネル
- 全フィールド `[SerializeField, Tooltip]`

## レーン・移動設計
- 3レーン: y = {2, 0, -2}
- キャラはx=-3に固定、レーン切替はy座標変更
- 入力: 画面上半分タップ → レーン上移動、画面下半分 → レーン下移動
- ジャンプ: ダブルタップ or 中央レーンでタップ → 短時間無敵

## 障害物・オーブスポーン
- 障害物: x=8からスポーン、左に移動（速度: 4〜7）
- スポーン間隔: 1.5秒〜0.8秒（距離で短縮）
- オーブ: 断片未取得時に一定間隔でスポーン
- 衝突判定: Vector2.Distance < 0.8f

## 背景演出
- 背景色が距離に応じて変化（紫→青→オレンジ→ピンク→白）
- 地面ラインの色も変化

## 状態遷移
```
Start → Playing
  → 障害物衝突 → ライフ-1、無敵時間
    → ライフ0 → GameOver
  → オーブ取得 → 断片+1、ストーリーテキスト表示
    → 5断片 → Clear
```

## SceneSetup 配線リスト

### DreamRunGameManager
- `_runManager`: RunManager
- `_ui`: DreamRunUI

### RunManager
- `_gameManager`: DreamRunGameManager
- `_characterSprite`: Sprite
- `_obstacleSprite`: Sprite
- `_orbSprite`: Sprite

### DreamRunUI
- `_distanceText`: TextMeshProUGUI
- `_fragmentText`: TextMeshProUGUI
- `_lifeText`: TextMeshProUGUI
- `_storyText`: TextMeshProUGUI
- `_clearPanel`: GameObject
- `_clearText`: TextMeshProUGUI
- `_gameOverPanel`: GameObject
- `_gameOverText`: TextMeshProUGUI
