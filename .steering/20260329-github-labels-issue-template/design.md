# 設計: GitHubラベルとIssueテンプレートのセットアップ

## 作成ファイル一覧

```
scripts/
└── setup-labels.sh                      # ラベル一括作成スクリプト
.github/
├── ISSUE_TEMPLATE/
│   └── game-spec.md                     # ゲーム実装仕様Issueテンプレート
└── ISSUE_TEMPLATE/config.yml            # テンプレート設定
```

## setup-labels.sh の設計

- `gh label create` に `--force` を付けて冪等実行可能にする
- 色はカテゴリ別に視認しやすい色を割り当て
- スクリプト冒頭で `gh auth status` を確認してエラーをわかりやすくする

**カラー設計**:
| ラベル種別 | 色 | 理由 |
|-----------|-----|------|
| category:puzzle | #0075ca (青) | 思考・論理のイメージ |
| category:action | #e4e669 (黄) | エネルギッシュなイメージ |
| category:casual | #cfd3d7 (グレー) | ライトなイメージ |
| category:idle | #a2eeef (水色) | ゆったりしたイメージ |
| category:rhythm | #d93f0b (赤橙) | 音楽・情熱のイメージ |
| category:simulation | #0e8a16 (緑) | 育成・成長のイメージ |
| category:unique | #7057ff (紫) | ユニーク・実験的なイメージ |
| size:S | #bfd4f2 (薄青) | 軽量感 |
| size:M | #5319e7 (紫) | 中程度 |
| size:L | #b60205 (赤) | 重量感 |
| status:in-progress | #fbca04 (黄) | 進行中 |
| status:done | #0e8a16 (緑) | 完了 |

## game-spec.md テンプレート設計

Issue 作成時に表示されるフォーム形式。以下のセクションを含む:

1. **ゲーム情報** (ID・タイトル・カテゴリ・工数)
2. **ゲーム概要** (100字以内)
3. **コアループ** (3ステップ)
4. **必要なGameObject一覧** (テーブル)
5. **クリア/ゲームオーバー条件**
6. **Unity実装方針**
7. **Claude Codeへの依頼文** (コピペ用)
8. **実装チェックリスト**
9. **動作確認チェックリスト**
