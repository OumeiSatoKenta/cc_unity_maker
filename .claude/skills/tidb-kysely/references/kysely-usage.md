<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidbx-kysely
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# Kysely + TiDB Cloud の使い方

以下のスニペットを出発点として使用し、パッケージ API が異なる場合はエクスポートを適宜調整してください。
`@tidbcloud/kysely` は、TiDB Cloud サーバーレス HTTP ドライバー用の Kysely ダイアレクトを提供します。

## 通常の使い方（mysql2 経由の TCP）- デフォルト

```ts
import { Kysely, MysqlDialect } from 'kysely'
import { createPool } from 'mysql2'

interface Database {
  users: {
    id: number
    email: string
  }
}

const pool = createPool({
  uri: process.env.DATABASE_URL,
  connectionLimit: 10,
})

const db = new Kysely<Database>({
  dialect: new MysqlDialect({ pool }),
})

const users = await db.selectFrom('users').selectAll().execute()
console.log(users)

await db.destroy()
```

## サーバーレスでの使い方（@tidbcloud/kysely 経由の HTTP）- サーバーレス/エッジ専用

セットアップ手順と Node.js およびエッジ環境でのステップバイステップガイドについては、
`references/serverless-kysely-tutorial.md` の完全なチュートリアルを参照してください。

## インストール

TCP（標準的な Node サーバー）:

```bash
npm install kysely mysql2
```

サーバーレス/エッジ（HTTP）:

```bash
npm install kysely @tidbcloud/kysely @tidbcloud/serverless
```

## ヒント

- `DATABASE_URL` に `mysql://user:pass@host/db` 形式の URL を設定してください。特殊文字はパーセントエンコーディングしてください。
- サーバーレス/エッジランタイムでは TCP プールを避け、HTTP ダイアレクトを使用してください。
