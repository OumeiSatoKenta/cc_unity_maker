<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidbx-serverless-driver
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB Cloud Serverless Driver (Beta)

> **注意:** Serverless Driver はベータ版であり、Starter または Essential クラスタにのみ適用されます。

## 使用する理由

サーバーレスやエッジランタイムは短命であり、完全なTCPサポートが欠けていることが多いです。従来のMySQLドライバーは長時間のTCP接続を前提としているため、これらの環境では失敗したりパフォーマンスが低下したりする可能性があります。TiDB Cloud Serverless Driver はHTTPを使用するため、サーバーレス/エッジ環境で適切に動作しつつ、開発者にとって馴染みのある使用感を維持しています。

SQL/ORMよりもRESTを好む場合は、Data Service (beta) を利用してください: https://docs.pingcap.com/tidbcloud/data-service-overview

## インストール

```bash
npm install @tidbcloud/serverless
```

## 基本的な使い方

### クエリ

```ts
import { connect } from '@tidbcloud/serverless'

const conn = connect({ url: 'mysql://[username]:[password]@[host]/[database]' })
const results = await conn.execute('select * from test where id = ?', [1])
```

### トランザクション（実験的機能）

```ts
import { connect } from '@tidbcloud/serverless'

const conn = connect({ url: 'mysql://[username]:[password]@[host]/[database]' })
const tx = await conn.begin()

try {
  await tx.execute('insert into test values (1)')
  await tx.execute('select * from test')
  await tx.commit()
} catch (err) {
  await tx.rollback()
  throw err
}
```

## エッジ環境の例

Vercel Edge Function:

```ts
import { NextResponse } from 'next/server'
import type { NextRequest } from 'next/server'
import { connect } from '@tidbcloud/serverless'

export const runtime = 'edge'

export async function GET(request: NextRequest) {
  const conn = connect({ url: process.env.DATABASE_URL })
  const result = await conn.execute('show tables')
  return NextResponse.json({ result })
}
```

Cloudflare Workers:

```ts
import { connect } from '@tidbcloud/serverless'

export interface Env {
  DATABASE_URL: string
}

export default {
  async fetch(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
    const conn = connect({ url: env.DATABASE_URL })
    const result = await conn.execute('show tables')
    return new Response(JSON.stringify(result))
  }
}
```

Netlify Edge Function:

```ts
import { connect } from 'https://esm.sh/@tidbcloud/serverless'

export default async () => {
  const conn = connect({ url: Netlify.env.get('DATABASE_URL') })
  const result = await conn.execute('show tables')
  return new Response(JSON.stringify(result))
}
```

Deno:

```ts
import { connect } from 'npm:@tidbcloud/serverless'

const conn = connect({ url: Deno.env.get('DATABASE_URL') })
const result = await conn.execute('show tables')
```

Bun:

```ts
import { connect } from '@tidbcloud/serverless'

const conn = connect({ url: Bun.env.DATABASE_URL })
const result = await conn.execute('show tables')
```

## 設定（主要項目）

接続レベルのオプション:

- `url`: `mysql://[username]:[password]@[host]/[database]`（推奨）
- `fetch`: カスタムfetch関数（例: Node.js での `undici`）
- `arrayMode`: 行を配列として返す
- `fullResult`: 完全な結果オブジェクトを返す
- `decoders`: カラムごとのカスタム型デコーダー

SQLレベルのオプション（接続レベルの設定を上書き）:

- `arrayMode`、`fullResult`、`decoders`
- `isolation`: `READ COMMITTED` または `REPEATABLE READ`（`begin` のみ対象）

URLエンコードに関する注意: ユーザー名/パスワード/データベース名の特殊文字はパーセントエンコードしてください。例: `password1@//?` → `password1%40%2F%2F%3F`

`url` は、従来の `host`、`username`、`password`、`database` の個別フィールドを置き換えるものです。

## 機能

対応SQL: `SELECT`、`SHOW`、`EXPLAIN`、`USE`、`INSERT`、`UPDATE`、`DELETE`、`BEGIN`、`COMMIT`、`ROLLBACK`、`SET`、およびDDL。

データ型のマッピング（概要）:

- 数値型 → `number`
- `BIGINT`、`DECIMAL` → `string`
- バイナリ/BLOB/BIT → `Uint8Array`
- `JSON` → `object`
- `DATETIME`、`TIMESTAMP`、`DATE`、`TIME` → `string`

正確な文字列デコードには `utf8mb4` を使用してください。v0.1.0 以降、バイナリ/BLOB/BIT 型は `string` ではなく `Uint8Array` を返します。

## 料金

ドライバー自体は無料です。利用量に応じて Request Units (RU) とストレージが消費されます:

- Starter の料金: https://www.pingcap.com/tidb-cloud-starter-pricing-details/
- Essential の料金: https://www.pingcap.com/tidb-cloud-essential-pricing-details/

## 制限事項

- クエリあたり最大 10,000 行。
- クエリあたり1つのSQL文のみ。
- プライベートエンドポイントは未対応。
- CORSにより未認可のブラウザオリジンはブロックされます。バックエンドサービスからのみ使用してください。
