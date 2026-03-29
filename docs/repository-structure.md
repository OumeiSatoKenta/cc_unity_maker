# リポジトリ構造定義書 (Repository Structure Document)

## プロジェクト全体構造

```
cc_unity_maker/                         # リポジトリルート
├── MiniGameCollection/                 # 単一Unityプロジェクト（全ゲーム共通）
│   ├── Assets/
│   │   ├── Scenes/                     # 全シーン
│   │   ├── Scripts/                    # 全C#スクリプト
│   │   ├── Editor/                     # Editorスクリプト
│   │   └── Resources/                  # 実行時に読み込むデータ
│   ├── Packages/
│   │   └── manifest.json
│   └── ProjectSettings/
│       └── ProjectVersion.txt
├── docs/                               # プロジェクトドキュメント
│   ├── ideas/                          # 壁打ち・アイデアメモ
│   ├── product-requirements.md
│   ├── functional-design.md
│   ├── architecture.md
│   ├── repository-structure.md         # 本ドキュメント
│   ├── development-guidelines.md
│   ├── glossary.md
│   └── GETTING_STARTED.md             # 非エンジニア向け初期セットアップガイド
├── scripts/                            # 自動化スクリプト（gh CLI等）
│   ├── create-all-issues.sh            # 全ゲームのIssue一括作成
│   └── update-registry.sh             # GameRegistry.json更新補助
├── .github/
│   ├── ISSUE_TEMPLATE/
│   │   └── game-spec.md               # Issue作成テンプレート
│   └── .gitignore-unity               # Unity用gitignore参考
├── .claude/
│   ├── skills/                         # Claude Codeスキル定義
│   └── agents/                         # サブエージェント定義
├── .steering/                          # 作業単位のドキュメント（作業ごとに作成）
├── knowledge/                          # セッション横断ナレッジ
├── CLAUDE.md                           # Claude Code設定・ワークフロー定義
└── .gitignore
```

---

## Unityプロジェクト詳細構造

```
MiniGameCollection/Assets/
├── Scenes/
│   ├── TopMenu.unity                   # ゲーム選択画面（カテゴリタブ）
│   ├── 001_BlockFlow.unity
│   ├── 002_MirrorMaze.unity
│   └── ...（ゲーム追加のたびに増える）
│
├── Scripts/
│   ├── Common/                         # 全ゲーム共通スクリプト
│   │   ├── SceneLoader.cs             # シーン遷移管理
│   │   ├── GameRegistry.cs            # GameRegistry.json読み込み
│   │   └── BackToMenuButton.cs        # 各ゲームから戻るボタン
│   ├── TopMenu/                        # TopMenu専用スクリプト
│   │   ├── TopMenuManager.cs          # カテゴリタブ制御・ゲームカード生成
│   │   └── GameCardUI.cs              # ゲームカードUI部品
│   ├── Game001_BlockFlow/              # ゲームごとに独立したフォルダ
│   │   ├── BlockFlowGameManager.cs
│   │   ├── BlockController.cs
│   │   └── BlockFlowUI.cs
│   ├── Game002_MirrorMaze/
│   │   └── ...
│   └── ...（ゲーム追加のたびに増える）
│
├── Editor/
│   └── SceneSetup/                     # シーン自動構成Editorスクリプト
│       ├── Setup001_BlockFlow.cs       # ゲームごとに1ファイル
│       ├── Setup002_MirrorMaze.cs
│       └── ...（ゲーム追加のたびに増える）
│
└── Resources/
    └── GameRegistry.json               # ゲーム一覧データ（TopMenuが参照）
```

---

## ディレクトリ詳細

### MiniGameCollection/Assets/Scenes/

**役割**: 全シーンファイルを管理

**配置ファイル**:
- `TopMenu.unity`: アプリ起動時の画面、全ゲームへのエントリポイント
- `<ID>_<Title>.unity`: 各ゲームのシーン（ゲーム追加のたびに追加）

