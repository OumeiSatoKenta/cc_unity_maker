---
description: Unityミニゲームを1本、Issue仕様に基づいて完全自動で実装する
---

# Unityミニゲーム実装 (完全自動実行モード)

**重要:** このワークフローは、ユーザーの介入なしに、開始から完了まで完全に自動で実行されるように設計されています。各ステップは完了後、ただちに次のステップへ移行してください。

**引数:** ゲームID (例: `/implement-game 002`)

---

## ステップ1: ゲーム情報の取得

引数で受け取ったゲームID（例: `002`）を使って、以下を収集する。

### 1-1. GameRegistry.json からゲーム基本情報を取得

```bash
cat MiniGameCollection/Assets/Resources/GameRegistry.json
```

対象ゲームの `id`, `title`, `category`, `sceneName`, `description` を特定する。
該当ゲームが `implemented: true` の場合は警告を出してユーザーに確認を求める。

### 1-2. GitHub Issue から仕様を取得

```bash
# ゲームIDに対応するIssueを検索（タイトルに "[00X]" を含む）
gh issue list --state open --limit 200 --json number,title,body \
  | python3 -c "
import json, sys
issues = json.load(sys.stdin)
target = '[GAME_ID_PADDED]'  # 例: [002]
found = [i for i in issues if target in i['title']]
print(json.dumps(found, ensure_ascii=False, indent=2))
"
```

Issue のボディからコアループ、画面構成、操作仕様を読み込む。

---

## ステップ2: ディレクトリ構造の確認

既存ゲーム（001_BlockFlow）を参考パターンとして確認する。

```
MiniGameCollection/Assets/
├── Scripts/Game001_BlockFlow/   ← 参考: スクリプト構成
├── Editor/SceneSetup/           ← 参考: SceneSetup構成
└── Sprites/Game001_BlockFlow/   ← 参考: アセット配置
```

新ゲーム用ディレクトリ:
- `MiniGameCollection/Assets/Scripts/Game[ID]_[Title]/`
- `MiniGameCollection/Assets/Editor/SceneSetup/`（既存に追加）
- `MiniGameCollection/Assets/Sprites/Game[ID]_[Title]/`（後でアセット生成時に使用）

---

## ステップ3: C# スクリプト群の生成

以下のファイルを生成する。ゲームの仕様に合わせて内容を設計すること。

### 3-1. ゲームロジック スクリプト群

**`[GameTitle]GameManager.cs`**
- namespace: `Game[ID]_[Title]` 形式
- ゲーム状態管理（Playing / Clear / GameOver）
- スコア・クリア条件の管理
- 参照: コアメカニクスManager, UI

**コアメカニクス Manager**（ゲームによって名前は変わる）
- ゲームのコアルール実装
- **入力処理は必ずこのManagerに一元管理する**（個別オブジェクトで処理しない）
- 新Input System使用: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint`
- `Input.mousePosition` は使わない → `Mouse.current.position.ReadValue()`
- using: `using UnityEngine.InputSystem;`

**個別オブジェクト制御スクリプト**（ブロック、タイル等）
- データ保持（位置、状態、色等）と表示のみを担当
- 入力処理は持たない
- `Resources.Load<Sprite>()` でスプライトを読み込む

**`[GameTitle]UI.cs`**
- スコア・手数・クリアパネル表示
- ボタンのUnityEvent登録

### 3-2. SceneSetup Editor スクリプト

`Setup[ID]_[Title].cs` を `Assets/Editor/SceneSetup/` に作成する。

**必須の実装ルール:**
- `[MenuItem("Assets/Setup/[ID] [Title]")]` で登録
- `EditorApplication.isPlaying` チェック
- `EditorSceneManager.NewScene()` でシーン作成
- Sprite は `File.WriteAllBytes` → `AssetDatabase.ImportAsset` → `AssetDatabase.LoadAssetAtPath<Sprite>()` で保存（`Sprite.Create()` はプレハブに保持できないため使わない）
- EventSystem は `InputSystemUIInputModule` を使用（`StandaloneInputModule` は使わない）
  - `using UnityEngine.InputSystem.UI;`
- カメラ、Canvas、UI要素、GameManager、コアMechanism全てを自動構成
- 最後に `EditorSceneManager.SaveScene()` → `AddSceneToBuildSettings()`

---

## ステップ4: GameRegistry.json の更新

```json
{
  "id": "[ID]",
  "title": "[Title]",
  "category": "[category]",
  "size": "[size]",
  "sceneName": "[ID]_[Title]",
  "description": "[description]",
  "implemented": true
}
```

`implemented: false` → `true` に更新する。

---

## ステップ5: アセットの生成（Pillow方式）

Gemini CLI の無料枠は限られるため、**Python + Pillow で直接スプライトを描画する**。

スプライト出力先: `MiniGameCollection/Assets/Resources/Sprites/Game[ID]_[Title]/`

ゲームの内容に合わせた Python スクリプトを書いて実行:

```bash
python3 -c "
from PIL import Image, ImageDraw
import os

