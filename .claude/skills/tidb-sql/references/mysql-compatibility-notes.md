---
title: MySQL to TiDB SQL Compatibility Notes (Common Breaks)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# MySQLからTiDBへのSQL互換性メモ（よくある問題）

MySQL SQLをTiDBに移行する際の「チェックリスト」として使用する。

## TiDBとMySQLの判別

```sql
SELECT VERSION();
```

返された文字列に `TiDB` が含まれている場合、TiDBに接続している。その文字列からTiDBバージョンを推定できる。

## サポートされていない、または利用できないことが多い機能（デフォルトでの生成を避ける）

ボーダーラインの機能に依存する前に、必ずTiDBのバージョンとデプロイ環境（TiDB Cloudのティア/リージョン vs セルフマネージド）を確認すること。

- ストアドプロシージャとストアドファンクション
- トリガー
- イベント（イベントスケジューラ）
- ユーザー定義関数（UDF）
- `SPATIAL` / `GEOMETRY` 関数、データ型、インデックス
- XML関数
- `XA` 構文（TiDBは内部的に2PCを使用するが、SQL経由でXAは公開していない）
- `CREATE TABLE ... AS SELECT ...`（CTAS）
- `CHECK TABLE`, `CHECKSUM TABLE`, `REPAIR TABLE`, `OPTIMIZE TABLE`
- `HANDLER`, `CREATE TABLESPACE`
- 一部の高度なクエリ構文はTiDBバージョンによってサポートされない場合がある（TiDBドキュメントでは `SKIP LOCKED`、横方向導出テーブル、`JOIN ... ON (subquery)` パターンなどの例がある）

## FULLTEXT: 意図の確認

- MySQLの `FULLTEXT` インデックスがTiDB上のすべての環境で動作するとは想定しない。
- ユーザーがキーワード検索を必要とする場合、デプロイ環境がサポートしていればTiDB全文検索を優先する（`skills/tidb-sql/references/full-text-search.md` を参照）。

## ビュー

- ビューは更新不可: ビューに対する `UPDATE/INSERT/DELETE` を生成しない。

## SELECT構文のエッジケース

- `SELECT ... INTO @variable` は出力しない（サポートされていない）。
- TiDBでは `SELECT ... GROUP BY expr` が `ORDER BY expr` を暗黙的に適用しない（MySQL 5.7の動作とは異なる）。順序が重要な場合は、明示的に `ORDER BY` を追加する。

## 組み込み関数（保守的に対応）

- TiDBはほとんどのMySQL組み込み関数をサポートするが、すべてではない。非自明な組み込み関数を使用するSQLを移植する場合、以下で利用可否を確認する:

```sql
SHOW BUILTINS;
```

## 文字セット/照合順序の落とし穴

- TiDBがサポートする文字セットは限定的。文字セット/照合順序に関するエラーが発生した場合、`utf8mb4` などの一般的にサポートされているセットを使用する（エキゾチックな文字セットは避ける）。
- デフォルトの文字セット/照合順序はMySQLと異なる場合がある: TiDBのデフォルトは通常 `utf8mb4` と `utf8mb4_bin`。大文字小文字を区別しない比較に依存する場合は、照合順序を明示的に設定する。

## 名前の大文字小文字の落とし穴

- TiDBは `lower_case_table_names = 2` のみサポート（大文字小文字を区別しないルックアップ動作）。名前が大文字小文字のみ異なる2つのオブジェクトに依存しないこと。

## AUTO_INCREMENTの落とし穴（TiDBでAUTO_RANDOMが一般的な理由）

- TiDBのAUTO_INCREMENT IDはグローバルに一意だが、ノード間で連番とは限らない。暗黙的なIDとカスタムの明示的な値の混在は避ける。
- `AUTO_INCREMENT` の削除は可能（`tidb_allow_remove_auto_inc` で制御）だが、後から追加することはサポートされていない。
- 主キーを定義しない場合、TiDBは `_tidb_rowid` を使用する。そのアロケータはAUTO_INCREMENTとMySQL利用者を驚かせる形で相互作用することがある。
- 書き込み負荷の高いスキーマを設計する場合、適合するならBIGINT主キーに `AUTO_RANDOM` を優先する（`skills/tidb-sql/references/auto-random.md` を参照）。

## DDL / スキーマ変更（保守的に対応、TiDBには追加の制約がある）

- 1つのステートメント内で同じカラム/インデックスを複数回参照する「複数変更」`ALTER TABLE` は避ける。
- 可能な場合、複数のTiDB固有のスキーマ変更を1つの `ALTER TABLE` にまとめない（分割する）。
- すべての型変更が `ALTER TABLE` でサポートされているわけではない（サポートされていない変更にはデータ移行/マイグレーションを計画する）。
- `ALGORITHM={INSTANT,INPLACE,COPY}` はアルゴリズム選択ではなくアサーションとして扱われる。
- クラスタード主キーの追加/削除はサポートされない場合がある。実際には主キーの変更は「新テーブル作成 + データ移行 + 入れ替え」として扱う。
- `USING HASH|BTREE|RTREE|FULLTEXT` などのインデックスタイプ修飾はパースされるが無視される。動作の変更に依存しないこと。

## パーティショニングに関する注意（TiDBがサポートしていることを確認するまで高度な操作は避ける）

- サポートされているパーティショニングタイプは `HASH`, `RANGE`, `LIST`, `KEY`。
- 一部のパーティションDDL操作は無視され、`SUBPARTITION` はサポートされていない。高度なパーティションDDLが必要な場合は、使用しているTiDBバージョンでのサポートを事前に確認する。

## オプティマイザ / プランの違い

- `optimizer_switch` は読み取り専用であり、TiDBのプランに影響しない。
- オプティマイザヒントはMySQLヒントのドロップイン置換ではない。重要なクエリはTiDB上で `EXPLAIN` を使用して検証する。
  - 構造化プランには `EXPLAIN FORMAT = "tidb_json"` を使用する（`skills/tidb-sql/references/explain.md` を参照）。

## タイムゾーンとタイムスタンプのデフォルト

- TiDBはシステムのタイムゾーンルールに基づく名前付きタイムゾーンをサポート。MySQLは名前付きタイムゾーンにタイムゾーンテーブルが必要な場合が多い。
- TiDBは `explicit_defaults_for_timestamp = ON` のみサポート。暗黙的なTIMESTAMPデフォルトに依存するMySQL 5.7時代のSQLを移植する場合、慎重にテストしデフォルトを明示的に設定する。

## 移植すべきでない非推奨のMySQL機能

- 浮動小数点型の精度指定子（固定精度が必要な場合は `DECIMAL` を優先）。
- `ZEROFILL`（代わりにアプリケーション層でパディングする）。
