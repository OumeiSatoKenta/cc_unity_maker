#!/usr/bin/env bash
# GitHubラベルの一括作成スクリプト
# 使い方: bash scripts/setup-labels.sh
# 冪等実行可能（既存ラベルは --force で上書き）

set -euo pipefail

# 認証確認
if ! gh auth status &>/dev/null; then
  echo "❌ GitHub CLI が認証されていません。'gh auth login' を実行してください。"
  exit 1
fi

echo "🏷️  GitHubラベルのセットアップを開始します..."
echo ""

# ---- カテゴリラベル ----
echo "📂 カテゴリラベルを作成中..."

gh label create "category:puzzle"     --color "0075ca" --description "パズル系 (001-020)"     --force
gh label create "category:action"     --color "e4e669" --description "アクション系 (021-040)" --force
gh label create "category:casual"     --color "cfd3d7" --description "カジュアル系 (041-060)" --force
gh label create "category:idle"       --color "a2eeef" --description "放置系 (061-070)"       --force
gh label create "category:rhythm"     --color "d93f0b" --description "リズム系 (071-080)"     --force
gh label create "category:simulation" --color "0e8a16" --description "育成系 (081-090)"       --force
gh label create "category:unique"     --color "7057ff" --description "ユニーク系 (091-100)"   --force

echo "✅ カテゴリラベル完了"
echo ""

# ---- 工数ラベル ----
echo "📏 工数ラベルを作成中..."

gh label create "size:S" --color "bfd4f2" --description "工数S: 1日程度"   --force
gh label create "size:M" --color "5319e7" --description "工数M: 1週間程度" --force
gh label create "size:L" --color "b60205" --description "工数L: 2週間程度" --force

echo "✅ 工数ラベル完了"
echo ""

# ---- ステータスラベル ----
echo "🚦 ステータスラベルを作成中..."

gh label create "status:in-progress" --color "fbca04" --description "作業中" --force
gh label create "status:done"        --color "0e8a16" --description "完成"   --force

echo "✅ ステータスラベル完了"
echo ""

echo "🎉 全ラベルのセットアップが完了しました！"
echo ""
echo "作成されたラベル一覧:"
gh label list --sort name
