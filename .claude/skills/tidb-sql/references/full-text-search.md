---
title: TiDB Full-Text Search (SQL)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB全文検索 (SQL)

TiDBは、MySQLスタイルのキーワード検索ユースケースを置き換え可能な全文検索機能を提供する。

## 利用条件ゲート（重要）

全文検索の利用可否は、TiDB Cloudのティア/リージョンおよびTiDBのバージョンに依存する場合がある。依存する前に、必ずデプロイ環境の対応状況を確認すること。

## 全文検索インデックスの作成

テーブル作成時に作成する:

```sql
CREATE TABLE stock_items(
  id INT,
  title TEXT,
  FULLTEXT INDEX (title) WITH PARSER MULTILINGUAL
);
```

既存テーブルに追加する:

```sql
ALTER TABLE stock_items
  ADD FULLTEXT INDEX (title) WITH PARSER MULTILINGUAL
  ADD_COLUMNAR_REPLICA_ON_DEMAND;
```

パーサー:

- `STANDARD`: スペース/句読点区切りの言語に最適（主に英語）
- `MULTILINGUAL`: 多言語対応（CJKを含む）

注意: `ADD_COLUMNAR_REPLICA_ON_DEMAND` は全文検索インデックスを有効にするために公式サンプルで使用されている。TiDBのデプロイ環境がこの句を拒否する場合は、削除してデプロイ環境固有のガイダンスに従うこと。

## ランキング付きクエリ

`WHERE` と `ORDER BY` の両方で `FTS_MATCH_WORD(query, column)` を使用する:

```sql
SELECT *
FROM stock_items
WHERE FTS_MATCH_WORD('bluetooth earbuds', title)
ORDER BY FTS_MATCH_WORD('bluetooth earbuds', title) DESC
LIMIT 10;
```

マッチ数のカウント:

```sql
SELECT COUNT(*)
FROM stock_items
WHERE FTS_MATCH_WORD('bluetooth earbuds', title);
```

## 移行に関する注意（MySQL FULLTEXT）

MySQLの `FULLTEXT` の動作/利用可否がすべてのTiDBデプロイ環境で引き継がれるとは想定しないこと。
ユーザーが「FULLTEXTを使いたい」と言った場合、「MySQL FULLTEXTインデックス」と「TiDB全文検索機能」のどちらを意味しているか確認すること。
