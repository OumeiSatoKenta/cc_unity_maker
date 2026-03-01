---
title: TiDB EXPLAIN and EXPLAIN ANALYZE (Troubleshooting)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB EXPLAINとEXPLAIN ANALYZE（トラブルシューティング）

クエリを実行せずにプランを確認するには `EXPLAIN` を、実行してランタイム統計を取得するには `EXPLAIN ANALYZE` を使用する。

## 基本ルール

- `EXPLAIN` の結果が「明らかにおかしい」場合、関係するテーブルに対して `ANALYZE TABLE <table>` を実行し、再確認する。
- オペレータツリーをプログラムで検査したい場合は `EXPLAIN FORMAT = "tidb_json"` を優先する。
- 視覚的なオペレータグラフが必要な場合は `EXPLAIN FORMAT = "dot"`（Graphviz）を優先する。
- `EXPLAIN ANALYZE` はステートメントを実行する。本番環境や重いクエリには注意して使用すること。
- TiDBはMySQLの `FORMAT=JSON` や `FORMAT=TREE` をサポートしない。代わりに `FORMAT="tidb_json"` を使用する。

## デフォルトのEXPLAINカラム（行形式）

TiDBの `EXPLAIN` はデフォルトで以下のカラムを出力する: `id`, `estRows`, `task`, `access object`, `operator info`。

## 構造化プラン: FORMAT = "tidb_json"

```sql
EXPLAIN FORMAT = "tidb_json"
SELECT /* your query */ 1;
```

出力はJSON配列。各オブジェクトには以下が含まれる:

- `id`, `estRows`, `taskType`, `accessObject`, `operatorInfo`
- `subOperators`: 子オペレータの配列（ツリー構造）

ヒント: フィールドが欠落している場合、それは空である。

## ビジュアルプラン: FORMAT = "dot"（Graphviz）

```sql
EXPLAIN FORMAT = "dot"
SELECT /* your query */ 1;
```

DOTグラフ文字列（`digraph ... {` で始まる）が返される。

### DOTを画像にレンダリング（任意）

`dot`（Graphviz）がローカルにインストールされている場合:

```bash
dot plan.dot -T png -O
```

ヘルパースクリプトが必要な場合は `skills/tidb-sql/scripts/render_dot_png.sh` を参照。

## EXPLAIN ANALYZE

```sql
EXPLAIN ANALYZE
SELECT /* your query */ 1;
```

`EXPLAIN` と比較して、`EXPLAIN ANALYZE` は以下のランタイムカラムを追加する:

- `actRows`
- `execution info`（時間、ループ数など）
- `memory`, `disk`

`estRows` と `actRows` の比較に使用する。大きな乖離は通常、統計情報の陳腐化・欠落、データの偏り、またはオプティマイザが適切に推定できない述語を示す。

注意: `EXPLAIN ANALYZE` でDMLステートメントを実行すると、データの変更は通常通り実行されるが、DMLステートメントの実行プランはまだ表示されない。

## EXPLAIN FOR CONNECTION（上級）

TiDBは以下をサポートする:

```sql
EXPLAIN FOR CONNECTION <connection_id>;
```

権限に関する注意: TiDBでは、他の接続のプランを表示するには通常 `SUPER` 権限が必要（または同一ユーザー/セッションであること）。
