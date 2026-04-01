# 技術設計: Game097_PixelEvolution

## namespace
`Game097_PixelEvolution`

## スクリプト構成

### PixelEvolutionGameManager.cs
- ゲーム状態管理（Playing / Branching / Clear）
- 世代管理、進化パス記録
- UI への通知
- `[SerializeField]` で `EvolutionManager` と `PixelEvolutionUI` を参照

### EvolutionManager.cs
- コアメカニクス担当
- 生命体のピクセルパターン定義（10世代 × 8パス）
- Texture2D → Sprite.Create でランタイム描画
- 分岐ロジック（世代3, 5, 7で分岐）
- 世代交代アニメーション（スケールパルス）
- GameManager 参照: `[SerializeField]`

### PixelEvolutionUI.cs
- 世代テキスト更新
- 生命体名テキスト更新
- 世代交代ボタン表示/非表示
- 分岐選択パネル表示（2択テキスト設定）
- クリアパネル表示
- 全フィールド `[SerializeField, Tooltip]`

## 進化パスデータ構造

```
パス = int[3] (各分岐での選択 0 or 1)
- 分岐1(世代3): 水辺(0) / 陸地(1)
- 分岐2(世代5): 熱帯(0) / 寒冷(1)
- 分岐3(世代7): 捕食者(0) / 草食者(1)

最終形態 = パスの組み合わせで8種:
000: アクアドラゴン（水×熱×捕食）
001: サンゴクラゲ（水×熱×草食）
010: アイスシャーク（水×寒×捕食）
011: ユキウオ（水×寒×草食）
100: ファイアビースト（陸×熱×捕食）
101: サボテンゴーレム（陸×熱×草食）
110: フロストウルフ（陸×寒×捕食）
111: モスディア（陸×寒×草食）
```

## ピクセルパターン生成
- 世代1: 3×3 の単純な点
- 世代2: 5×5 の十字
- 以降: 奇数サイズで徐々に複雑化
- パターンは左右対称にして生き物らしさを出す
- Texture2D を FilterMode.Point で描画（ピクセル感）

## 色の決定
- ベースカラーは進化パスで決定
  - 水辺: 青系 (0.2, 0.5, 0.9)
  - 陸地: 緑系 (0.3, 0.7, 0.2)
  - 熱帯で暖色寄り、寒冷で寒色寄りにシフト
  - 捕食者で濃く、草食者で明るく

## 状態遷移
```
Start → Playing（世代1）
  → 世代交代ボタン → 世代+1
  → 世代3,5,7 → Branching（ボタン非表示、分岐パネル表示）
    → 選択 → Playing（次世代へ）
  → 世代10到達 → Clear
```

## SceneSetup 配線リスト

### PixelEvolutionGameManager
- `_evolutionManager`: EvolutionManager
- `_ui`: PixelEvolutionUI

### EvolutionManager
- `_gameManager`: PixelEvolutionGameManager

### PixelEvolutionUI
- `_generationText`: TextMeshProUGUI（世代テキスト）
- `_creatureNameText`: TextMeshProUGUI（生命体名）
- `_evolveButton`: Button（世代交代ボタン）
- `_branchPanel`: GameObject（分岐パネル）
- `_branchOptionAButton`: Button（選択肢A）
- `_branchOptionBButton`: Button（選択肢B）
- `_branchOptionAText`: TextMeshProUGUI
- `_branchOptionBText`: TextMeshProUGUI
- `_clearPanel`: GameObject
- `_clearText`: TextMeshProUGUI
- `_clearRetryButton`: Button
- `_menuButton`: Button

## 視覚構成
- カメラ: orthographic, size=5.5, 背景色 dark
- 背景: 宇宙っぽい暗い青グラデーション
- 生命体: 中央に大きく表示（localScale で拡大）
- ピクセル感を強調するため FilterMode.Point 使用
