# cc_unity_maker

Unity ミニゲームコレクション（100本）を Claude Code で自動実装するプロジェクト。

## ゲーム自動実装の実行方法

### run-features.sh で連続実行

`scripts/run-features.sh` を使うと、`/add-feature` を連番で自動実行できます。

```bash
./scripts/run-features.sh          # 001 から 1本だけ実行
./scripts/run-features.sh 5        # 005 から 1本だけ実行
./scripts/run-features.sh 3 7      # 003 〜 007 を順番に実行
START=2 END=10 ./scripts/run-features.sh  # 環境変数での指定も可
```

各ゲームのログは `logs/features/feature-[ID].jsonl` に保存されます。

### 手動で1本ずつ実行

```
/implement-game 014
```

## プロジェクト構成

```
MiniGameCollection/Assets/
├── Scripts/Game[ID]_[Title]/           # ゲームスクリプト
├── Editor/SceneSetup/                  # SceneSetup Editorスクリプト
├── Resources/Sprites/Game[ID]_[Title]/ # スプライトアセット
└── Resources/GameRegistry.json         # ゲーム一覧
```

## スラッシュコマンド

| コマンド | 説明 |
|---|---|
| `/implement-game [ID]` | ゲームを1本実装する（v2: 5ステージ・チュートリアル・高品質スプライト対応） |
| `/add-feature [機能]` | Unity ミニゲームに機能を追加する |
| `/setup-project` | 初回セットアップ: 6つの永続ドキュメントを対話的に作成する |
| `/unity-scene-setup [ゲームID]` | SceneSetup メニューを自動実行してシーンを構成する |
| `/unity-compile-check` | コンパイル状態を確認し、エラーがあれば自動修正する |
| `/unity-playmode-screenshot` | PlayMode を起動してゲーム画面をスクリーンショット撮影・検証する |
| `/review-codes [対象]` | 3軸の並列コードレビュー（構造・欠陥/セキュリティ・API準拠） |
| `/review-docs [対象]` | ドキュメントの詳細レビューをサブエージェントで実行 |

## スキル一覧

### Unity / ゲーム開発

| スキル | 説明 |
|---|---|
| `generate-game-assets` | Gemini CLI を使って Unity ゲーム用スプライト画像を自動生成 |
| `setup-game-issues` | TSV データを元に GitHub Issues にゲーム仕様を一括登録 |
| `update-game-issues` | 既存ゲーム Issue の body を v2+ テンプレートに一括更新 |
| `steering` | 作業計画・実装・検証フローをステアリングファイルで管理 |

### ドキュメント作成

| スキル | 説明 |
|---|---|
| `prd-writing` | プロダクト要求定義書（PRD）を作成 |
| `functional-design` | 機能設計書を作成 |
| `architecture-design` | アーキテクチャ設計書を作成 |
| `repository-structure` | リポジトリ構造定義書を作成 |
| `development-guidelines` | 開発ガイドラインを作成 |
| `glossary-creation` | ユビキタス言語定義（用語集）を作成 |
| `doc-coauthoring` | ドキュメントの共同執筆を構造化されたワークフローで支援 |
| `internal-comms` | ステータスレポート・社内ニュースレターなど社内向けコミュニケーションを作成 |

### フロントエンド / Web

| スキル | 説明 |
|---|---|
| `frontend-design` | プロダクション品質のフロントエンド UI を高いデザイン品質で作成 |
| `web-artifacts-builder` | React・Tailwind CSS・shadcn/ui を使った複雑なアーティファクトを構築 |
| `theme-factory` | スライド・レポート・HTML ページにテーマを適用 |
| `webapp-testing` | Playwright を使ってローカル Web アプリを操作・テスト |

### Zenn 記事

| スキル | 説明 |
|---|---|
| `zenn-article` | Zenn 技術記事を作成・執筆 |
| `zenn-review` | Zenn 記事を 6 軸でレビュー（AI 臭さ・技術的正確性・フォーマット等） |
| `zenn-publish` | 記事を zenn-satoken リポジトリにコピー・コミット・push して下書き公開 |

### TiDB

| スキル | 説明 |
|---|---|
| `tidb-cloud` | TiDB Cloud のクラスタ作成・削除・管理（ticloud CLI） |
| `tidb-serverless-driver` | サーバーレス/エッジ環境向け HTTP ドライバーのセットアップ |
| `tidb-kysely` | Kysely ORM と TiDB Cloud の統合パターン |
| `tidb-python` | Python (pytidb) による CRUD・ベクター検索・AI 機能の実装 |
| `tidb-sql` | TiDB 向け SQL 作成・MySQL 互換性対応・パフォーマンス診断 |

### その他

| スキル | 説明 |
|---|---|
| `ship-pr` | ブランチ作成・コミット・Push・PR 作成を一貫して実行 |
| `skill-creator` | 新しいスキルの作成・既存スキルの改善・最適化 |
