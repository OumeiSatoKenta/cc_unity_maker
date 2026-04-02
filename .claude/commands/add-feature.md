---
description: Unityミニゲームを1本、Issue仕様に基づいて完全自動で実装する（v2: 5ステージ・チュートリアル・高品質スプライト対応）
---

# Unityミニゲーム実装 (完全自動実行モード v2)

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

### 2-0. 共通インフラの存在確認（必須）

以下のファイルが存在することを確認する。存在しない場合は **警告を出して停止** する:
- `MiniGameCollection/Assets/Scripts/Common/InstructionPanel.cs`
- `MiniGameCollection/Assets/Scripts/Common/StageManager.cs`
- `MiniGameCollection/Assets/Scripts/Common/FavoriteManager.cs`

### 2-1. 既存コードの調査

直近で実装済みのゲーム（GameRegistry.json で `implemented: true` かつ最も大きい ID）の以下を参照する:
- `MiniGameCollection/Assets/Scripts/Game[直近ID]_[Title]/`
- `MiniGameCollection/Assets/Editor/SceneSetup/Setup[直近ID]_[Title].cs`

> **注意**: Game001_BlockFlow は毎回読まない。直近1本のみ参照すればパターンは十分。

> **カテゴリ不一致時の追加参照**: 直近ゲームの `category` が対象ゲームと異なる場合は、同カテゴリの実装済みゲームからも1本参照し、カテゴリ固有のパターンを把握する。

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

Issue から取得した仕様を元に、以下を **すべて** 記述:
- ゲーム概要・コアループ
- クリア条件・ゲームオーバー条件
- 必要な GameObject 一覧
- 操作仕様
- **チュートリアル表示内容**（以下の4項目を日本語で記述）:
  - タイトル（ゲーム名）
  - 説明（1行でゲームの概要）
  - 操作方法（具体的な操作説明）
  - ゴール（何をすればクリアか）
- **5ステージ難易度設計**:
  - ステージ1〜5で何が変わるか（速度、数量、複雑度、新要素の追加など）
  - 各ステージの具体的なパラメータ値
- **ゲーム深化設計**（以下をすべて記述すること）:
  - **判断ポイント**: プレイヤーが毎秒〜数秒ごとに迫られる判断は何か？（例: 「安全に1個取るか、リスクを冒して3個取るか」）
  - **スキルカーブ**: うまいプレイヤーと下手なプレイヤーの差はどこに出るか？（例: 「先読みの深さ」「タイミング精度」「リソース管理」）
  - **相互作用する2メカニクス**: 以下のゲームデザインパターンから最低1つを選び、具体的にどう適用するか記述:
    - **時間 vs 精度トレードオフ**: 急ぐほど高スコアだがミスしやすい
    - **リソース管理**: 使うと強力だが有限（ボム、シールド、特殊能力等）
    - **連鎖/波及効果**: 1手が複数結果に影響する（マッチ3の連鎖、ドミノ式破壊等）
    - **リスク vs リワード選択**: 安全路線と高リスク高リターンの選択肢がある
    - **マルチタスク**: 複数の要素を同時に管理する（左右独立操作、複数レーン監視等）
    - **パターン認識 + 応用**: 規則性を学習し、変化に対応する
  - **コンボ/乗算スコアシステム**: 連続成功でスコア倍率が上がる仕組み
  - **ステージごとの新ルール追加**: 各ステージで少なくとも1つ新要素が加わる設計（ステージ2で新敵、3で新ギミック、4で環境変化、5で複合チャレンジ等）

### 3-3. design.md の作成

調査結果を元に、以下を **すべて** 記述:
- スクリプト構成（どのクラスが何を担当するか）
- 盤面・ステージデータ設計
- 入力処理フロー
- SceneSetup の構成方針
- **StageManager統合**:
  - `OnStageChanged` 購読でどう盤面を再構築するか
  - ステージ別パラメータ表（Stage 1〜5の速度・数量・複雑度）
- **InstructionPanel内容**:
  - title, description, controls, goal の具体的な文字列
- **ビジュアルフィードバック設計**:
  - 最低2つの演出（例: スケールパルス、色フラッシュ、シェイク、パーティクル風）
  - どのアクション時にどの演出を適用するか
- **スコアシステム**:
  - 基本スコア、コンボルール、乗算条件
- **ステージ別新ルール表**:
  - Stage 1: 基本ルールのみ（チュートリアル的）
  - Stage 2: 新要素1つ追加（具体的に何か）
  - Stage 3: 新要素1つ追加 or 既存要素の複合化
  - Stage 4: 環境変化やプレッシャー要素の追加
  - Stage 5: 全要素の複合チャレンジ（最終試練）
- **判断ポイントの実装設計**:
  - プレイヤーが選択を迫られる瞬間のトリガー条件
  - 各選択の報酬/ペナルティの具体的な数値

### 3-4. tasklist.md の作成

