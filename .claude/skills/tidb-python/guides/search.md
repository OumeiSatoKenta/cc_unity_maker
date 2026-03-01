<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/pytidb
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# 検索ガイド（ベクトル vs 全文 vs ハイブリッド）

## 使い分け

### ベクトル検索

意味的な類似度が必要な場合に使用します（RAG の取得、レコメンデーション、重複排除）。

一般的な構成:
- `VectorField(...)` カラム（テキストフィールドから自動エンベディングされることが多い）
- `table.search(query).limit(k)`
- メタデータフィルタ（JSON / スカラー）はオプション

開始テンプレート:
- `templates/auto_embedding.py`
- `templates/vector_search.py`

### 全文検索

キーワードマッチが重要な場合に使用します（タイトル検索、商品検索、多言語キーワードクエリ）。

一般的な構成:
- `FullTextField()` カラム
- `table.search("keywords", search_type="fulltext")`

注意:
- 全文検索の利用可否は TiDB Cloud のプランやリージョンによって制限される場合があります。

最小限のスニペット:

```py
from pytidb.schema import TableModel, Field, FullTextField

class Item(TableModel):
    __tablename__ = "items"
    id: int = Field(primary_key=True)
    title: str = FullTextField()

# results = table.search("Bluetooth headphones", search_type="fulltext").limit(10).to_list()
```

### ハイブリッド検索

以下の両方が必要な場合に使用します:
- 意味的類似度（ベクトル）+ キーワードマッチ（全文検索）
- 一つのランキングに融合（RRF / 重み付け）、オプションでリランキング

開始テンプレート:
- `templates/hybrid_search.py`

## よくある注意点

- **エンベディング次元の不一致**: 保存されたベクトルはフィールドの期待する次元と一致する必要があります。
- **インデックスの再現率 vs パフォーマンス**: フィルタ付きベクトル検索では、候補数（`num_candidate`）を調整し、プレフィルタ / ポストフィルタを意図的に選択してください。
- **全文検索のサポート**: 全文インデックスを作成できない場合、クラスタの機能の利用可否やリージョンを確認してください。
