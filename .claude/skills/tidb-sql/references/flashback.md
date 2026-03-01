---
title: TiDB Flashback (Recover from Drops/Truncates)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB Flashback（DROP/TRUNCATEからの復旧）

誤った `DROP` / `TRUNCATE` からの復旧には、GCが過去のバージョンを完全に削除する前であればflashbackを使用できる。

重要: FlashbackはGCに制約される。デフォルトの `tidb_gc_life_time` は短いことが多い（例: 10分）。迅速に対応すること。

## 復旧を試みる前に

1. TiDBに接続していることを確認する（MySQLではない）: `SELECT VERSION();`
2. GCセーフポイントを確認する:

```sql
SELECT * FROM mysql.tidb WHERE variable_name = 'tikv_gc_safe_point';
```

DROP/TRUNCATEがセーフポイントより前に発生した場合、flashbackでは復旧できない。

## FLASHBACK TABLE（TiDB v4.0以降）

削除されたテーブルの復旧:

```sql
FLASHBACK TABLE t;
```

TRUNCATEされたテーブルの復旧:

- `TRUNCATE` 後はテーブル名がまだ存在するため、新しい名前で復旧する必要がある:

```sql
FLASHBACK TABLE t TO t_recovered;
```

注意事項:

- 同じ削除済みテーブルを複数回復旧することはできない（復旧されたテーブルは同じテーブルIDを再利用する）。

## FLASHBACK DATABASE（TiDB v6.4.0以降）

削除されたデータベースの復旧:

```sql
FLASHBACK DATABASE test;
```

リネームして復旧:

```sql
FLASHBACK DATABASE test TO test_recovered;
```

注意事項:

- 同じデータベースを複数回復旧することはできない（スキーマIDはグローバルに一意である必要がある）。

## FLASHBACK CLUSTER TO TIMESTAMP / TSO（影響範囲大）

クラスタ全体を特定の時点に復元する場合に使用する。

利用条件 / 安全ゲート:

- TiDB Cloud Starter/Essentialクラスタには適用不可。
- `SUPER` 権限が必要。
- GCライフタイム内であること。
- 未来のタイムスタンプ/TSOは指定しないこと。
- 実行中、TiDBは関連する接続を切断し、読み取り/書き込みをブロックする。開始後はキャンセルできない。
- 古いデータを新しいタイムスタンプで書き込む（現在のデータは削除されない）。十分なストレージ容量を確保すること。
- TiCDCを使用している場合、メタデータのロールバックはレプリケーションされない。changefeedを一時停止し、実行後にスキーマを照合する計画を立てること。

構文:

```sql
FLASHBACK CLUSTER TO TIMESTAMP '2022-09-21 16:02:50';
FLASHBACK CLUSTER TO TSO 445494839813079041;
```

正確な時点のTSOを取得する:

```sql
SELECT @@tidb_current_ts;
```
