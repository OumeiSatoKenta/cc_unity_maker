<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidbx
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB Cloud CLI コマンドパターン

以下のパターンを使用し、<> 内のプレースホルダーをユーザー提供の値に置き換えてください。

## クラスター操作 (Serverless)

```bash
ticloud serverless create --display-name <cluster-name> --region <region> --project-id <project-id>
ticloud serverless list
ticloud serverless list -p <project-id>
ticloud serverless list -p <project-id> -o json
ticloud serverless describe
ticloud serverless describe -c <cluster-id>
ticloud serverless delete
ticloud serverless delete -c <cluster-id>
```

## ブランチ操作 (Serverless)

```bash
ticloud serverless branch create --cluster-id <cluster-id> --name <branch-name>
ticloud serverless branch list --cluster-id <cluster-id>
ticloud serverless branch delete --cluster-id <cluster-id> --branch-id <branch-id>
```

## インポート / エクスポート (Serverless)

```bash
ticloud serverless import start --cluster-id <cluster-id> --local.file-path <file-path> --file-type <file-type> --local.target-database <database> --local.target-table <table>
ticloud serverless export create --cluster-id <cluster-id> --target-type <target-type>
```

## 補助コマンド

```bash
ticloud serverless region
ticloud project list
ticloud auth login
ticloud auth whoami
```
## SQL ユーザー操作 (Serverless)

```bash
ticloud serverless sql-user list --cluster-id <cluster-id>
ticloud serverless sql-user update --user <user-name> --password <password> --role <role> --cluster-id <cluster-id>
ticloud serverless sql-user delete --user <user-name> --cluster-id <cluster-id>
```

SQL ユーザーの作成

使用方法:
  ticloud serverless sql-user create [flags]

例:
  対話モードで SQL ユーザーを作成:
  $ ticloud serverless sql-user create

  非対話モードで SQL ユーザーを作成:
  $ ticloud serverless sql-user create --user <user-name> --password <password> --role <role> --cluster-id <cluster-id>

フラグ:
  -c, --cluster-id string   クラスターの ID。
  -h, --help                create のヘルプ
      --password string     SQL ユーザーのパスワード。
      --role strings        SQL ユーザーのロール。使用可能なロール: ["role_admin" "role_readwrite" "role_readonly"]
  -u, --user string         SQL ユーザーの名前。ユーザープレフィックスは自動的に付与されます。

グローバルフラグ:
  -D, --debug            デバッグモードを有効にする
      --no-color         カラー出力を無効にする
  -P, --profile string   設定ファイルから使用するプロファイル
