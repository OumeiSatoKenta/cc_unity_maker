# タスクリスト: GitHubラベルとIssueテンプレートのセットアップ

## 実装タスク

- [x] `.github/ISSUE_TEMPLATE/` ディレクトリを作成する
- [x] `.github/ISSUE_TEMPLATE/game-spec.md` を作成する
- [x] `.github/ISSUE_TEMPLATE/config.yml` を作成する
- [x] `scripts/setup-labels.sh` を作成する
- [x] `scripts/setup-labels.sh` に実行権限を付与する

## レビューセクション

**完了日**: 2026-03-29
**実績**: 計画通り全タスク完了

**作成物**:
- `scripts/setup-labels.sh` — 12ラベルを実際にGitHubへ作成済み
- `.github/ISSUE_TEMPLATE/game-spec.md` — Issue作成テンプレート
- `.github/ISSUE_TEMPLATE/config.yml` — ブランクIssue禁止・アイデア一覧リンク

**備考**:
- リモートが cc_base だったため、セッション途中で cc_unity_maker に変更した
- ラベルはスクリプト実行で即座に反映済み（GitHub上に存在する）
- 次: 全100ゲームのIssue一括登録スクリプト (`/add-feature 全100ゲームのIssue一括登録スクリプト`)
