# 要求内容: 全100ゲームのIssue一括登録スクリプト

## 背景
cc_game_ideasの100本のゲームアイデアを、cc_unity_makerリポジトリのGitHub Issueとして一括登録する。
各Issueにはゲーム実装仕様テンプレートの骨格と適切なラベルが付与される。

## 作成物

### 1. Issue一括登録スクリプト (`scripts/create-all-issues.sh`)
cc_game_ideasの`ideas/ideas.md`からゲーム情報を取得し、100件のIssueを自動作成するシェルスクリプト。

**各Issueに含める内容**:
- タイトル: `[XXX] タイトル (工数: S/M/L)`
- 本文: game-specテンプレートに基づく骨格（概要・コアメカニクスは ideas.md から取得）
- ラベル: `category:<カテゴリ>` + `size:<工数>`

### 2. ゲームデータファイル (`scripts/game-data.tsv`)
ideas.mdから抽出した100件のゲームデータ（TSV形式）。スクリプトの入力データとして使用。

## 制約
- GitHub APIのレート制限を考慮して適切なスリープを入れる
- 冪等性: 同じタイトルのIssueが既に存在する場合はスキップ
- スクリプト実行前にgh認証状態を確認
