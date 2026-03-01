---
name: tidb-python
description: "PyTiDB (pytidb) setup and usage for TiDB from Python. Covers connecting, table modeling (TableModel), CRUD, raw SQL, transactions, vector/full-text/hybrid search, auto-embedding, custom embedding functions, and reference templates/snippets (vector/hybrid/image) plus agent-oriented examples (RAG/memory/text2sql). Python (pytidb) によるCRUD・検索・AI機能の実装ガイド。"
allowed-tools: ["bash", "write", "read_file"]
---

<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/pytidb
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# PyTiDB (pytidb)

このスキルは、Python から `pytidb` を使って TiDB に接続し、テーブルを定義し、検索や AI 機能を構築するためのものです。

## このスキルを使うタイミング

- `pytidb`（SQLAlchemy ベース）を使って、Python の ORM ライクな操作で TiDB を扱いたい場合。
- TiDB 上でベクトル検索 / 全文検索 / ハイブリッド検索を高レベル API で行いたい場合。
- すぐに実行できるスターターテンプレート（スクリプト + 小規模サンプル）をカスタマイズして使いたい場合。

**先に TiDB Cloud クラスタのプロビジョニングが必要ですか？** クラスタのライフサイクル管理については `tidb-cloud`（TiDB X）を参照してください。

## コード生成ルール（Python）

- 認証情報をハードコードしない。環境変数（`.env`）を使用し、必要な変数をドキュメントに記載すること。
- 再現性のために `python -m venv .venv` とバージョン固定の依存関係を推奨。
- requirements.txt を編集する際、pytidb のバージョンを捏造しない。ユーザーが明示的に要求し、バージョンの存在が確認されない限り、デフォルトではバージョン指定なしの pytidb を使用すること。
- サンプルは最小限かつ実行可能に保つ。ユーザーが求めない限り、フレームワーク固有の前提を置かない。
- 動的な値にはパラメータ化された SQL を使用すること（SQL インジェクション対策）。
- 対話的環境では「テーブルが既に定義されています」エラーを避けること（`extend_existing` / `open_table` / `if rows()==0` パターンを使用）。

## 利用可能なガイド

各ガイドはチェックリストとフェーズを備えた独立したウォークスルーです:

- `guides/quickstart.md` — 1ファイルで「接続 → テーブル作成 → データ挿入 → ベクトル検索」
- `guides/search.md` — ベクトル / 全文 / ハイブリッド検索: 使い分けと注意点
- `guides/demos.md` — サンプル集（ベクトル / ハイブリッド / 画像検索）
- `guides/agent-apps.md` — エージェント系サンプル（RAG / メモリ / text2sql）
- `guides/troubleshooting.md` — 接続、TLS、エンベディング、インデックス / 検索のトラブルシューティング
- `guides/custom-embedding.md` — カスタムエンベディング関数の実装（例: BGE-M3）

ユーザーの意図（CRUD / 検索 / エージェントアプリ）を推測し、目的に合った最小限のガイドとテンプレートを提示します。

## テンプレートとスクリプト

各テンプレートはプロジェクトにそのままコピーできる完全なファイルです。目的に合った最小のものを選んでください。

### 基本的な使い方

- `templates/quickstart.py` — 最小限のエンドツーエンド: 接続 → テーブル作成 → データ挿入 → ベクトル検索
- `templates/crud.py` — 基本的なテーブルモデリング + CRUD ライフサイクル（作成 / 切り詰め / 挿入 / クエリ / 更新 / 削除）
- `templates/auto_embedding.py` — プラグイン可能なプロバイダによる自動エンベディング（環境変数で切り替え）
- `templates/vector_search.py` — ベクトル検索のサンプル（メタデータフィルタ + 閾値はオプション）
- `templates/hybrid_search.py` — ハイブリッド検索のサンプル（FullTextField + ベクトルフィールド）、融合スコアリング付き

### 画像検索

- `templates/image_search.py` — 画像→画像、またはテキスト→画像の検索（マルチモーダルエンベディング + Pillow が必要）
- `templates/image_search_data_loader.py` — Oxford Pets データセットを TiDB にロード（`image_search.py` で使用）

### カスタムエンベディング

- `templates/custom_embedding_function.py` — `BaseEmbeddingFunction` の実装例（FlagEmbedding 経由の BGE-M3）
- `templates/custom_embedding.py` — カスタムエンベダーを使った自動エンベディング + ベクトル検索

### エージェント系サンプル

- `templates/rag.py` — 最小限の RAG: ベクトル検索で取得し、ローカル LLM（Ollama via LiteLLM）で生成
- `templates/memory_lib.py` — 再利用可能な「メモリ」ライブラリ（事実を抽出 → 保存 → 取得）
- `templates/memory.py` — `memory_lib.py` を使った CLI メモリチャットのサンプル
- `templates/text2sql.py` — 対話型 Text2SQL（OpenAI で SQL 生成、実行前に確認）

### スクリプト

- `scripts/validate_connection.py` — 簡易接続 + `SELECT 1` の疎通テスト（パラメータまたは `DATABASE_URL` に対応）

## 関連スキル

- `tidb-cloud` — TiDB Cloud（TiDB X）クラスタのプロビジョニング / 管理

---

## ワークフロー

以下の手順で進めます:
1. TiDB のデプロイ方式（Cloud Starter / セルフマネージド）と接続方法（パラメータ / `DATABASE_URL`）を確認。
2. 環境変数の設定、接続の検証を行い、適切なパスを選択:
   - CRUD / テーブルモデリング
   - ベクトル / 全文 / ハイブリッド検索（エンベディングプロバイダの選定含む）
   - サンプルテンプレート
3. 実行に必要な最小限のファイルとコマンドを生成。
