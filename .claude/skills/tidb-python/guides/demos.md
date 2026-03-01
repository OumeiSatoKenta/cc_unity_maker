<!--
  原文: https://github.com/pingcap/agent-rules/tree/main/skills/pytidb
  このファイルは上記リポジトリのスキル定義を日本語に翻訳したものです。
-->

# サンプル集

そのまま実行してカスタマイズできる `example.py` テンプレートです。

## ワークフローチェックリスト

- [ ] 依存関係をインストール
- [ ] `.env` を設定
- [ ] 接続を検証（`scripts/validate_connection.py`）
- [ ] 選択したサンプルを実行

---

## ベクトル検索

1) 依存関係のインストール:

```bash
pip install pytidb python-dotenv
```

2) 実行:

```bash
python templates/vector_search.py "HTAP database"
```

## ハイブリッド検索

1) 依存関係のインストール:

```bash
pip install pytidb python-dotenv
```

2) `.env` に `OPENAI_API_KEY` を設定。
3) 実行:

```bash
python templates/hybrid_search.py
```

## 画像検索

1) 依存関係のインストール:

```bash
pip install pytidb python-dotenv pillow
```

2) データセット（Oxford Pets）を `./oxford_pets/images` にダウンロード。
3) ロードして検索:

```bash
python templates/image_search.py --load-sample --text "fluffy orange cat"
```
