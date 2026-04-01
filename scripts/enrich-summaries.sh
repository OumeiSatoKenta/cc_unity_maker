#!/usr/bin/env bash
# game-summaries.jsonl の各エントリに新フィールドを追加するスクリプト
# 既存フィールド（operations, clear_condition 等）がない行にのみ空のデフォルトを追加
# 使い方: bash scripts/enrich-summaries.sh
#
# 新フィールドの手動入力:
#   game-summaries.jsonl を直接編集して以下のフィールドを追記する
#   - clear_condition: "全敵撃破" 等のクリア条件テキスト
#   - gameover_condition: "ライフ0" 等のゲームオーバー条件テキスト
#   - score_criteria: "★3=1手、★2=2手、★1=3手" 等
#   - operations: [{"name":"発射","input":"ボタン","after":"刃が1周半回転"}]
#   - ui_elements: [{"name":"スコア","type":"Text","range":"0〜","note":""}]
#   - unity_notes: "Rigidbody2D.Static では OnCollisionEnter2D が発火しない" 等

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
FILE="${SCRIPT_DIR}/game-summaries.jsonl"
TMPFILE="${FILE}.tmp"

if ! command -v jq &>/dev/null; then
  echo "❌ jq がインストールされていません。'brew install jq' を実行してください。"
  exit 1
fi

if [[ ! -f "$FILE" ]]; then
  echo "❌ ${FILE} が見つかりません。"
  exit 1
fi

UPDATED=0
while IFS= read -r line; do
  # 新フィールドが存在しない場合のみデフォルト値を追加
  ENRICHED=$(echo "$line" | jq -c '
    . +
    (if has("clear_condition") then {} else {"clear_condition": ""} end) +
    (if has("gameover_condition") then {} else {"gameover_condition": ""} end) +
    (if has("score_criteria") then {} else {"score_criteria": ""} end) +
    (if has("operations") then {} else {"operations": []} end) +
    (if has("ui_elements") then {} else {"ui_elements": []} end) +
    (if has("unity_notes") then {} else {"unity_notes": ""} end)
  ')
  echo "$ENRICHED" >> "$TMPFILE"
  UPDATED=$((UPDATED + 1))
done < "$FILE"

mv "$TMPFILE" "$FILE"
echo "✅ ${UPDATED} エントリにデフォルトフィールドを追加しました"
echo ""
echo "次のステップ:"
echo "  1. scripts/game-summaries.jsonl を編集して 033〜055 の新フィールドを埋める"
echo "  2. bash scripts/update-issue-summaries.sh を実行して Issue を更新する"
