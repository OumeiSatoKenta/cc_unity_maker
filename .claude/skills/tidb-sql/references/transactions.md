---
title: TiDB Transactions (Optimistic vs Pessimistic)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDBトランザクション（オプティミスティック vs ペシミスティック）

TiDBはペシミスティックとオプティミスティックの両方のトランザクションモードをサポートしている。
MySQL/InnoDBのユーザーは通常、デフォルトでペシミスティックな動作を期待する。

## モードの選択

- 競合が頻繁に発生する場合、またはアプリケーションがコミット失敗時に安全にリトライできない場合は **ペシミスティック** を優先する。
- 書き込み-書き込み競合がまれで、アプリケーションがコミット失敗を処理できる場合は **オプティミスティック** を検討する。

## デフォルトモードの設定（クラスタ全体）

```sql
SET GLOBAL tidb_txn_mode = 'pessimistic';
-- または:
SET GLOBAL tidb_txn_mode = 'optimistic';
```

## トランザクション単位でのモード強制

```sql
BEGIN PESSIMISTIC;
-- ... DML ...
COMMIT;
```

```sql
BEGIN OPTIMISTIC;
-- ... DML ...
COMMIT;
```

## アプリケーション層のルール（オプティミスティック）

オプティミスティックトランザクション向けのSQLを生成する場合、呼び出し元/アプリケーションが `COMMIT` エラーを処理し、トランザクション全体を安全にリトライすることを要求する（冪等性 + リトライループ）。
