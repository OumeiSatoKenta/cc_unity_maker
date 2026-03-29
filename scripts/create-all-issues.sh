#!/usr/bin/env bash
# 全100ゲームのGitHub Issueを一括登録するスクリプト
# 使い方: bash scripts/create-all-issues.sh
# 冪等実行可能（同タイトルのIssueが存在する場合はスキップ）

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DATA_FILE="${SCRIPT_DIR}/game-data.tsv"
TOTAL=100
CREATED=0
SKIPPED=0

# 認証確認
if ! gh auth status &>/dev/null; then
  echo "❌ GitHub CLI が認証されていません。'gh auth login' を実行してください。"
  exit 1
fi

# データファイル確認
if [[ ! -f "$DATA_FILE" ]]; then
  echo "❌ ${DATA_FILE} が見つかりません。"
  exit 1
fi

echo "🎮 全${TOTAL}ゲームのIssue一括登録を開始します..."
echo ""

# 既存Issueのタイトル一覧を取得（冪等性チェック用）
echo "📋 既存Issueを確認中..."
EXISTING_ISSUES=$(gh issue list --state all --limit 200 --json title -q '.[].title' 2>/dev/null || echo "")

# ヘッダー行をスキップしてデータを読み込み
CURRENT=0
tail -n +2 "$DATA_FILE" | while IFS=$'\t' read -r ID TITLE MECHANICS CATEGORY SIZE; do
  CURRENT=$((CURRENT + 1))

  # Issueタイトルを構築
  ISSUE_TITLE="[${ID}] ${TITLE} (工数: ${SIZE})"

  # 既存チェック
  if echo "$EXISTING_ISSUES" | grep -qF "$ISSUE_TITLE"; then
    echo "  ⏭️  [${CURRENT}/${TOTAL}] ${ISSUE_TITLE} — スキップ（既存）"
    continue
  fi

  # Issue本文を生成
  BODY=$(cat <<ISSUEEOF
## ゲーム情報

| 項目 | 内容 |
|------|------|
| ID | \`${ID}\` |
| タイトル | ${TITLE} |
| カテゴリ | ${CATEGORY} |
| 工数 | ${SIZE} |
| シーン名 | \`${ID}_${TITLE}\` |

---

## ゲーム概要

${MECHANICS}

---

## コアループ

<!-- Claude Codeが仕様展開時に記入 -->
1.
2.
3.

---

## 必要な GameObject 一覧

<!-- Claude Codeが仕様展開時に記入 -->

| 名前 | 役割 | 主なコンポーネント | スクリプト |
|------|------|------------------|-----------|
| | | | |

---

## クリア / ゲームオーバー条件

<!-- Claude Codeが仕様展開時に記入 -->

**クリア条件**:

**ゲームオーバー条件**:

---

## Unity 実装方針

<!-- Claude Codeが仕様展開時に記入 -->

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
- [ ] Console にエラーログが出ない
ISSUEEOF
  )

  # Issue作成
  gh issue create \
    --title "$ISSUE_TITLE" \
    --body "$BODY" \
    --label "category:${CATEGORY},size:${SIZE}" \
    > /dev/null

  echo "  ✅ [${CURRENT}/${TOTAL}] ${ISSUE_TITLE}"

  # レート制限対策（1秒スリープ）
  sleep 1
done

echo ""
echo "🎉 Issue一括登録が完了しました！"
echo ""
echo "確認: gh issue list --limit 10"
