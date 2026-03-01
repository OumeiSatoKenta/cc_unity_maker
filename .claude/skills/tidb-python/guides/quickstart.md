<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/pytidb
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# クイックスタート（接続 → テーブル作成 → データ挿入 → ベクトル検索）

> このガイドは独自のフェーズを持つ完全なウォークスルーです。順番に進めてください。

## ワークフローチェックリスト

- [ ] TiDB のデプロイ方式と接続方法を確認
- [ ] Python 環境を作成し、依存関係をインストール
- [ ] `.env` を設定
- [ ] 接続を検証（`scripts/validate_connection.py`）
- [ ] クイックスタートテンプレートを実行

---

## フェーズ 1: 環境の確認

1) TiDB のタイプを確認:
- TiDB Cloud Starter
- TiDB セルフマネージド（`tiup playground` を含む）

2) 接続方法を確認:
- **接続パラメータ**（`TIDB_HOST`, `TIDB_PORT`, ...）
- **接続文字列**（`DATABASE_URL` / `TIDB_DATABASE_URL`）

## フェーズ 2: Python 環境のセットアップ

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -U pip
pip install pytidb python-dotenv
```

## フェーズ 3: 環境変数の設定

`.env` を作成（TiDB Cloud Starter の例）:

```bash
cat > .env <<'EOF'
TIDB_HOST={gateway-region}.prod.aws.tidbcloud.com
TIDB_PORT=4000
TIDB_USERNAME={prefix}.root
TIDB_PASSWORD={password}
TIDB_DATABASE=quickstart_example

# エンベディング用（いずれかのパスを選択）
OPENAI_API_KEY={your-openai-api-key}
EOF
```

## フェーズ 4: 接続の検証

```bash
python scripts/validate_connection.py
```

失敗した場合は `guides/troubleshooting.md` を参照してください。

## フェーズ 5: クイックスタートテンプレートの実行

1) `templates/quickstart.py` から `quickstart.py` を作成。
2) 実行:

```bash
python quickstart.py
```

期待される動作:
- TiDB に接続
- 自動エンベディングされるベクトルフィールドを持つテーブルを作成
- サンプルデータを挿入
- セマンティック検索クエリを実行
---

## 次のステップ

- CRUD / テーブルモデリング: `templates/crud.py` から開始
- 自動エンベディングプロバイダの選択: `templates/auto_embedding.py`
- その他のサンプル: `guides/demos.md`