output_dir = 'MiniGameCollection/Assets/Resources/Sprites/Game[ID]_[Title]'
os.makedirs(output_dir, exist_ok=True)

# ゲームに必要なスプライトを生成
# 例: タイル、背景、UI要素等

print('アセット生成完了')
"
```

Pillow がない場合: `pip3 install Pillow` を実行してからリトライ。

---

## ステップ6: 動作確認用チェックリスト（ユーザーへの案内）

全ファイル生成後、以下をユーザーに伝える:

```
実装が完了しました！Unity Editor で以下を実行してください:

1. Unity Editor を開く（MiniGameCollection プロジェクト）
2. メニュー: Assets > Setup > [ID] [Title] を実行
3. Play ボタンでゲームを確認

確認ポイント:
- [ ] シーンが正常に開く
- [ ] ゲームオブジェクトが表示される
- [ ] 操作（クリック/ドラッグ）が反応する
- [ ] クリア/ゲームオーバー条件が動作する
- [ ] 「メニューへ戻る」ボタンが機能する
```

---

## ステップ7: ブランチ作成 → commit → PR作成 → main マージ

```bash
# 1. main を最新化してからブランチを切る
git checkout main
git pull origin main

# 2. フィーチャーブランチ作成
git checkout -b feature/[YYYYMMDD]-game[ID]-[title-lowercase]

# 3. ステージング & コミット
git add MiniGameCollection/Assets/Scripts/Game[ID]_[Title]/
git add MiniGameCollection/Assets/Editor/SceneSetup/Setup[ID]_[Title].cs
git add MiniGameCollection/Assets/Resources/Sprites/Game[ID]_[Title]/
git add MiniGameCollection/Assets/Resources/GameRegistry.json
git commit -m "feat(game[ID]): implement [Title] game" \
  --trailer "Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"

# 4. フィーチャーブランチをリモートにプッシュ
git push -u origin feature/[YYYYMMDD]-game[ID]-[title-lowercase]

# 5. PR を作成してそのままマージ（レビュアー不要・自動実行のため）
gh pr create \
  --title "feat(game[ID]): implement [Title] game" \
  --body "## 概要
- [Description from GameRegistry.json]

## 実装内容
- C#スクリプト群: \`Scripts/Game[ID]_[Title]/\`
- SceneSetup: \`Editor/SceneSetup/Setup[ID]_[Title].cs\`
- スプライト: \`Resources/Sprites/Game[ID]_[Title]/\`
- GameRegistry.json 更新 (implemented: true)

Closes #[ISSUE_NUMBER]

🤖 Generated with [Claude Code](https://claude.com/claude-code)" \
  --base main

gh pr merge --merge --auto

# 6. main に戻り最新を pull（次のゲームのために）
git checkout main
git pull origin main
```

---

## ステップ8: GitHub Issue の更新

```bash
# Issueにコメントを追加
gh issue comment [ISSUE_NUMBER] --body "## 実装完了

- C#スクリプト生成: ✅
- SceneSetup Editorスクリプト: ✅
- アセット生成: ✅
- GameRegistry.json 更新: ✅

Unity Editor で Assets > Setup > [ID] [Title] を実行してシーンを構成してください。"
```

---

## 完了条件

- [ ] C#スクリプト群が `Scripts/Game[ID]_[Title]/` に存在する
- [ ] SceneSetup スクリプトが `Editor/SceneSetup/Setup[ID]_[Title].cs` に存在する
- [ ] スプライト画像が `Resources/Sprites/Game[ID]_[Title]/` に存在する
- [ ] `GameRegistry.json` の該当ゲームが `implemented: true`
- [ ] feature ブランチ作成・commit・push 完了
- [ ] PR 作成 & main へマージ完了
- [ ] ローカル main を pull して最新化済み
- [ ] GitHub Issue にコメント済み

---

## 参考: 001_BlockFlow の構成

001_BlockFlow は実装済みなので、以下を参考パターンとして活用すること:
- `MiniGameCollection/Assets/Scripts/Game001_BlockFlow/`
- `MiniGameCollection/Assets/Editor/SceneSetup/Setup001_BlockFlow.cs`
- `MiniGameCollection/Assets/Resources/Sprites/Game001_BlockFlow/`
