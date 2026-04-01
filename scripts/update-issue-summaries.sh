#!/usr/bin/env bash
# 全100ゲームのIssue本文を概要付きで更新するスクリプト
# 使い方: bash scripts/update-issue-summaries.sh
# game-summaries.jsonl を読み込んでIssue本文を更新する

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SUMMARIES_FILE="${SCRIPT_DIR}/game-summaries.jsonl"
TOTAL=100
UPDATED=0

if ! gh auth status &>/dev/null; then
  echo "❌ GitHub CLI が認証されていません。"
  exit 1
fi

if [[ ! -f "$SUMMARIES_FILE" ]]; then
  echo "❌ ${SUMMARIES_FILE} が見つかりません。"
  exit 1
fi

# jq がインストールされているか確認
if ! command -v jq &>/dev/null; then
  echo "❌ jq がインストールされていません。'brew install jq' を実行してください。"
  exit 1
fi

echo "📝 全${TOTAL}ゲームのIssue概要を更新します..."
echo ""

# Issue番号とタイトルの対応を取得
echo "📋 既存Issueを取得中..."
ISSUES_JSON=$(gh issue list --state all --limit 200 --json number,title)

CURRENT=0
while IFS= read -r line; do
  CURRENT=$((CURRENT + 1))

  ID=$(echo "$line" | jq -r '.id')
  TITLE=$(echo "$line" | jq -r '.title')
  OVERVIEW=$(echo "$line" | jq -r '.overview')
  CORE1=$(echo "$line" | jq -r '.core_loop[0]')
  CORE2=$(echo "$line" | jq -r '.core_loop[1]')
  CORE3=$(echo "$line" | jq -r '.core_loop[2]')
  SCREEN1=$(echo "$line" | jq -r '.screens[0]')
  SCREEN2=$(echo "$line" | jq -r '.screens[1]')
  SCREEN3=$(echo "$line" | jq -r '.screens[2]')
  SCREEN4=$(echo "$line" | jq -r '.screens[3] // empty')

  # カテゴリと工数を game-data.tsv から取得
  TSV_LINE=$(grep "^${ID}" "${SCRIPT_DIR}/game-data.tsv" || true)
  if [[ -z "$TSV_LINE" ]]; then
    echo "  ⚠️  [${CURRENT}/${TOTAL}] ID ${ID} がgame-data.tsvに見つかりません。スキップ"
    continue
  fi
  CATEGORY=$(echo "$TSV_LINE" | cut -f4)
  SIZE=$(echo "$TSV_LINE" | cut -f5)

  # Issueタイトルで番号を検索
  ISSUE_TITLE="[${ID}] ${TITLE} (工数: ${SIZE})"
  ISSUE_NUMBER=$(echo "$ISSUES_JSON" | jq -r --arg title "$ISSUE_TITLE" '.[] | select(.title == $title) | .number')

  if [[ -z "$ISSUE_NUMBER" ]]; then
    echo "  ⚠️  [${CURRENT}/${TOTAL}] ${ISSUE_TITLE} のIssueが見つかりません。スキップ"
    continue
  fi

  # 画面構成リスト
  SCREENS_LIST="- ${SCREEN1}
- ${SCREEN2}
- ${SCREEN3}"
  if [[ -n "$SCREEN4" ]]; then
    SCREENS_LIST="${SCREENS_LIST}
- ${SCREEN4}"
  fi

  # game-summaries.jsonl の拡張フィールドを読み取る（存在しない場合は空文字）
  CLEAR_COND=$(echo "$line" | jq -r '.clear_condition // empty')
  GAMEOVER_COND=$(echo "$line" | jq -r '.gameover_condition // empty')
  SCORE_CRITERIA=$(echo "$line" | jq -r '.score_criteria // empty')

  # 操作フローテーブル行を生成
  OPS_TABLE=""
  OPS_COUNT=$(echo "$line" | jq -r '.operations | length // 0' 2>/dev/null || echo "0")
  if [[ "$OPS_COUNT" -gt 0 ]]; then
    for i in $(seq 0 $((OPS_COUNT - 1))); do
      OP_NAME=$(echo "$line" | jq -r ".operations[$i].name // empty")
      OP_INPUT=$(echo "$line" | jq -r ".operations[$i].input // empty")
      OP_AFTER=$(echo "$line" | jq -r ".operations[$i].after // empty")
      OPS_TABLE="${OPS_TABLE}
