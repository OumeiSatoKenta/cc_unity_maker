---
description: Unityミニゲームを1本、Issue仕様に基づいて完全自動で実装する
---

# Unityミニゲーム実装 (完全自動実行モード)

**重要:** このワークフローは、ユーザーの介入なしに、開始から完了まで完全に自動で実行されるように設計されています。各ステップは完了後、ただちに次のステップへ移行してください。

**引数:** ゲームID (例: `/add-feature 002`)

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

## ステップ2: 調査フェーズ（MCP・スキルの積極活用）

**重要:** このステップでは、一般知識に頼らず、利用可能なMCPサーバーとスキルを最大限に活用して正確な情報を収集すること。

### 2-1. 既存コードの調査

直近で実装済みのゲーム（GameRegistry.json で `implemented: true` かつ最も大きい ID）の以下を参照する:
- `MiniGameCollection/Assets/Scripts/Game[直近ID]_[Title]/`
- `MiniGameCollection/Assets/Editor/SceneSetup/Setup[直近ID]_[Title].cs`

> **注意**: Game001_BlockFlow は毎回読まない。直近1本のみ参照すればパターンは十分。

### 2-2. 外部ドキュメント・技術調査（MCP・スキル活用）

Unity の API で不明な点があれば、**CLAUDE.md の「MCP・スキルの自動使用ルール」に従い、該当するMCP/スキルを自動的に使用する**こと。

| 関連技術 | 使用するツール |
|---|---|
| 外部ライブラリの使い方・API仕様 | Context7 MCP (`mcp__context7__resolve-library-id` → `mcp__context7__query-docs`) |
| Unity Editor の操作・確認 | Unity MCP (`mcp__coplaydev-mcp__*`) |

調査結果は、次の計画フェーズで `design.md` に反映する。

---

## ステップ3: 計画フェーズ（ステアリングファイルの作成）

### 3-1. ステアリングディレクトリ作成

```
日付: [YYYYMMDD]
パス: .steering/[YYYYMMDD]-game[ID]-[title]/
```

### 3-2. requirements.md の作成

Issue から取得した仕様を元に、以下を記述:
- ゲーム概要・コアループ
- クリア条件・ゲームオーバー条件
- 必要な GameObject 一覧
- 操作仕様

### 3-3. design.md の作成

調査結果を元に、以下を記述:
- スクリプト構成（どのクラスが何を担当するか）
- 盤面・ステージデータ設計
- 入力処理フロー
- SceneSetup の構成方針

### 3-4. tasklist.md の作成

以下のフェーズに分けたタスクリストを作成:

```markdown
# タスクリスト: Game[ID]_[Title]

## フェーズ1: スプライトアセット生成
- [ ] Python + Pillow でスプライト画像を生成

## フェーズ2: C# スクリプト実装
- [ ] [Title]GameManager.cs
- [ ] [CoreManager].cs（コアメカニクス）
- [ ] [Controller].cs（個別オブジェクト制御）
- [ ] [Title]UI.cs

## フェーズ3: SceneSetup Editor スクリプト
- [ ] Setup[ID]_[Title].cs

## フェーズ4: GameRegistry.json 更新
- [ ] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
```

**このステップが正常に完了したら、決して停止せず、ただちにステップ3.5に進むこと。**

---

## ステップ3.5: 設計書のセルフチェック

サブエージェントを起動せず、以下のチェックリストを自己確認して問題があれば即修正する:

- [ ] `design.md` に `namespace Game[ID]_[Title]` が明記されているか
- [ ] 各クラスの GameManager 参照取得方法（`GetComponentInParent` or `SerializeField`）が明記されているか
- [ ] クリアとゲームオーバー**両方**の状態遷移フローが記述されているか
- [ ] SceneSetup で配線が必要なフィールドが全て `design.md` に列挙されているか
- [ ] `tasklist.md` のタスクが実装可能な粒度に分解されているか

問題があればその場で修正してステップ4へ進む。**サブエージェントは起動しない。**

---

## ステップ4: 実装ループ（tasklist.md の完全消化）

**このステップは、`tasklist.md`の全タスクが `[x]` になるまで自動で繰り返されるループ処理です。**
**このステップが正常に完了したら、決して停止せず、ただちにステップ5に進むこと。**

