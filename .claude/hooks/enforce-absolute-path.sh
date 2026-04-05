#!/bin/bash
# Claude Code PreToolUse hook: Write/Edit/Read ツールで相対パスが使われたらブロック
#
# 相対パス（/で始まらないfile_path）を検出し、絶対パスを使うよう指示する。
#
# stdin: Claude Code から渡される JSON (tool_input.file_path を含む)
# stdout: permissionDecision を含む JSON (ブロック時のみ)

set -e

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

# file_path が空なら許可
if [[ -z "$FILE_PATH" ]]; then
  exit 0
fi

# 絶対パス（/で始まる）なら許可
if [[ "$FILE_PATH" == /* ]]; then
  exit 0
fi

# 相対パスが検出された場合、絶対パスに変換した候補を提示してブロック
ABSOLUTE_PATH="$(cd "${CLAUDE_PROJECT_DIR:-.}" && realpath -m "$FILE_PATH" 2>/dev/null || echo "${CLAUDE_PROJECT_DIR:-$(pwd)}/$FILE_PATH")"

jq -n \
  --arg rel "$FILE_PATH" \
  --arg abs "$ABSOLUTE_PATH" \
  '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "deny",
      permissionDecisionReason: ("相対パスは禁止されています。絶対パスを使用してください: " + $rel + " → " + $abs)
    }
  }'

exit 0
