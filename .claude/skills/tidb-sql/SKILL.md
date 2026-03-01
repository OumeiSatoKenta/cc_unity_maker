---
name: tidb-sql
description: "Write, review, and adapt SQL for TiDB with correct handling of TiDB-vs-MySQL differences (VECTOR type + vector indexes/functions, full-text search, AUTO_RANDOM, optimistic/pessimistic transactions, foreign keys, views, DDL limitations, and unsupported MySQL features like procedures/triggers/events/GEOMETRY/SPATIAL). Use when generating SQL that must run on TiDB, migrating MySQL SQL to TiDB, or debugging TiDB SQL compatibility errors. TiDB固有SQL・MySQL互換性・パフォーマンス診断。"
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB SQL（MySQL互換重視）

目的: デフォルトでTiDB上で正しく動作するSQLを生成し、「MySQLでは動くがTiDBでは壊れる」構文を回避する。

## ワークフロー（毎回実行）

1. ターゲットエンジンとバージョンを特定する:
   - `SELECT VERSION();` を実行する
   - 結果に `TiDB` が含まれている場合、TiDBとして扱い、バージョンを解析する（Vector / Foreign Keyなどの機能ゲートに必要）。
   - TiDB Cloudに接続する場合、クライアントで証明書とID検証を含むSSLを有効にする（`skills/tidb-sql/references/tidb-cloud-ssl.md` を参照）。
2. リクエストが依存する場合、2つの簡単な確認を行う:
   - 「TiFlashはありますか？」（ベクトルインデックスに必要）
   - 「サポート対象リージョンのTiDB Cloud Starter/Essentialですか？」（全文検索の利用可否は限定的）
3. TiDB安全なデフォルトでSQLを生成する:
   - サポートされていないMySQL機能（プロシージャ/トリガー/イベント/UDF/GEOMETRY/SPATIALなど）を避ける
   - ビューは読み取り専用として扱う
   - 主キーの変更はマイグレーション/リビルド作業として扱う
4. ユーザーがMySQL SQLを提供した場合、互換性チェックを行う:
   - サポートされていない機能をTiDBの代替手段に置き換える
   - 動作の違いとバージョン要件を明示的に伝える
5. SQLが遅い、または予期せず失敗する場合、TiDBネイティブの診断を使用する:
   - 構造化されたプランとオペレータツリーには `EXPLAIN FORMAT = "tidb_json"` を使用する。
   - `estRows` と `actRows` を比較するには `EXPLAIN ANALYZE` を使用する（クエリが実行される）。
   - プランが正しくない場合、`ANALYZE TABLE ...` で統計情報を更新することを検討する。

## 重要な差異（常に念頭に置く）

- **ベクトル**: TiDBは `VECTOR` / `VECTOR(D)` 型およびベクトル関数/インデックスをサポート。MySQLはサポートしない。
- **GEOMETRY/SPATIALなし**: `GEOMETRY`、空間関数、`SPATIAL` インデックスは使用しない。
- **プロシージャ / 関数 / トリガー / イベントなし**: ロジックはアプリケーション層または外部スケジューラに移行する。
- **全文検索（TiDB機能）**: 利用可能な場合はTiDBの全文検索SQLを使用する。MySQL の `FULLTEXT` がどこでも動作するとは想定しない。
- **ビューは読み取り専用**: ビューに対する `UPDATE/INSERT/DELETE` は不可。
- **外部キー**: TiDB v6.6.0以降でサポート。それ以前のバージョンではFK制約に依存しない。
- **主キーの変更は制限あり**: 主キーの変更は「新テーブル作成 + データ移行 + 入れ替え」を前提とする。
- **AUTO_RANDOM**: 書き込みホットスポットの回避に適切な場合、`AUTO_INCREMENT` より `AUTO_RANDOM` を優先する。
- **トランザクション**: TiDBはペシミスティックモードとオプティミスティックモードをサポート。オプティミスティックモードの `COMMIT` 失敗はアプリケーションロジックで処理する。

## リファレンス（このスキル内）

- `skills/tidb-sql/references/vector.md` - VECTOR型、関数、ベクトルインデックスDDL、クエリパターン。
- `skills/tidb-sql/references/full-text-search.md` - 全文検索SQLパターンと利用上の注意点。
- `skills/tidb-sql/references/auto-random.md` - `AUTO_RANDOM` のルール、DDLパターン、制約事項。
- `skills/tidb-sql/references/transactions.md` - ペシミスティック vs オプティミスティックモードとセッション/グローバル設定。
- `skills/tidb-sql/references/mysql-compatibility-notes.md` - SQLが壊れやすい「MySQL vs TiDB」のその他の差異。
- `skills/tidb-sql/references/explain.md` - EXPLAIN / EXPLAIN ANALYZEの使い方、tidb_jsonおよびdotフォーマット。
- `skills/tidb-sql/references/flashback.md` - FLASHBACK TABLE/DATABASEおよびFLASHBACK CLUSTERのリカバリ手順。
- `skills/tidb-sql/references/tidb-cloud-ssl.md` - TiDB Cloud SSL検証の要件とクライアントフラグ。
