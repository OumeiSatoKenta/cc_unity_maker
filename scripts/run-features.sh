#!/bin/bash
# add-feature を連番で実行し、進捗をターミナルに表示するスクリプト
#
# 使い方:
#   ./scripts/run-features.sh                  # 001 から開始
#   ./scripts/run-features.sh 5                # 005 から開始
#   ./scripts/run-features.sh 3 7              # 003 〜 007 を実行
#   START=2 END=10 ./scripts/run-features.sh   # 環境変数でも指定可

set -euo pipefail

START_NUM="${1:-${START:-1}}"
END_NUM="${2:-${END:-$START_NUM}}"
LOG_DIR="logs/features"

mkdir -p "$LOG_DIR"

for i in $(seq "$START_NUM" "$END_NUM"); do
  NUM=$(printf "%03d" "$i")
  LOG_FILE="${LOG_DIR}/feature-${NUM}.jsonl"

  echo ""
  echo "=========================================="
  echo "  add-feature ${NUM}  (log: ${LOG_FILE})"
  echo "=========================================="
  echo ""

  claude -p "/add-feature ${NUM}" \
    --output-format stream-json \
    --verbose | \
    tee "$LOG_FILE" | \
    jq -rj 'select(.type == "stream_event" and .event.delta.type? == "text_delta") | .event.delta.text'

  echo ""
  echo "--- feature ${NUM} 完了 ---"
  echo ""
done

echo "全タスク完了 (${START_NUM} 〜 ${END_NUM})"
