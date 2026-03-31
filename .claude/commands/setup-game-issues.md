---
description: cc_game_ideas のTSVデータを元に、GitHub Issuesに詳細仕様を持ったゲームIssueを一括登録する
---

# ゲームIssue一括セットアップ

**重要:** このワークフローは、ユーザーの介入なしに完全自動で実行されます。

**引数:** なし（または登録範囲 例: `/setup-game-issues 001-020`）

---

## ステップ1: ゲームアイデアデータの確認

`docs/ideas/` または以下のパスにTSVデータが存在するか確認する。

```bash
ls docs/ideas/
cat docs/ideas/game-ideas.tsv  # または類似ファイル
```

TSVの形式:
```
ID	タイトル	コアメカニクス	カテゴリ	工数
001	BlockFlow	色付きブロックをスワイプして同色を全て繋げる	puzzle	S
```

---

## ステップ2: GitHubラベルの作成（初回のみ）

```bash
# カテゴリラベル
gh label create "category:puzzle"    --color "0075ca" --description "パズル系 (001-020)"     --force
gh label create "category:action"    --color "e4e669" --description "アクション系 (021-040)" --force
gh label create "category:casual"    --color "0e8a16" --description "カジュアル系 (041-056)" --force
gh label create "category:idle"      --color "f9d0c4" --description "放置系 (057-066)"       --force
gh label create "category:rhythm"    --color "d93f0b" --description "リズム系 (067-076)"     --force
gh label create "category:nurture"   --color "5319e7" --description "育成系 (077-086)"       --force
gh label create "category:unique"    --color "e4e669" --description "ユニーク系 (087-100)"   --force

# 工数ラベル
gh label create "size:S" --color "bfd4f2" --description "工数S: 1日程度"   --force
gh label create "size:M" --color "5319e7" --description "工数M: 1週間程度" --force
gh label create "size:L" --color "b60205" --description "工数L: 2週間程度" --force

# ステータスラベル
gh label create "status:todo"        --color "cccccc" --description "未実装"   --force
gh label create "status:in-progress" --color "fbca04" --description "実装中"   --force
gh label create "status:done"        --color "0e8a16" --description "実装完了" --force
```

---

## ステップ3: Issue一括登録（冪等設計）

既存Issueと重複しないよう冪等に登録する。

```bash
# 既存Issue一覧を取得
EXISTING=$(gh issue list --state all --limit 200 --json title -q '.[].title')

# TSVの各行を処理
while IFS=$'\t' read -r ID TITLE MECHANICS CATEGORY SIZE; do
  PADDED_ID=$(printf '%03d' "$ID")
  ISSUE_TITLE="[${PADDED_ID}] ${TITLE} (工数: ${SIZE})"

  # 冪等チェック
  if echo "$EXISTING" | grep -qF "$ISSUE_TITLE"; then
    echo "⏭️  スキップ（既存）: $ISSUE_TITLE"
    continue
  fi

  BODY="## ゲーム概要

**コアメカニクス:** ${MECHANICS}
**カテゴリ:** ${CATEGORY}
**工数:** ${SIZE}

## 実装仕様

（Claude Codeが /implement-game ${PADDED_ID} を実行して自動生成します）

## チェックリスト

- [ ] C#スクリプト生成
- [ ] SceneSetupスクリプト作成
- [ ] アセット生成
- [ ] GameRegistry.json更新
- [ ] 動作確認
"

  gh issue create \
    --title "$ISSUE_TITLE" \
    --body "$BODY" \
    --label "category:${CATEGORY},size:${SIZE},status:todo"

  echo "✅ 登録: $ISSUE_TITLE"
  sleep 1  # レート制限対策

done < <(tail -n +2 docs/ideas/game-ideas.tsv)
```

---

## ステップ4: GameRegistry.json の初期化

まだ存在しない場合、全ゲームを `implemented: false` で登録する。

```bash
python3 -c "
import json

# TSVを読み込んでGameRegistry.jsonを生成
games = []
with open('docs/ideas/game-ideas.tsv') as f:
    next(f)  # ヘッダースキップ
    for line in f:
        parts = line.strip().split('\t')
        if len(parts) < 5:
            continue
        id_, title, mechanics, category, size = parts[:5]
        padded_id = id_.zfill(3)
        # タイトルをアルファベット+数字のsceneName形式に変換（簡易）
        scene_name = f'{padded_id}_{title.replace(\" \", \"\")}'
        games.append({
            'id': padded_id,
            'title': title,
            'category': category,
            'size': size,
            'sceneName': scene_name,
            'description': mechanics,
            'implemented': False
        })

registry_path = 'MiniGameCollection/Assets/Resources/GameRegistry.json'
with open(registry_path) as f:
    existing = json.load(f)

# 既存の implemented: true を保持しつつ未登録のゲームを追加
existing_ids = {g['id'] for g in existing['games']}
for g in games:
    if g['id'] not in existing_ids:
        existing['games'].append(g)

existing['games'].sort(key=lambda x: x['id'])

with open(registry_path, 'w') as f:
    json.dump(existing, f, ensure_ascii=False, indent=2)

print(f'GameRegistry.json 更新完了: {len(existing[\"games\"])}件')
"
```

---

## 完了条件

- [ ] GitHubラベルが全て作成済み
- [ ] 全ゲームのIssueが登録済み（冪等チェック済み）
- [ ] GameRegistry.json に全ゲームが `implemented: false` で登録済み
