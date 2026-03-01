<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/pytidb
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# トラブルシューティング

## 接続エラー

1) `scripts/validate_connection.py` を実行し、表示されるホスト / DB / ユーザーの情報を確認してください。
2) 環境変数を確認:
- `TIDB_HOST`, `TIDB_PORT`, `TIDB_USERNAME`, `TIDB_PASSWORD`, `TIDB_DATABASE`
- または単一の URL（`DATABASE_URL` / `TIDB_DATABASE_URL`）

よくある原因:
- ホスト / ポート / ユーザーの誤り
- パスワードのリセットが必要（TiDB Cloud コンソールで実施）
- 一部の TiDB Cloud Starter のパブリックエンドポイントでは TLS が必要（コンソールの「接続」ガイドを参照）

## 全文検索エラー

全文検索の利用は TiDB Cloud のプランやリージョンによって制限される場合があります。インデックスの作成や検索が失敗する場合:
- クラスタが全文検索をサポートしているか確認
- 該当する場合は、サポートされている別のリージョンやクラスタタイプを試す

## エンベディングのエラー / タイムアウト

- プロバイダ / モデル名と API キー（必要な場合）が正しいことを確認してください。
- 自動エンベディングで大量の挿入を行う場合、同時実行数とバッチサイズを減らしてください。

## 対話的環境での「テーブルが既に定義されています」エラー

対話的環境ではコードが繰り返し実行されることがあります。以下のパターンを使用してください:
- `__table_args__ = {"extend_existing": True}`
- `db.open_table(Model)` または `create_table(..., if_exists="skip")`