以下のフェーズに分けたタスクリストを作成:

```markdown
# タスクリスト: Game[ID]_[Title]

## フェーズ1: スプライトアセット生成
- [ ] Python + Pillow で高品質スプライト画像を生成（グラデーション・影・アウトライン付き）

## フェーズ2: C# スクリプト実装
- [ ] [Title]GameManager.cs（StageManager・InstructionPanel統合）
- [ ] [CoreManager].cs（コアメカニクス・5ステージ難易度対応）
- [ ] [Title]UI.cs（ステージ表示・コンボ表示対応）

## フェーズ3: SceneSetup Editor スクリプト
- [ ] Setup[ID]_[Title].cs（InstructionPanel・StageManager配線含む）

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
- [ ] **5ステージの具体的パラメータ変化が記述されているか**
- [ ] **InstructionPanelの4テキスト（title, description, controls, goal）が記述されているか**
- [ ] **2つ以上のビジュアルフィードバック演出が記述されているか**
- [ ] **コンボ/スコア乗算ルールが記述されているか**
- [ ] **StageManager の OnStageChanged 購読とステージ再構築ロジックが記述されているか**
- [ ] **判断ポイント**: 「プレイヤーが毎秒〜数秒ごとに迫られる判断」が具体的に記述されているか（「タップする」だけでは不十分。「どこを」「いつ」「何を犠牲にして」が必要）
- [ ] **スキルカーブ**: うまい/下手の差が出るポイントが明記されているか
- [ ] **ゲームデザインパターン**: 6パターン（時間vs精度/リソース管理/連鎖/リスクvsリワード/マルチタスク/パターン認識）から最低1つが選択・適用されているか
- [ ] **ステージ別新ルール**: Stage 2〜5で各ステージに最低1つの新要素が追加される設計か（パラメータ変更だけでは不十分）
- [ ] **同カテゴリ重複チェック**: GameRegistry.json で同 `category` の実装済みゲームとコアメカニクスが重複していないか確認したか
- [ ] **Buggy Code防止**: `Physics2D` タグ・レイヤー比較には `gameObject.name` を使用しているか。複数クラスの `Update()` が同時に走る場合は `_isActive` ガードがあるか。動的生成リソース（Texture2D, Sprite等）は `OnDestroy()` でクリーンアップされるか

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
├── Scripts/Common/               ← 共通コンポーネント（InstructionPanel, StageManager等）
├── Editor/SceneSetup/            ← SceneSetup Editorスクリプト
└── Resources/Sprites/Game[ID]_[Title]/  ← スプライトアセット
```

#### C# スクリプト生成ルール

**`[GameTitle]GameManager.cs`**
- namespace: `Game[ID]_[Title]` 形式
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- スコア・クリア条件の管理
- **必須フィールド:**
  - `[SerializeField] StageManager _stageManager`
  - `[SerializeField] InstructionPanel _instructionPanel`
- **Start() の流れ:**
  1. `_instructionPanel.Show(gameId, title, description, controls, goal)`
  2. `_instructionPanel.OnDismissed += StartGame`
  3. `StartGame()` 内で `_stageManager.StartFromBeginning()`
- **StageManager 統合:**
  - `_stageManager.OnStageChanged += OnStageChanged` でステージ再構築
  - `_stageManager.OnAllStagesCleared += OnAllStagesCleared` で最終クリア
  - `OnStageChanged(int stage)` でコアマネージャーに `GetCurrentStageConfig()` を渡す
- **スコアシステム:**
  - コンボカウンターとスコア乗算を実装

