#!/bin/bash
# Claude Code PreToolUse hook: シークレットファイルへの Bash アクセスをブロック
#
# 保護対象:
#   - .env, .env.* (プロジェクト内全階層)
#   - ~/.aws/* (credentials, config 等)
#
# ホワイトリスト (許可):
#   - .env.example の読み取り
#   - cp .env.example .env (テンプレートコピー)
#   - echo/printf による .env への書き込み (セットアップ用)
#   - git add .env.example
#
# stdin: Claude Code から渡される JSON (tool_input.command を含む)
# stdout: permissionDecision を含む JSON (ブロック時のみ)
# exit 0: 正常終了

set -e

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# コマンドが空なら許可
if [[ -z "$COMMAND" ]]; then
  exit 0
fi

# --- ホワイトリスト判定 ---

# ホワイトリスト1: cp .env.example .env (テンプレートコピー)
if echo "$COMMAND" | grep -qE '^[[:space:]]*cp[[:space:]]+\.env\.example[[:space:]]+\.env[[:space:]]*$'; then
  exit 0
fi

# ホワイトリスト2: echo/printf による .env への書き込み (リダイレクト)
# 例: echo "KEY=value" > .env, echo "KEY=value" >> .env
if echo "$COMMAND" | grep -qE '^[[:space:]]*(echo|printf)[[:space:]]+.*>+[[:space:]]*\.env([[:space:]]|$)'; then
  exit 0
fi

# --- 保護対象の検出 ---

# .env.example への参照を一時的に除去してから検査
# (.env.example は安全なので除外する)
SANITIZED=$(echo "$COMMAND" | sed -E 's/\.env\.example//g')

BLOCKED=false
REASON=""

# パターン1: .env ファイル (.env, .env.local, .env.production, path/to/.env 等)
# .env.example は既に SANITIZED から除去済みなので、単純に .env の出現を検査
if echo "$SANITIZED" | grep -qE '\.env($|[[:space:]]|\.[a-zA-Z])'; then
  BLOCKED=true
  REASON="シークレットファイル (.env*) へのアクセスは禁止されています。.env.example を使用してください。"
fi

# パターン2: ~/.aws/ または $HOME/.aws/
if echo "$COMMAND" | grep -qE '(~|(\$HOME)|(\\$HOME)|/home/[^/]+)/\.aws/'; then
  BLOCKED=true
  REASON="AWS認証情報ファイル (~/.aws/*) へのアクセスは禁止されています。"
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
