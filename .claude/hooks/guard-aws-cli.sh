#!/bin/bash
# Claude Code PreToolUse hook: AWS CLI の書き込み系コマンドをブロック
#
# 方針:
#   読み取り専用の操作のみ許可し、リソースの作成・更新・削除を行うコマンドをブロック。
#   aws-vault 経由の aws コマンドも同様にチェック。
#
# 許可 (読み取り系):
#   - describe-*, list-*, get-*, head-*, wait, lookup-*, check-*
#   - validate-*, verify-*, test-*, simulate-*, search-*, scan, query, select
#   - batch-get-*, download-*
#   - aws --version, aws help, aws <service> help
#   - aws configure list / aws configure get
#   - aws sts get-caller-identity, assume-role 等
#   - aws s3 ls / aws s3 presign
#
# ブロック (書き込み系):
#   - create-*, delete-*, update-*, modify-*, put-* 等の変更系アクション
#   - aws s3 cp/mv/rm/sync/mb/rb
#   - aws lambda invoke
#   - aws configure set 等
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

# --- aws CLI が含まれているか確認 ---
# 'aws ' を検出 (aws-vault は 'aws-' なのでマッチしない)
if ! echo "$COMMAND" | grep -qE '(^|[;&|[:space:]])aws[[:space:]]'; then
  exit 0
fi

# deny 出力用の関数
deny_command() {
  local reason="$1"
  jq -n \
    --arg reason "$reason" \
    '{
      hookSpecificOutput: {
        hookEventName: "PreToolUse",
        permissionDecision: "deny",
        permissionDecisionReason: $reason
      }
    }'
  exit 0
}

# --- ホワイトリスト (明示的に許可) ---

# aws --version, aws help
if echo "$COMMAND" | grep -qE 'aws[[:space:]]+(--version|help)([[:space:]]|$)'; then
  exit 0
fi

# aws <service> help
if echo "$COMMAND" | grep -qE 'aws[[:space:]]+[a-z0-9-]+[[:space:]]+help([[:space:]]|$)'; then
  exit 0
fi

# aws configure list / get (読み取りのみ)
if echo "$COMMAND" | grep -qE 'aws[[:space:]]+configure[[:space:]]+(list|get)([[:space:]]|$)'; then
  exit 0
fi

# --- 特定コマンドのブロック ---

# aws configure (set, add-model 等の変更系)
if echo "$COMMAND" | grep -qE 'aws[[:space:]]+configure[[:space:]]+(set|add-model|import|export)([[:space:]]|$)'; then
  deny_command "aws configure の変更コマンドは禁止されています。list / get のみ許可されています。"
fi

# aws s3 の書き込みコマンド (cp, mv, rm, sync, mb, rb)
if echo "$COMMAND" | grep -qE 'aws[[:space:]]+s3[[:space:]]+(cp|mv|rm|sync|mb|rb)([[:space:]]|$)'; then
  deny_command "AWS S3 の書き込みコマンドは禁止されています。読み取り専用 (ls, presign) のみ許可されています。"
fi

# aws lambda invoke (非ハイフン形式の書き込みコマンド)
if echo "$COMMAND" | grep -qE 'aws[[:space:]]+lambda[[:space:]]+invoke([[:space:]]|$)'; then
  deny_command "aws lambda invoke の実行は禁止されています。"
fi

# --- 汎用的な書き込みアクション検出 ---

# S3 パス (s3://...) を除去して誤検知を防ぐ
CLEAN_CMD=$(echo "$COMMAND" | sed 's|s3://[^ ]*||g')

# 書き込み系アクションプレフィクス (ハイフン付き形式: create-*, delete-* 等)
WRITE_PREFIXES="create|delete|remove|update|modify|put|start|stop|terminate|reboot|run|invoke|execute"
WRITE_PREFIXES="${WRITE_PREFIXES}|attach|detach|enable|disable|register|deregister"
WRITE_PREFIXES="${WRITE_PREFIXES}|authorize|revoke|associate|disassociate|allocate|release"
WRITE_PREFIXES="${WRITE_PREFIXES}|import|export|tag|untag|deploy|cancel|reset|restore"
WRITE_PREFIXES="${WRITE_PREFIXES}|send|publish|copy|move|write|purchase|request|submit"
WRITE_PREFIXES="${WRITE_PREFIXES}|replace|apply|accept|reject|close|complete|confirm"
WRITE_PREFIXES="${WRITE_PREFIXES}|batch-delete|batch-put|batch-write|batch-update"

# aws ... <write-prefix>-<noun> パターンを検出 (ハイフン付き形式)
if echo "$CLEAN_CMD" | grep -qE "aws[[:space:]]+.*[[:space:]](${WRITE_PREFIXES})-[a-zA-Z]"; then
  MATCHED=$(echo "$CLEAN_CMD" | grep -oE "(${WRITE_PREFIXES})-[a-zA-Z][a-zA-Z0-9-]*" | head -1)
  deny_command "AWS CLI の書き込みコマンド (${MATCHED}) は禁止されています。読み取り専用コマンドのみ許可されています。"
fi

# ハイフンなし形式の書き込みアクション (aws <service> <verb> の形式)
# 例: aws sns publish, aws sqs send-message (send は上でキャッチ), etc.
STANDALONE_WRITE_VERBS="publish|invoke|execute|deploy|import|export|cancel|close|submit|reject|confirm|complete"
if echo "$CLEAN_CMD" | grep -qE "aws[[:space:]]+[a-z0-9-]+[[:space:]]+(${STANDALONE_WRITE_VERBS})([[:space:]]|$)"; then
  MATCHED=$(echo "$CLEAN_CMD" | grep -oE "(${STANDALONE_WRITE_VERBS})([[:space:]]|$)" | head -1 | tr -d ' ')
  deny_command "AWS CLI の書き込みコマンド (${MATCHED}) は禁止されています。読み取り専用コマンドのみ許可されています。"
fi

exit 0