**命名規則**:
- TopMenu: `TopMenu.unity`（固定）
- ゲームシーン: `<3桁ID>_<PascalCaseタイトル>.unity`
  - 例: `001_BlockFlow.unity`, `021_BladeDash.unity`

**依存関係**:
- `Scripts/Common/` のスクリプトを参照
- `Scripts/TopMenu/` または `Scripts/Game<ID>_<Title>/` を参照

---

### MiniGameCollection/Assets/Scripts/Common/

**役割**: 全ゲームから利用する共通スクリプト

**配置ファイル**:
- `SceneLoader.cs`: `LoadGame(sceneName)` / `BackToMenu()` を提供
- `GameRegistry.cs`: `Resources/GameRegistry.json` を読み込み、ゲーム一覧を提供
- `BackToMenuButton.cs`: 全ゲームシーンに配置するメニュー戻るボタン

**依存関係**:
- 依存可能: Unityの標準API（`UnityEngine`, `UnityEngine.SceneManagement`）
- 依存禁止: 各ゲーム固有のスクリプト（`Game001_*` 等）

---

### MiniGameCollection/Assets/Scripts/Game\<ID\>_\<Title\>/

**役割**: 各ゲームのロジックを独立したフォルダで管理

**配置ファイル**:
- `<Title>GameManager.cs`: ゲーム全体の状態管理（開始・終了・スコア）
- `<CoreMechanic>.cs`: コアメカニクスの実装（ゲームごとに異なる）
- `<Title>UI.cs`: ゲーム内UI制御（スコア表示・クリア画面等）

**命名規則**:
- フォルダ: `Game<3桁ID>_<PascalCaseタイトル>`
  - 例: `Game001_BlockFlow/`, `Game021_BladeDash/`
- スクリプト: `<PascalCaseタイトル><役割>.cs`
  - 例: `BlockFlowGameManager.cs`, `BlockController.cs`
- 名前空間: `namespace Game<ID>_<Title>` で競合を防ぐ
  - 例: `namespace Game001_BlockFlow`

**依存関係**:
- 依存可能: `Scripts/Common/`、Unity標準API
- 依存禁止: 他のゲームのスクリプト（`Game002_*` 等）

---

### MiniGameCollection/Assets/Editor/SceneSetup/

**役割**: 各ゲームのシーンを自動構成するEditorスクリプト

**配置ファイル**:
- `Setup<ID>_<Title>.cs`: Unity Editorメニューから実行するセットアップスクリプト

**命名規則**:
- `Setup<3桁ID>_<PascalCaseタイトル>.cs`
  - 例: `Setup001_BlockFlow.cs`

**メニューパス**:
- `Assets/Setup/<タイトル>` でEditorメニューに表示
  - 例: `[MenuItem("Assets/Setup/001 BlockFlow")]`

**依存関係**:
- 依存可能: `UnityEditor`, `UnityEngine`、対応ゲームのスクリプト
- 禁止: ゲーム実行時（`[InitializeOnLoad]`等の乱用は避ける）

---

### MiniGameCollection/Assets/Resources/

**役割**: 実行時に `Resources.Load()` で読み込むデータ

**配置ファイル**:
- `GameRegistry.json`: TopMenuが参照するゲーム一覧データ

**更新タイミング**: 新ゲーム追加のたびにClaude Codeが自動更新

---

### docs/

**役割**: プロジェクト全体のドキュメント管理

| ファイル | 内容 |
|---------|------|
| `product-requirements.md` | プロダクト要求定義書 |
| `functional-design.md` | 機能設計書 |
| `architecture.md` | アーキテクチャ設計書 |
| `repository-structure.md` | リポジトリ構造定義書（本ドキュメント） |
| `development-guidelines.md` | 開発ガイドライン |
| `glossary.md` | 用語集 |
| `GETTING_STARTED.md` | 非エンジニア向け初期セットアップガイド |
| `ideas/` | 壁打ち・調査メモ（自由形式） |

---

### scripts/

**役割**: 繰り返し実行する自動化スクリプト（gh CLI等）

| ファイル | 内容 |
|---------|------|
| `create-all-issues.sh` | 全100ゲームのGitHub Issueを一括作成 |
| `update-registry.sh` | `GameRegistry.json` の更新補助 |

