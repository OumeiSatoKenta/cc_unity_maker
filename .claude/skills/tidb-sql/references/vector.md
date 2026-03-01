---
title: TiDB Vector SQL (Types, Functions, Indexes)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDBベクトルSQL（型、関数、インデックス）

## 機能ゲート

- ベクトルデータ型と関数はTiDB v8.4.0以降が必要（セルフマネージド/Dedicatedデプロイメントではv8.5.0以降を推奨）。
- `SELECT VERSION();` で確認する。

## データ型

- `VECTOR`: 可変次元（ベクトルインデックスを構築できない）
- `VECTOR(D)`: 固定次元 `D`（ベクトルインデックスに必要）

例:

```sql
CREATE TABLE embedded_documents (
  id INT PRIMARY KEY,
  document TEXT,
  embedding VECTOR(3)
);
```

ベクトルリテラルは文字列として挿入する:

```sql
INSERT INTO embedded_documents VALUES (1, 'dog', '[1,2,1]');
```

## 距離関数（主要なもの）

- `VEC_COSINE_DISTANCE(v1, v2)`
- `VEC_L2_DISTANCE(v1, v2)`
- （他にも `VEC_L1_DISTANCE`, `VEC_NEGATIVE_INNER_PRODUCT` がある）

クエリ例（完全スキャン）:

```sql
SELECT id, document, VEC_COSINE_DISTANCE(embedding, '[1,2,3]') AS distance
FROM embedded_documents
ORDER BY distance
LIMIT 10;
```

## キャスト / パースヘルパー

- `VEC_FROM_TEXT('[...]')` - 文字列 -> ベクトル
- `VEC_AS_TEXT(vec)` - ベクトル -> 文字列
- `CAST('[...]' AS VECTOR)` - 文字列 -> ベクトル

ヒント: ベクトル定数を比較する場合、文字列ベースの比較を避けるために明示的にキャストする。

## ベクトルインデックス（HNSW）の要点

前提条件 / 制約:

- TiFlashノード（およびテーブルのTiFlashレプリカ）が必要。
- `PRIMARY KEY` や `UNIQUE` にはできない。
- 単一のベクトルカラムのみ（ベクトル+他カラムの複合インデックスは不可）。
- インデックス定義とクエリの並べ替えで同じ距離関数を使用する必要がある。

テーブル作成時にインデックスを作成する:

```sql
CREATE TABLE foo (
  id INT PRIMARY KEY,
  embedding VECTOR(3),
  VECTOR INDEX idx_embedding ((VEC_COSINE_DISTANCE(embedding)))
);
```

既存テーブルにインデックスを作成する:

```sql
CREATE VECTOR INDEX idx_embedding ON foo ((VEC_COSINE_DISTANCE(embedding))) USING HNSW;
-- または:
ALTER TABLE foo ADD VECTOR INDEX idx_embedding ((VEC_COSINE_DISTANCE(embedding))) USING HNSW;
```

ANNインデックスを使用するクエリパターン:

- `ORDER BY VEC_COSINE_DISTANCE(...) ASC LIMIT <K>` を使用する（Top-Kが必要）
- 降順や距離関数の不一致はインデックス使用を妨げる

インデックス使用の確認:

```sql
EXPLAIN SELECT * FROM foo
ORDER BY VEC_COSINE_DISTANCE(embedding, '[1,2,3]')
LIMIT 10;
SHOW WARNINGS;
```
