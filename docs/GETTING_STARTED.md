# はじめてのセットアップガイド

このガイドに従えば、プログラミング経験がなくても最初のゲームを完成させることができます。

---

## 必要なもの

| ツール | 用途 | 入手先 |
|--------|------|--------|
| Unity Hub | Unityプロジェクトの管理 | https://unity.com/ja/download |
| Unity 6 (6000.x LTS) | ゲーム開発エンジン | Unity Hub からインストール |
| Git | バージョン管理 | https://git-scm.com/ |
| GitHub アカウント | ソースコード・Issue管理 | https://github.com/ |
| GitHub CLI (gh) | ターミナルからGitHub操作 | https://cli.github.com/ |
| Claude Code | AI開発アシスタント | https://claude.ai/code |

---

## Step 1: リポジトリをクローンする

ターミナル（Mac: Terminal.app / Windows: PowerShell）を開いて以下を実行します。

```bash
git clone https://github.com/OumeiSatoKenta/cc_unity_maker.git
cd cc_unity_maker
```

---

## Step 2: GitHub CLI にログインする

```bash
gh auth login
```

画面の指示に従ってブラウザでGitHubにログインします。
完了後、以下で確認できます:

```bash
gh auth status
# → ✓ Logged in to github.com が表示されればOK
```

---

## Step 3: Unity Hub でプロジェクトを開く

1. **Unity Hub** を起動する
2. 左メニューの **Projects** をクリック
3. 右上の **Add** ボタン → **Add project from disk** を選択
4. `cc_unity_maker/MiniGameCollection` フォルダを選択
5. Unity 6 (6000.x) がインストールされていなければ、Unity Hub が自動で案内するのでインストール
6. プロジェクト名「MiniGameCollection」をクリックして開く（初回は数分かかります）

---

## Step 4: 日本語フォントをセットアップする（初回のみ）

Unity Editor が開いたら:

1. 上部メニューの **Assets** をクリック
2. **Setup** → **Generate Japanese Font** を選択
3. Console に「フォントアセットを作成しました」と表示されれば成功

---

## Step 5: TopMenu シーンを作成する（初回のみ）

1. 上部メニューの **Assets** をクリック
2. **Setup** → **TopMenu** を選択
3. Console に「TopMenuシーンを作成しました」と表示されれば成功

**Play ボタン（▶）** を押して確認してみましょう。
カテゴリタブとゲームカードの一覧が表示されれば OK です。

---

## Step 6: Claude Code を起動する

ターミナルで `cc_unity_maker` ディレクトリに移動して:

```bash
claude
```

Claude Code が起動したら、日本語で話しかけるだけで OK です。

---

## Step 7: 最初のゲームを作る

### 7-1. GitHub Projects でゲームを選ぶ

1. https://github.com/OumeiSatoKenta/cc_unity_maker/issues を開く
2. ラベルで `size:S` を絞り込む（1日で作れるゲーム）
3. 気になるゲームのIssueを開いて内容を確認

### 7-2. Claude Code に依頼する

例: 001 BlockFlow を作る場合

```
ゲーム001 BlockFlow を作って
```

Claude Code が以下を自動で行います:
- C# スクリプトの生成
- Editor スクリプト（SceneSetup）の生成
- GameRegistry.json の更新
- Git へのコミット・プッシュ

### 7-3. Unity Editor でシーンを構成する

1. **Play を停止** する（再生中は実行できません）
2. 上部メニュー **Assets** → **Setup** → **001 BlockFlow** を選択
3. Console に「シーンを作成しました」と表示されれば成功

### 7-4. Play で動作確認する

1. **Play ボタン（▶）** を押す
2. ゲームが動くか確認する
3. 問題があれば Claude Code に伝える:
   - 「ブロックが動かない」
   - 「クリアしても画面が変わらない」
   - 「もっと速くしたい」

### 7-5. 完成したら

1. GitHub Issues で該当ゲームの Issue をクローズする
2. 次のゲームへ！

---

## 困ったときは

### Unity Editor で赤いエラーが出る

Claude Code に状況をそのまま伝えてください:
```
Unity を開いたら赤いエラーがたくさん出た
```

### Play ボタンを押しても何も動かない

正しいシーンが開かれているか確認してください:
- **File** → **Open Scene** → `Assets/Scenes/001_BlockFlow.unity` を選択

### Assets > Setup にメニューが見つからない

Unity Editor が C# スクリプトのコンパイル中の可能性があります。
画面右下にスピナー（回転アイコン）が表示されていれば、消えるまで待ってください。

### Git でエラーが出る

```bash
# 現在の状態を確認
git status

# Claude Code に聞く
「git status でこのエラーが出た: （エラー内容を貼り付け）」
```

---

## ゲーム追加の全体フロー（まとめ）

```
1. GitHub Issues でゲームを選ぶ
      ↓
2. Claude Code に「ゲームXXX を作って」と依頼
      ↓
3. Unity Editor で Assets > Setup > XXX を実行
      ↓
4. Play ボタンで動作確認
      ↓
5. 問題があれば Claude Code に伝えて修正
      ↓
6. 完成！Issue をクローズして次のゲームへ
```

---

## 用語がわからないとき

`docs/glossary.md` に主要な用語がまとまっています。