**コアメカニクス Manager**（ゲームによって名前は変わる）
- ゲームのコアルール実装
- **入力処理は必ずこのManagerに一元管理する**（個別オブジェクトで処理しない）
- 新Input System使用: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint`
- `Input.mousePosition` は使わない → `Mouse.current.position.ReadValue()`
- using: `using UnityEngine.InputSystem;`
- **5ステージ対応:**
  - `SetupStage(StageManager.StageConfig config)` メソッドで難易度パラメータを適用
  - `speedMultiplier`, `countMultiplier`, `complexityFactor` を利用
- **ビジュアルフィードバック:**
  - 正解/成功時: `transform.localScale` のポップアニメーション（1.0 → 1.3 → 1.0、0.2秒）
  - 失敗/ミス時: `SpriteRenderer.color` の赤フラッシュ + カメラシェイク
  - コンボ時: スケール + 色変化の複合演出

**`[GameTitle]UI.cs`**
- スコア・手数・クリアパネル表示
- **必須表示要素:**
  - ステージ表示「Stage X / 5」
  - スコア表示（コンボ乗算込み）
  - ステージクリアパネル（「次のステージへ」ボタン）
  - 最終クリアパネル（全5ステージ完了）
  - ゲームオーバーパネル
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
- **StageManager の生成と配線**（GameManagerの子オブジェクトとして）
- **InstructionPanel の生成と配線:**
  - Canvas上にフルスクリーンオーバーレイパネルとして作成
  - タイトル、説明、操作、ゴールの4テキスト + 「はじめる」ボタン
  - 「?」ボタン（右下、再表示用）
  - sortOrder を最前面に設定
- **ステージクリアパネル の生成:**
  - 「ステージクリア！」テキスト + 「次のステージへ」ボタン
- **ボタンの最低タップ領域サイズ**: `sizeDelta` の最小値は `(150, 55)` とする（モバイル操作性確保）

#### アセット生成（高品質Pillow方式）

**Python + Pillow で高品質スプライトを描画する。**

スプライト出力先: `MiniGameCollection/Assets/Resources/Sprites/Game[ID]_[Title]/`

Pillow がない場合: `pip3 install Pillow` を実行してからリトライ。

**品質ルール（必須）:**

1. **カテゴリ別カラーパレット** を使用すること:

| カテゴリ | メインカラー | サブカラー | アクセント |
|---------|------------|----------|----------|
| puzzle | `#2196F3` (青) | `#00BCD4` (ティール) | `#E3F2FD` (淡青) |
| action | `#F44336` (赤) | `#FF9800` (オレンジ) | `#FBE9E7` (淡赤) |
| casual | `#4CAF50` (緑) | `#FFEB3B` (黄) | `#E8F5E9` (淡緑) |
| idle | `#9C27B0` (紫) | `#E91E63` (ピンク) | `#F3E5F5` (淡紫) |
| rhythm | `#00BCD4` (シアン) | `#E040FB` (マゼンタ) | `#E0F7FA` (淡シアン) |
| simulation | `#795548` (茶) | `#FF8F00` (琥珀) | `#EFEBE9` (淡茶) |
| unique | `#76FF03` (ネオン緑) | `#D500F9` (ネオン紫) | `#212121` (ダーク) |

2. **グラデーション塗り**（単色塗りつぶし禁止）:
```python
for y in range(h):
    ratio = y / h
    r = int(top_r + (bottom_r - top_r) * ratio)
    g = int(top_g + (bottom_g - top_g) * ratio)
    b = int(top_b + (bottom_b - top_b) * ratio)
    draw.line([(0, y), (w-1, y)], fill=(r, g, b, 255))
```

3. **2px ドロップシャドウ**:
```python
# 影を先に描画（オフセット+2, +2、暗い色）
shadow_color = (int(r*0.3), int(g*0.3), int(b*0.3), 180)
draw.rounded_rectangle([x+2, y+2, x2+2, y2+2], radius=8, fill=shadow_color)
# 本体を描画
draw.rounded_rectangle([x, y, x2, y2], radius=8, fill=main_color)
```

4. **1px アウトライン**:
```python
draw.rounded_rectangle([x, y, x2, y2], radius=8, fill=main_color, outline=outline_color, width=1)
```

5. **アンチエイリアス**: 2倍サイズで描画 → LANCZOS ダウンサンプル:
```python
img_2x = Image.new("RGBA", (w*2, h*2), (0,0,0,0))
# ... 2倍サイズで描画 ...
img = img_2x.resize((w, h), Image.LANCZOS)
```

6. **最小サイズ**: 128x128（2倍描画なので256x256で描いて縮小）

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
- structural には「SceneSetupでの全フィールド配線漏れ確認」と「StageManager・InstructionPanel配線確認」も観点に含める

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

MCP 接続がある場合、**まず Play Mode を停止してから実行する**:

```
mcp__coplaydev-mcp__manage_editor(action="stop")
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

# 3. コミット前の安全確認（stash pop で意図しないファイルが復元されていないか）
git status --short
# 対象ゲーム以外のファイルが変更されている場合は git checkout -- で戻す

# 4. ステージング & コミット
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
- 5ステージ進行対応
- チュートリアルパネル内蔵

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
- 5ステージ進行: ✅
- チュートリアルパネル: ✅

Unity Editor で Assets > Setup > [ID] [Title] を実行してシーンを構成してください。"
```

---

## 完了条件

このワークフローは、以下の全ての条件を満たした時点で自動的に完了となる:

- ステップ2: 既存パターンの調査が完了している。共通インフラの存在確認済み。
- ステップ3.5: ステアリングファイルのセルフチェックで全項目がパス。
- ステップ4: `tasklist.md`の全てのタスクが完了状態（`[x]`または正当な理由でスキップ）になっている。
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

---

## ステップ8: コンテキストクリア（必須）

全ステップ完了後、以下を順番に実行すること:

1. PRがマージされたことを確認する
2. `/compact` を実行してコンテキストを圧縮する
3. `/compact` が失敗した場合は `"CONTEXT_CLEAR_NEEDED"` と出力する

**重要:** 次のゲームへの連続実行はコンテキストクリア後にのみ許可される。
コンテキストクリアなしでの連続実装は禁止する。
