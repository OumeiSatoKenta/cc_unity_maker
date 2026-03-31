# 要求内容: GitHubラベルとIssueテンプレートのセットアップ

## 背景
cc_unity_maker の GitHub リポジトリで、全100ゲームの Issue を体系的に管理するために
ラベルと Issue テンプレートを整備する。

## 作成物

### 1. GitHubラベル定義スクリプト (`scripts/setup-labels.sh`)
以下のラベルを `gh label create` コマンドで一括作成するシェルスクリプト。

**カテゴリラベル** (ゲームジャンル):
- `category:puzzle` — パズル系 (001-020)
- `category:action` — アクション系 (021-040)
- `category:casual` — カジュアル系 (041-060)
- `category:idle` — 放置系 (061-070)
- `category:rhythm` — リズム系 (071-080)
- `category:simulation` — 育成系 (081-090)
- `category:unique` — ユニーク系 (091-100)

**工数ラベル**:
- `size:S` — 1日程度
- `size:M` — 1週間程度
- `size:L` — 2週間程度

**ステータスラベル**:
- `status:in-progress` — 作業中
- `status:done` — 完成

### 2. Issue テンプレート (`.github/ISSUE_TEMPLATE/game-spec.md`)
ゲーム実装仕様書フォーマット。Claude Code が自動入力する前提で、
チェックリスト・依頼文テンプレートを含む。

## 制約
- 現在のリモートは `cc_base` テンプレートリポジトリ
- スクリプトは `--repo` を指定せずカレントリポジトリで動作する形式にする
- 冪等性を保つ（既存ラベルを上書き: `--force` オプション使用）
