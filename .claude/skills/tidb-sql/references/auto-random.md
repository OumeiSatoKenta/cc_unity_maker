---
title: TiDB AUTO_RANDOM (SQL)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB AUTO_RANDOM (SQL)

`AUTO_RANDOM` は、分散ストレージにおいて連番キーで発生しうる書き込みホットスポットを回避するために使用する。

## AUTO_RANDOMを優先すべき場面

- 書き込み負荷の高いワークロードで、主キーに `BIGINT AUTO_INCREMENT` を使用する場合の代替として。
- 厳密に増加するIDが不要な場合。

## DDLパターン

有効な形式（`BIGINT` であること、主キーの一部であることが必要。通常はPKの最初のカラム）:

```sql
CREATE TABLE t (a BIGINT PRIMARY KEY AUTO_RANDOM, b VARCHAR(255));
CREATE TABLE t (a BIGINT AUTO_RANDOM(6), b VARCHAR(255), PRIMARY KEY (a));
```

INSERT時の動作:

- `AUTO_RANDOM` カラムを `INSERT` で省略すると、TiDBがランダムな一意値を生成する。
- 明示的に指定した場合、TiDBはその値をそのまま挿入する（ただし、通常は推奨されない）。

## 運用上の注意点

- 明示的な挿入には `@@allow_auto_random_explicit_insert = 1` の有効化が必要な場合がある。
- マルチノード環境での明示的な挿入後、衝突を避けるために「リベース」が必要になることがある:

```sql
ALTER TABLE t AUTO_RANDOM_BASE = 0;
```

## 覚えておくべき制約事項

- `ALTER TABLE` で `AUTO_RANDOM` 属性を後から追加・削除・変更することはできない。
- 同じカラムで `AUTO_RANDOM` と `AUTO_INCREMENT` または `DEFAULT` を併用することはできない。