### Unity ゲーム実装の固有ルール

#### ディレクトリ構成

```
MiniGameCollection/Assets/
├── Scripts/Game[ID]_[Title]/     ← ゲームスクリプト群
├── Editor/SceneSetup/            ← SceneSetup Editorスクリプト
└── Resources/Sprites/Game[ID]_[Title]/  ← スプライトアセット
```

#### C# スクリプト生成ルール

**`[GameTitle]GameManager.cs`**
- namespace: `Game[ID]_[Title]` 形式
- ゲーム状態管理（Playing / Clear / GameOver）
- スコア・クリア条件の管理

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

#### SceneSetup Editor スクリプト

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

#### アセット生成（Pillow方式）

**Python + Pillow で直接スプライトを描画する。**

スプライト出力先: `MiniGameCollection/Assets/Resources/Sprites/Game[ID]_[Title]/`

Pillow がない場合: `pip3 install Pillow` を実行してからリトライ。

### 実装ループ手順

**ループ開始:**

1. タスクリストの読み込み:
   - `[ステアリングディレクトリパス]/tasklist.md` ファイルを読み込む。

2. 進捗の確認:
   - ファイル内に未完了タスク (`[ ]`) が存在するか確認する。
   - **もし未完了タスクが存在しない場合:** この実装ループは完了とみなし、ただちに**ステップ5**へ進む。
   - **もし未完了タスクが存在する場合:** 次の処理（3. タスクの実行）に進む。

3. タスクの実行:
   - `tasklist.md`の**先頭にある未完了タスク**を1つ特定する。
   - そのタスクを完了させるために必要な実装作業を実行する。

4. タスクリストの更新:
   - 実行したタスクが完了したら、`Edit`ツールを使用して`tasklist.md`を更新し、該当タスクを `[ ]` から `[x]` に変更する。

5. ループ継続:
   - **ステップ4の先頭 (1. タスクリストの読み込み) に戻り、処理を繰り返す。**

### 実装ループ内の例外処理ルール

- **ルールA: タスクが大きすぎる場合**
  - **対処法:** 現在のタスクをより小さな複数のサブタスクに分割する。`Edit`ツールを使い、元のタスクを削除し、その場所に新しいサブタスク（`[ ]`付き）を挿入する。その後、ループを継続する。

- **ルールB: 技術的理由でタスクが不要になった場合**
  - **対処法:** `Edit`ツールを使い、該当タスクを `[x] ~~タスク名~~ (理由: [具体的な技術的理由を簡潔に記述])` の形式で更新する。その後、ループを継続する。

- **❌ 絶対禁止の行為:**
  - 未完了タスクを「後でやる」「別タスクにする」などの理由で意図的にスキップすること。
  - 理由なく未完了タスクを放置してループを終了させること。
  - ユーザーに判断を仰ぐこと。

---

## ステップ5: 実装検証

### 5-1. ~~実装品質検証~~ → 5-2 に統合

implementation-validator は省略する。code-reviewer-structural が同等の観点（コーディング規約・配線漏れ・ゲームロジック正確性）をカバーするため重複になる。

### 5-2. 2軸コードレビューとフィードバックループ

以下の2つのサブエージェントを**並列で**起動する:
- `code-reviewer-structural`（構造・アーキテクチャ・配線漏れ・ゲームロジック正確性）
- `code-reviewer-secondary`（欠陥・null安全・エッジケース）

各レビュアーへのプロンプトに必ず明示する:
- **対象**: `Scripts/Game[ID]_[Title]/` と `Editor/SceneSetup/Setup[ID]_[Title].cs` のみ
- **他ゲームのファイルは参照しない**（コンテキスト節約）
- structural には「SceneSetupでの全フィールド配線漏れ確認」も観点に含める

結果を統合し、総合評価（A/B/C/D）を算出する。

**フィードバックの反映:**
1. `[必須]` の指摘があれば即修正する（再レビューは行わない）。
2. 修正の正しさは 5-3 のコンパイル確認と 5-4 の SceneSetup 実行で検証する。

### 5-3. コンパイル検証（Unity MCP）

MCP 接続がある場合:

