# タスクリスト: 全100ゲームのIssue一括登録スクリプト

## 実装タスク

- [x] `scripts/game-data.tsv` を作成する（ideas.mdから100件抽出）
- [x] `scripts/create-all-issues.sh` を作成する
- [x] `scripts/create-all-issues.sh` に実行権限を付与する
- [x] スクリプトを実行して全100件のIssueを作成する

## レビューセクション

**完了日**: 2026-03-29
**実績**: 全100件のIssue登録完了（Issue #2〜#101）

**備考**:
- 1回目の実行で40件作成後にGitHub API 504タイムアウト発生
- 冪等設計により2回目の実行で残り60件を問題なく作成
- 全Issueにカテゴリ・工数ラベルが正しく付与されている
- 次: `/ship-pr` → mainにマージ → `/add-feature Unityプロジェクト初期構築`
