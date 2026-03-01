---
title: TiDB Cloud SSL Verification (Connection Gotchas)
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidb-sql
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB Cloud SSL検証（接続時の注意点）

MySQLプロトコルでTiDB Cloudに接続する場合、TLSを有効にし以下を強制する:

- サーバー証明書の検証
- サーバーID（ホスト名）の検証

SSL検証が欠落または誤設定されている場合、SQLを実行する前に接続エラーが発生することがある。

## TiDB Cloudゲートウェイホストの検出

ホストが以下のパターン（Python正規表現）に一致する場合:

```python
r"gateway\\d{2}\\.(.+)\\.(prod|dev|staging)\\.(shared\\.)?(aws|alicloud)\\.tidbcloud\\.com"
```

TiDB Cloudの要件が適用されるものとして扱う。

## 一般的なクライアント設定

MySQL CLI:

```bash
mysql ... --ssl-mode=VERIFY_IDENTITY
```

MariaDB CLI:

```bash
mariadb ... --ssl-verify-server-cert
```

Node.js (mysql2):

```js
ssl: {
  minVersion: 'TLSv1.2',
  rejectUnauthorized: true,
},
```

Prisma（接続文字列のクエリパラメータ）:

```text
sslaccept=strict
```

Go (database/sql + go-sql-driver/mysql):

TLS設定を登録し、DSNで `tls=` を使って参照する:

```go
mysql.RegisterTLSConfig("tidb", &tls.Config{
  MinVersion: tls.VersionTLS12,
  ServerName: "gateway01.ap-southeast-1.prod.aws.tidbcloud.com",
})

db, err := sql.Open(
  "mysql",
  "USER:PASSWORD@tcp(gateway01.ap-southeast-1.prod.aws.tidbcloud.com:4000)/DB?tls=tidb",
)
```

Rails (ActiveRecord + mysql2, database.yml URL):

```yaml
development:
  adapter: mysql2
  url: mysql2://USER:PASSWORD@gateway01.ap-southeast-1.prod.aws.tidbcloud.com:4000/DB?ssl_mode=verify_identity
```

MySQL Connector/J (JDBC URLパラメータ):

```text
sslMode=VERIFY_IDENTITY
```

## DSN / URLクエリパラメータ（汎用）

接続文字列を構築していてクエリパラメータのみで制御する場合、以下を追加する:

```text
ssl_verify_cert=true&ssl_verify_identity=true
```

ホストが上記のゲートウェイパターンに一致する場合、特にこれを使用する。