| ${OP_NAME} | ${OP_INPUT} | ${OP_AFTER} |"
    done
  fi

  # UI要素テーブル行を生成
  UI_TABLE=""
  UI_COUNT=$(echo "$line" | jq -r '.ui_elements | length // 0' 2>/dev/null || echo "0")
  if [[ "$UI_COUNT" -gt 0 ]]; then
    for i in $(seq 0 $((UI_COUNT - 1))); do
      UI_NAME=$(echo "$line" | jq -r ".ui_elements[$i].name // empty")
      UI_TYPE=$(echo "$line" | jq -r ".ui_elements[$i].type // empty")
      UI_RANGE=$(echo "$line" | jq -r ".ui_elements[$i].range // empty")
      UI_NOTE=$(echo "$line" | jq -r ".ui_elements[$i].note // empty")
      UI_TABLE="${UI_TABLE}
| ${UI_NAME} | ${UI_TYPE} | ${UI_RANGE} | ${UI_NOTE} |"
    done
  fi

  UNITY_NOTES=$(echo "$line" | jq -r '.unity_notes // empty')

  # Issue本文を生成
  BODY="## ゲーム情報

| 項目 | 内容 |
|------|------|
| ID | \`${ID}\` |
| タイトル | ${TITLE} |
| カテゴリ | ${CATEGORY} |
| 工数 | ${SIZE} |
| シーン名 | \`${ID}_${TITLE}\` |

---

## ゲーム概要

${OVERVIEW}

---

## コアループ

1. ${CORE1}
2. ${CORE2}
3. ${CORE3}

---

## 画面構成

${SCREENS_LIST}

---

## 操作仕様・ゲームフロー

| 操作 | 入力方法 | 操作後の動作 |
|------|---------|-------------|${OPS_TABLE:-"
| | | |"}

---

## クリア / ゲームオーバー条件

**クリア条件**: ${CLEAR_COND}

**ゲームオーバー条件**: ${GAMEOVER_COND}

---

## スコア・評価基準

${SCORE_CRITERIA:-"<!-- 実装時に決定 -->"}

---

## UI 要素一覧

| 要素 | 種類 | 初期値/範囲 | 備考 |
|------|------|-----------|------|${UI_TABLE:-"
| | | | |"}

---

## 必要な GameObject 一覧

<!-- Claude Codeが仕様展開時に記入 -->

| 名前 | 役割 | 主なコンポーネント | スクリプト |
|------|------|------------------|-----------|
| | | | |

---

## Unity 実装上の注意

${UNITY_NOTES:-"<!-- Claude Codeが仕様展開時に記入 -->"}

---

## Claude Code への依頼文（コピペ用）

\`\`\`
ゲーム${ID} ${TITLE} を作って
\`\`\`

---

## 実装チェックリスト

- [ ] プロジェクト生成・スクリプト追加完了
- [ ] SceneSetup スクリプト作成完了
- [ ] GameRegistry.json に追加完了（implemented: true）
- [ ] GitHub に push 完了

## 動作確認チェックリスト

- [ ] TopMenu のカテゴリタブにゲームカードが表示される
- [ ] カードをタップするとゲームシーンに遷移する
- [ ] Play でゲームが起動する（クラッシュしない）
- [ ] コアメカニクスが動作する
- [ ] クリア / ゲームオーバーが発動する
- [ ] 「メニューへ戻る」で TopMenu に戻れる
- [ ] Console にエラーログが出ない"

  # 一時ファイルに書き出してから更新
  TMPFILE=$(mktemp)
  echo "$BODY" > "$TMPFILE"
  gh issue edit "$ISSUE_NUMBER" --body-file "$TMPFILE" > /dev/null
  rm -f "$TMPFILE"

  UPDATED=$((UPDATED + 1))
  echo "  ✅ [${CURRENT}/${TOTAL}] #${ISSUE_NUMBER} ${ISSUE_TITLE}"

  # レート制限対策
  sleep 1
done < "$SUMMARIES_FILE"

echo ""
echo "🎉 ${UPDATED}件のIssueを更新しました！"
