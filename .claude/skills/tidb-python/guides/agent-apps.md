<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/pytidb
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# エージェント系アプリ（RAG / メモリ / text2sql）

これらはより大規模なサンプルです。PyTiDB を使って TiDB にデータを保存・検索する「AI アプリ」を構築したい場合に使用してください。

## RAG（検索拡張生成）

- テンプレート: `templates/rag.py`
- 使用技術: ベクトル検索による取得 + ローカル LLM（Ollama via LiteLLM）
- 最適な用途: モデルのランタイムを自分で管理するオフラインデモ

## メモリ（永続的なエージェントメモリ）

- テンプレート:
  - `templates/memory_lib.py`
  - `templates/memory.py`
- 使用技術: 事実を抽出 → ベクトルとして保存 → user_id ごとに類似度で取得

## Text2SQL

- テンプレート: `templates/text2sql.py`
- 使用技術: OpenAI で SQL を生成し、PyTiDB 経由で TiDB 上で実行
- 安全性: 生成された SQL は必ずレビューすること。アプリロジックで SELECT 以外をブロックすること
