#!/bin/bash
# Claude Code PreToolUse hook: terraform/terragrunt の破壊的コマンドをブロック
#
# ブロック対象:
#   - terraform apply / terraform destroy
#   - terragrunt apply / terragrunt destroy
#   - terragrunt run-all apply / terragrunt run-all destroy
#   - aws-vault exec ... terraform apply/destroy (コマンド文字列全体をスキャン)
#
# 許可:
#   - terraform plan, validate, init, fmt, show, output, state, workspace 等
#   - terragrunt plan, validate, init 等
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

# terraform/terragrunt の apply または destroy を検出
# run-all 経由も含む: terragrunt run-all apply/destroy
if echo "$COMMAND" | grep -qE '(terraform|terragrunt)[[:space:]]+(run-all[[:space:]]+)?(apply|destroy)'; then
  MATCHED=$(echo "$COMMAND" | grep -oE '(terraform|terragrunt)[[:space:]]+(run-all[[:space:]]+)?(apply|destroy)' | head -1)
  jq -n \
    --arg reason "${MATCHED} の実行は禁止されています。plan までの操作のみ許可されています。" \
    '{
      hookSpecificOutput: {
        hookEventName: "PreToolUse",
        permissionDecision: "deny",
        permissionDecisionReason: $reason
      }
    }'
fi

exit 0
