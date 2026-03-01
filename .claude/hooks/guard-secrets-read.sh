#!/bin/bash
# Claude Code PreToolUse hook: Read ツールによるシークレットファイル読み取りをブロック
#
# 保護対象:
#   - .env, .env.* (プロジェクト内全階層)
#   - ~/.aws/* (credentials, config 等)
#
# ホワイトリスト (許可):
#   - .env.example
#
# stdin: Claude Code から渡される JSON (tool_input.file_path を含む)
# stdout: permissionDecision を含む JSON (ブロック時のみ)
# exit 0: 正常終了

set -e

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

# file_path が空なら許可
if [[ -z "$FILE_PATH" ]]; then
  exit 0
fi

# ファイル名部分を取得
BASENAME=$(basename "$FILE_PATH")

# ホワイトリスト: .env.example は許可
if [[ "$BASENAME" == ".env.example" ]]; then
  exit 0
fi

BLOCKED=false
REASON=""

# パターン1: .env ファイル (.env, .env.local, .env.production 等)
if [[ "$BASENAME" == ".env" ]] || echo "$BASENAME" | grep -qE '^\.env\.'; then
  BLOCKED=true
  REASON="シークレットファイル ($BASENAME) の読み取りは禁止されています。.env.example を使用してください。"
fi

# パターン2: ~/.aws/ 配下
if echo "$FILE_PATH" | grep -qE '(^|/)\.aws/'; then
  BLOCKED=true
  REASON="AWS認証情報ファイル ($FILE_PATH) の読み取りは禁止されています。"
fi

# --- 結果出力 ---

if [[ "$BLOCKED" == "true" ]]; then
  jq -n \
    --arg reason "$REASON" \
    '{
      hookSpecificOutput: {
        hookEventName: "PreToolUse",
        permissionDecision: "deny",
        permissionDecisionReason: $reason
      }
    }'
fi

exit 0