```
mcp__coplaydev-mcp__refresh_unity(mode="force", scope="all", compile="request", wait_for_ready=true)
mcp__coplaydev-mcp__read_console(types=["error"], count=20)
```

- **エラーがある場合**: スクリプトを修正して再度リフレッシュ。エラーが解消するまで繰り返す。
- **エラーがない場合**: 次のステップへ。

### 5-4. SceneSetup メニューの自動実行

MCP 接続がある場合:

```
mcp__coplaydev-mcp__execute_menu_item(menu_path="Assets/Setup/[ID] [Title]")
mcp__coplaydev-mcp__read_console(types=["error"], count=20)
```

- **エラーがある場合**: SceneSetup スクリプトを修正して再実行。
- **エラーがない場合**: シーン構成完了。

### 5-B. フォールバック（MCP 接続がない場合）

コンパイル検証・SceneSetup 実行をスキップし、ステップ6へ進む。

---

## ステップ6: ブランチ作成 → commit → PR作成 → main マージ

```bash
# 1. main を最新化してからブランチを切る
git checkout main
git pull origin main

# 2. フィーチャーブランチ作成
git checkout -b feature/[YYYYMMDD]-game[ID]-[title-lowercase]

# 3. ステージング & コミット
git add MiniGameCollection/Assets/Scripts/Game[ID]_[Title]/
git add MiniGameCollection/Assets/Editor/SceneSetup/Setup[ID]_[Title].cs
git add MiniGameCollection/Assets/Editor/SceneSetup/Setup[ID]_[Title].cs.meta
git add MiniGameCollection/Assets/Resources/Sprites/Game[ID]_[Title]/
git add MiniGameCollection/Assets/Resources/GameRegistry.json
git add MiniGameCollection/Assets/Scenes/[ID]_[Title].unity
git add MiniGameCollection/Assets/Scenes/[ID]_[Title].unity.meta
git add MiniGameCollection/ProjectSettings/EditorBuildSettings.asset
git add .steering/
git commit -m "feat(game[ID]): implement [Title] game" \
  --trailer "Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"

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

## ステップ7: 振り返りと GitHub Issue 更新

### 7-1. 振り返り記録

`Edit`ツールで `tasklist.md` の「実装後の振り返り」セクションを更新:
- 実装完了日
- 計画と実績の差分
- 学んだこと
- 次回への改善提案

### 7-2. GitHub Issue の更新

```bash
gh issue comment [ISSUE_NUMBER] --body "## 実装完了

- C#スクリプト生成: ✅
- SceneSetup Editorスクリプト: ✅
- アセット生成: ✅
- GameRegistry.json 更新: ✅
- コンパイル検証: ✅
- コードレビュー: ✅

Unity Editor で Assets > Setup > [ID] [Title] を実行してシーンを構成してください。"
```

---

## 完了条件

このワークフローは、以下の全ての条件を満たした時点で自動的に完了となる:

- ステップ2: 既存パターンの調査が完了している。
- ステップ3.5: ステアリングファイルのドキュメントレビューで重要な指摘が解消されている。
- ステップ4: `tasklist.md`の全てのタスクが完了状態（`[x]`または正当な理由でスキップ）になっている。
- ステップ5-1: `implementation-validator`サブエージェントの検証をパスする。
- ステップ5-2: 2軸コードレビューで `[必須]` が0件（総合評価不問）。
- ステップ5-3: コンパイル検証（Unity MCP）がエラーなく成功する（MCP接続時）。
- ステップ6: feature ブランチ作成・commit・push・PR作成・mainマージ完了。
- ステップ7: `tasklist.md`に振り返りが記載され、GitHub Issue にコメント済み。

この完了条件を満たすまで、自律的に思考し、問題解決を行い、作業を継続すること。

---

## 参考: 001_BlockFlow の構成

001_BlockFlow は実装済みなので、以下を参考パターンとして活用すること:
- `MiniGameCollection/Assets/Scripts/Game001_BlockFlow/`
- `MiniGameCollection/Assets/Editor/SceneSetup/Setup001_BlockFlow.cs`
- `MiniGameCollection/Assets/Resources/Sprites/Game001_BlockFlow/`
