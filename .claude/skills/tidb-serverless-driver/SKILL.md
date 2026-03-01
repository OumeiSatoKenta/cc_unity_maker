---
name: tidb-serverless-driver
description: "Guidance for using the TiDB Cloud Serverless Driver (Beta) in Node.js, serverless, and edge environments. Use when connecting to TiDB Cloud Starter/Essential over HTTP with @tidbcloud/serverless, or when integrating with Prisma/Kysely/Drizzle serverless adapters in Vercel/Cloudflare/Netlify/Deno/Bun. サーバーレス/エッジ環境向けHTTPドライバーのセットアップガイド。"
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/tidbx-serverless-driver
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# TiDB Cloud Serverless Driver (Beta)

サーバーレスまたはエッジ環境で TiDB Cloud Serverless Driver (Beta) を利用するユーザーを支援するためのスキルです。

## はじめに

サーバーレスやエッジランタイムは、長時間のTCP接続をサポートしないことが多いです。従来のMySQLドライバーはTCP接続を前提としているため、これらの環境には適していません。TiDB Cloud Serverless Driver (Beta) はHTTPを使用するため、サーバーレスやエッジ環境で動作しつつ、開発者にとって馴染みのある使用感を維持しています。

## インストール

**`npm install @tidbcloud/serverless`**

## チュートリアル（リファレンス）

ドライバーの概要、使用例、設定、制限事項についてはリファレンスファイルを参照してください。必要な部分だけを読み込み、目次から該当セクションに移動してください:

- 正式なドキュメント: `references/serverless-driver.md`

## 使い方ガイド

- クラスタの種別を確認してください: Starter または Essential。
- 使用するランタイムを確認してください: Node.js、Vercel Edge、Cloudflare Workers、Netlify、Deno、Bun。
- TiDB Cloud コンソールから接続文字列を取得してください。**Connect** 画面で **Serverless Driver** を選択し、パスワードを生成/リセットしてから `DATABASE_URL` をコピーしてください。
- クラスタのプロビジョニングやCRUD操作については、`tidb-cloud` スキルを使用してください。
