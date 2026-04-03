---
name: setup-game-issues
description: cc_game_ideas のTSVデータを元に、GitHub Issuesに詳細仕様を持ったゲームIssueを一括登録する。「Issue登録」「ゲームIssueを作成」「TSVからIssue作成」等のリクエストでトリガーする。新規Issueの作成専用。既存Issueの更新には update-game-issues を使う。
allowed-tools: Bash, Read, Write, Edit, Glob, Grep, Agent
---

# ゲームIssue一括セットアップ

TSVデータからゲームIssueをGitHubに一括登録するスキル。

**引数:** なし（または登録範囲 例: `/setup-game-issues 001-020`）

## 前提条件

- `gh` CLI がインストール済みで認証済み
- TSVデータが `docs/ideas/game-ideas.tsv` に存在する

## リファレンスファイル

スキル実行時に必ず以下を読み込むこと:

1. `references/issue-template-v2plus.md` — Issue bodyの正式テンプレート
2. `references/category-examples.md` — カテゴリ別の既存ゲーム参考例

## 実行フロー

### Step 1: TSVデータの確認

```bash
cat docs/ideas/game-ideas.tsv | head -5
```

TSVの形式:
```
ID	タイトル	コアメカニクス	カテゴリ	工数
001	BlockFlow	色付きブロックをスワイプして同色を全て繋げる	puzzle	S
```

### Step 2: GitHubラベルの作成（初回のみ）

以下のラベルが存在しない場合のみ作成する:

```bash
# カテゴリラベル
gh label create "category:puzzle"    --color "0075ca" --description "パズル系"       --force
gh label create "category:action"    --color "e4e669" --description "アクション系"   --force
gh label create "category:casual"    --color "0e8a16" --description "カジュアル系"   --force
gh label create "category:idle"      --color "f9d0c4" --description "放置系"         --force
gh label create "category:rhythm"    --color "d93f0b" --description "リズム系"       --force
gh label create "category:nurture"   --color "5319e7" --description "育成系"         --force
gh label create "category:unique"    --color "e4e669" --description "ユニーク系"     --force

# 工数ラベル
gh label create "size:S" --color "bfd4f2" --description "工数S: 1日程度"   --force
gh label create "size:M" --color "5319e7" --description "工数M: 1週間程度" --force
gh label create "size:L" --color "b60205" --description "工数L: 2週間程度" --force

# ステータスラベル
gh label create "status:todo"        --color "cccccc" --description "未実装"   --force
gh label create "status:in-progress" --color "fbca04" --description "実装中"   --force
gh label create "status:done"        --color "0e8a16" --description "実装完了" --force
```

### Step 3: Issue bodyの2段階生成

TSVの1行からv2+テンプレート全セクションを埋めるため、2段階で生成する。

#### Stage 1: 概要レベル生成

TSVのコアメカニクスと `references/category-examples.md` のカテゴリ特徴を参考に、以下を生成する:

- ゲーム概要（コアメカニクスを2-3文に拡張）
- コアループ（3-4ステップ）
- 画面構成
- 操作仕様・ゲームフロー（テーブル）
- クリア/ゲームオーバー条件
- スコア・評価基準

**カテゴリリファレンスの使い方:**
- 同カテゴリの参考例の「操作」「クリア条件」「UI傾向」を確認する
- カテゴリ特徴に沿った設計をする（例: idle系ならゲームオーバーなし、rhythm系ならタイミング判定）
- ただし参考例をコピーするのではなく、このゲーム固有のメカニクスに合わせてアレンジする

#### Stage 2: 深化レベル生成

Stage 1の結果を踏まえて、より深い設計セクションを生成する:

- **難易度設計（5ステージ）**: Stage 1のコアメカニクス・操作仕様を元に、各ステージで何が変わるかを具体的に設計。パラメータ変更だけでなく新ルール/要素を各ステージに追加する
- **ゲームデザインパターン**: 6種カタログからこのゲームに最も合うものを選択し、具体的な適用方法を記述
- **判断ポイント**: コアループの中でプレイヤーが「どこを」「いつ」「何を犠牲にして」意思決定するかを3つ以上
- **チュートリアル情報**: ゲーム概要と操作仕様から、初見プレイヤー向けの4テキストを生成
- **UI要素一覧**: 画面構成とスコア/難易度設計を踏まえて必要なUI要素を列挙

### Step 4: サンプルレビュー

最初の3件のIssue bodyを生成したら、**Issue作成前にユーザーに表示して確認を求める**。

- 異なるカテゴリから3件選ぶ
- 「以下の3件のIssue bodyを確認してください。OKなら残りを一括作成します。」と伝える
- フィードバックがあれば生成方法を調整してから残りを処理

### Step 5: Issue登録（冪等設計）

```bash
# 既存Issue一覧を取得
EXISTING=$(gh issue list --state all --limit 300 --json title -q '.[].title')

# 冪等チェック: タイトルが既に存在する場合はスキップ
ISSUE_TITLE="[${PADDED_ID}] ${TITLE} (工数: ${SIZE})"
if echo "$EXISTING" | grep -qF "$ISSUE_TITLE"; then
  echo "スキップ（既存）: $ISSUE_TITLE"
  continue
fi

# Issue作成
gh issue create \
  --title "$ISSUE_TITLE" \
  --body "$BODY" \
  --label "category:${CATEGORY},size:${SIZE},status:todo"

sleep 1  # レート制限対策
```

### Step 6: GameRegistry.json の更新

未登録のゲームを `implemented: false` で追加する。既存の `implemented: true` は保持する。

## 注意事項

- 1秒間隔でAPIリクエストを送信（レート制限対策）
- 既存Issueがある場合はスキップ（冪等）
- テンプレートのセクションをプレースホルダーのまま残さない
- カテゴリリファレンスは参考にするが、コピーしない
