---
name: tidb-kysely
description: "Set up Kysely with TiDB Cloud (TiDB X), including @tidbcloud/kysely over the TiDB Cloud serverless HTTP driver for serverless or edge environments, plus standard TCP usage. Kysely ORMとTiDB Cloudの統合パターン・接続セットアップ。"
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidbx-kysely
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB Cloud + Kysely

このスキルは、ユーザーが Kysely を TiDB Cloud（TiDB X）に接続したい場合に使用します。デフォルトでは
標準的な TCP 接続（Node サーバー/ランタイム）を使用します。サーバーレスまたはエッジランタイムの場合のみ、
HTTP 経由の TiDB Cloud サーバーレスドライバーを使用してください。

## ワークフロー

1. ランタイムとデプロイ先を確認する（Node サーバー vs サーバーレス/エッジ）。
2. クラスタータイプを確認する。サーバーレス HTTP ドライバーは Starter/Essential クラスターに適用されます。
3. 接続情報を収集する（`DATABASE_URL` に `mysql://` URL を設定することを推奨）。
4. パスを選択する:
   - 通常の使い方（デフォルト）: TCP + `mysql2` プール + Kysely `MysqlDialect`。
   - サーバーレス/エッジ: HTTP 経由の `@tidbcloud/kysely` ダイアレクト。
5. まず該当するパスのスニペットのみを提示する。もう一方のパスはユーザーから要望があった場合のみ提示する。
   完全なサンプルコードは `references/kysely-usage.md` を参照。

## 通常の使い方（デフォルト）

Node サーバーや長時間稼働するランタイム、TCP が利用可能な環境で使用します。ユーザーが明示的に
サーバーレス/エッジを必要としない限り、これがメインのパスです。TCP と `mysql2` プールを使用します。

```ts
import { Kysely, MysqlDialect } from 'kysely'
import { createPool } from 'mysql2'

const pool = createPool({ uri: process.env.DATABASE_URL })
const db = new Kysely({ dialect: new MysqlDialect({ pool }) })
```

## サーバーレス/エッジでの使い方（HTTP）

ランタイムが TCP 接続を維持できない場合（サーバーレス/エッジ）のみ使用します。TiDB Cloud
サーバーレスドライバーと Starter/Essential クラスターが必要です。バックエンドサービスからのみ
使用してください（ブラウザからのリクエストは CORS によりブロックされる場合があります）。
完全なチュートリアルは `references/serverless-kysely-tutorial.md` を参照してください。

```ts
import { Kysely } from 'kysely'
import { TiDBCloudServerlessDialect } from '@tidbcloud/kysely'

const db = new Kysely({
  dialect: new TiDBCloudServerlessDialect({ url: process.env.DATABASE_URL }),
})
```

## 補足事項

- 多くのユーザーは「インスタンス」を「クラスター」の意味で使います。同じものとして扱ってください。
- 説明は簡潔にまとめ、詳細なドキュメントは references に配置してください。