---

### .github/ISSUE_TEMPLATE/

**役割**: GitHub Issueの統一テンプレート

| ファイル | 内容 |
|---------|------|
| `game-spec.md` | ゲーム実装仕様書テンプレート（Claude Codeが使用） |

---

## ファイル配置規則

### Unityスクリプト

| ファイル種別 | 配置先 | 命名規則 | 例 |
|------------|--------|---------|-----|
| 共通スクリプト | `Scripts/Common/` | PascalCase | `SceneLoader.cs` |
| TopMenuスクリプト | `Scripts/TopMenu/` | PascalCase | `TopMenuManager.cs` |
| ゲームスクリプト | `Scripts/Game<ID>_<Title>/` | `<Title><役割>.cs` | `BlockFlowGameManager.cs` |
| Editorスクリプト | `Editor/SceneSetup/` | `Setup<ID>_<Title>.cs` | `Setup001_BlockFlow.cs` |

### 新ゲーム追加時に変更するファイル

| ファイル | 変更内容 |
|---------|---------|
| `Resources/GameRegistry.json` | 新エントリを追加（`implemented: true`） |
| `Scenes/<ID>_<Title>.unity` | 新規作成（空シーン） |
| `Scripts/Game<ID>_<Title>/` | フォルダ・スクリプト新規作成 |
| `Editor/SceneSetup/Setup<ID>_<Title>.cs` | 新規作成 |

---

## 命名規則まとめ

### Unityシーン名
- フォーマット: `<3桁ID>_<PascalCaseタイトル>`
- 例: `001_BlockFlow`, `021_BladeDash`, `071_BeatTiles`

### C#スクリプト
- フォーマット: PascalCase（Unity標準）
- ゲーム固有: `<Title><役割>.cs`
- 名前空間: `namespace Game<ID>_<Title>`

### フォルダ名（Unity外）
- kebab-case（例: `create-all-issues.sh`）

---

## 依存関係ルール

```
TopMenu
  ↓ 参照OK
Common（SceneLoader / GameRegistry）
  ↑ 参照OK
Game<ID>_<Title>（各ゲーム）
```

**禁止される依存**:
- `Common` → 各ゲームスクリプト（❌）
- `Game001_*` → `Game002_*`（❌ ゲーム間の直接依存）
- `Editor/` スクリプト → 実行時スクリプト（❌ `using UnityEditor` は Editor のみ）

---

## .gitignore 設定

```gitignore
# Unity生成ファイル
MiniGameCollection/Library/
MiniGameCollection/Temp/
MiniGameCollection/Obj/
MiniGameCollection/Build/
MiniGameCollection/Builds/
MiniGameCollection/UserSettings/
MiniGameCollection/*.pidb
MiniGameCollection/*.booproj
MiniGameCollection/*.suo
MiniGameCollection/*.user
MiniGameCollection/*.userprefs
MiniGameCollection/*.unityproj
MiniGameCollection/*.sln
MiniGameCollection/*.csproj

# OS
.DS_Store
Thumbs.db

# Claude Code作業ファイル
.steering/
```

---

## スケーリング戦略

### ゲーム数が増えた場合

- `Scenes/` のシーン数が増えても構造は変わらない
- `GameRegistry.json` はClaude Codeが自動管理するため手動更新不要
- `Scripts/Game<ID>_<Title>/` フォルダが増えるだけで既存コードへの影響ゼロ

### 共通機能が必要になった場合

- `Scripts/Common/` に追加
- 全ゲームから参照可能

### ゲームが複数シーンを必要とする場合（工数M/L）

```
Scenes/
├── 091_TimeBlender_Title.unity    # タイトル画面
├── 091_TimeBlender_Game.unity     # ゲーム本編
└── 091_TimeBlender_Result.unity   # リザルト画面
```

シーン名にサフィックス（`_Title` / `_Game` / `_Result`）を付与して識別。
TopMenuからは `_Game` シーンへ直接遷移する。
