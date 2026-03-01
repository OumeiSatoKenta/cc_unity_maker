<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/pytidb
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# カスタムエンベディング関数（PyTiDB）

PyTiDB は `BaseEmbeddingFunction` を実装することで**カスタムエンベディング関数**をサポートしています。以下のような場合に便利です:
- エンベディングをローカル（CPU/GPU）で実行したい場合
- PyTiDB に組み込まれていないモデルやプロバイダを使いたい場合
- バッチ処理、タイムアウト、前処理を完全に制御したい場合

このガイドでは、**BGE-M3** を例として一般的なパターンを解説します。

## ワークフローチェックリスト

- [ ] エンベディングモデルを選択（例: BGE-M3）
- [ ] `BaseEmbeddingFunction` のメソッドを実装
- [ ] `embed_fn.VectorField(source_field=...)` でテーブルを定義
- [ ] 行を挿入（自動エンベディングが実装経由で実行される）
- [ ] `table.search(...)` で検索

## フェーズ 1: 依存関係のインストール

必要なもの:
- `pytidb`
- エンベディングランタイムライブラリ（例: BGE-M3 用の `FlagEmbedding`）

例:

```bash
pip install pytidb FlagEmbedding
```

注意:
- 初回実行時にモデルファイルのダウンロードが発生する場合があります（ネットワーク接続が必要）。
- GPU の使用は環境とライブラリのサポート状況に依存します。

## フェーズ 2: カスタムエンベディング関数の実装

`templates/custom_embedding_function.py` を参照してください。

主な要件:
- `dimensions` を正しく設定すること（BGE-M3 は通常 1024）。
- 以下のメソッドを実装:
  - `get_query_embedding(...)`
  - `get_source_embedding(...)`
  - `get_source_embeddings(...)` （バッチ処理）

## フェーズ 3: 自動エンベディングと組み合わせて使用

テキストフィールドをソースとするベクトルフィールドを持つテーブルを定義します:

```py
class Document(TableModel):
    id: int = Field(primary_key=True)
    content: str = Field()
    content_vec: list[float] = embed_fn.VectorField(source_field="content")
```

これにより、挿入時に `content_vec` に対してエンベダーが自動的に呼び出されます。

## フェーズ 4: 最小限のサンプルを実行

`templates/custom_embedding.py` を参照してください。

## トラブルシューティング

- **次元の不一致**: `dimensions` がモデルの出力と一致していることを確認してください。
- **初回実行が遅い**: モデルのダウンロードとロードに数分かかることがあります。
- **OOM（GPU/CPU）**: CPU に切り替える、FP16 を無効にする、バッチサイズを小さくするなどの対策を行ってください。
