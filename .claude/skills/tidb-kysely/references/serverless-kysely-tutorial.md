<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidbx-kysely
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB Cloud サーバーレスドライバー Kysely チュートリアル

サーバーレスまたはエッジランタイムで HTTP ベースの接続が必要な場合に `@tidbcloud/kysely` を使用します。
TCP プールが利用できない環境での信頼性が向上します。

## 前提条件

- Node.js 18 以上および npm。
- {{{ .starter }}} クラスター。
- `DATABASE_URL` を `mysql://[username]:[password]@[host]/[database]` に設定済みであること。

## Node.js（サーバーレスドライバー）

```bash
npm install kysely @tidbcloud/kysely @tidbcloud/serverless
```

```ts
import { Kysely, GeneratedAlways, Selectable } from 'kysely'
import { TiDBCloudServerlessDialect } from '@tidbcloud/kysely'

interface Database {
  person: PersonTable
}

interface PersonTable {
  id: GeneratedAlways<number>
  name: string
  gender: 'male' | 'female'
}

const db = new Kysely<Database>({
  dialect: new TiDBCloudServerlessDialect({ url: process.env.DATABASE_URL }),
})

type Person = Selectable<PersonTable>
export async function findPeople(criteria: Partial<Person> = {}) {
  let query = db.selectFrom('person')

  if (criteria.name) {
    query = query.where('name', '=', criteria.name)
  }

  return await query.selectAll().execute()
}

console.log(await findPeople())
```

## エッジ（Vercel の例）

```bash
npm install kysely @tidbcloud/kysely @tidbcloud/serverless
```

```ts
import { NextResponse } from 'next/server'
import type { NextRequest } from 'next/server'
import { Kysely, GeneratedAlways, Selectable } from 'kysely'
import { TiDBCloudServerlessDialect } from '@tidbcloud/kysely'

export const runtime = 'edge'

interface Database {
  person: PersonTable
}

interface PersonTable {
  id: GeneratedAlways<number>
  name: string
  gender: 'male' | 'female' | 'other'
}

const db = new Kysely<Database>({
  dialect: new TiDBCloudServerlessDialect({ url: process.env.DATABASE_URL }),
})

type Person = Selectable<PersonTable>
async function findPeople(criteria: Partial<Person> = {}) {
  let query = db.selectFrom('person')

  if (criteria.name) {
    query = query.where('name', '=', criteria.name)
  }

  return await query.selectAll().execute()
}

export async function GET(request: NextRequest) {
  const query = request.nextUrl.searchParams.get('query')
  const response = query ? await findPeople({ name: query }) : await findPeople()
  return NextResponse.json(response)
}
```

## リンク

- Kysely: https://kysely.dev/docs/intro
- @tidbcloud/kysely: https://github.com/tidbcloud/kysely
