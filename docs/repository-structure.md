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
│   ├── CollectionSelect.unity          # コレクション選択画面（Classic/Remake/お気に入り）
│   ├── TopMenu.unity                   # ゲーム選択画面（カテゴリタブ）
│   ├── 001_BlockFlow.unity             # classic版（v1）
│   ├── 001v2_BlockFlow.unity           # remake版（v2、5ステージ）
│   ├── 002_MirrorMaze.unity
│   ├── 002v2_MirrorMaze.unity
│   └── ...（ゲーム追加のたびに増える）
│
├── Scripts/
│   ├── Common/                         # 全ゲーム共通スクリプト
│   │   ├── SceneLoader.cs             # シーン遷移管理（CurrentCollectionを保持）
│   │   ├── GameRegistry.cs            # GameRegistry.json読み込み
│   │   └── BackToMenuButton.cs        # 各ゲームから戻るボタン
│   ├── CollectionSelect/               # CollectionSelect専用スクリプト
│   │   └── CollectionSelectManager.cs # コレクション選択画面制御
│   ├── TopMenu/                        # TopMenu専用スクリプト
│   │   ├── TopMenuManager.cs          # カテゴリタブ制御・ゲームカード生成
│   │   └── GameCardUI.cs              # ゲームカードUI部品
│   ├── Game001_BlockFlow/              # classic版（v1）
│   │   ├── BlockFlowGameManager.cs
│   │   ├── BlockController.cs
│   │   └── BlockFlowUI.cs
│   ├── Game001v2_BlockFlow/            # remake版（v2、namespace: Game001v2_BlockFlow）
│   │   ├── BlockFlowGameManager.cs
│   │   └── ...
│   ├── Game002_MirrorMaze/
│   ├── Game002v2_MirrorMaze/
│   └── ...（ゲーム追加のたびに増える）
│
├── Fonts/
│   ├── NotoSansJP-Regular.ttf          # 日本語フォント（Noto Sans JP）
│   └── NotoSansJP-Regular SDF.asset    # TMP用フォントアセット（Generate Japanese Fontで生成）
│
├── Editor/
│   └── SceneSetup/                     # シーン自動構成Editorスクリプト
│       ├── SetupCollectionSelect.cs    # CollectionSelectシーン自動構成
│       ├── SetupTopMenu.cs             # TopMenuシーン自動構成
│       ├── SetupJapaneseFont.cs        # 日本語フォントアセット生成
│       ├── Setup001_BlockFlow.cs       # classic版（メニュー: Assets/Setup/001 BlockFlow）
│       ├── Setup001v2_BlockFlow.cs     # remake版（メニュー: Assets/Setup/001v2 BlockFlow）
│       ├── Setup002_MirrorMaze.cs
│       ├── Setup002v2_MirrorMaze.cs
│       └── ...（ゲーム追加のたびに増える）
│
└── Resources/
    └── GameRegistry.json               # ゲーム一覧データ（collectionフィールドで分類）
```

---

## ディレクトリ詳細

### MiniGameCollection/Assets/Scenes/

**役割**: 全シーンファイルを管理

**配置ファイル**:
- `CollectionSelect.unity`: アプリ起動時の最初の画面（コレクション選択）
- `TopMenu.unity`: コレクション選択後のゲーム選択画面
- `<ID>_<Title>.unity`: 各ゲームのclassic版シーン（v1）
- `<ID>v2_<Title>.unity`: 各ゲームのremake版シーン（v2、5ステージ）

**命名規則**:
- CollectionSelect: `CollectionSelect.unity`（固定）
- TopMenu: `TopMenu.unity`（固定）
- classic版ゲームシーン: `<3桁ID>_<PascalCaseタイトル>.unity`
  - 例: `001_BlockFlow.unity`, `021_BladeDash.unity`
- remake版ゲームシーン: `<3桁ID>v2_<PascalCaseタイトル>.unity`
  - 例: `001v2_BlockFlow.unity`, `021v2_BladeDash.unity`

**依存関係**:
- `Scripts/Common/` のスクリプトを参照
- `Scripts/CollectionSelect/`、`Scripts/TopMenu/`、または `Scripts/Game<ID>_<Title>/`（`Game<ID>v2_<Title>/`）を参照

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

### MiniGameCollection/Assets/Fonts/

**役割**: 日本語フォントファイルとTMP用フォントアセットを管理

**配置ファイル**:
- `NotoSansJP-Regular.ttf`: Noto Sans JP フォント（Google Fonts）
- `NotoSansJP-Regular SDF.asset`: TextMeshPro用フォントアセット

**生成方法**: `Assets > Setup > Generate Japanese Font` で自動生成

---

### MiniGameCollection/Assets/Scripts/Game\<ID\>_\<Title\>/ と Game\<ID\>v2_\<Title\>/

**役割**: 各ゲームのロジックを独立したフォルダで管理（classic版とremake版は別フォルダ）

**配置ファイル**:
- `<Title>GameManager.cs`: ゲーム全体の状態管理（開始・終了・スコア）
- `<CoreMechanic>.cs`: コアメカニクスの実装（ゲームごとに異なる）
- `<Title>UI.cs`: ゲーム内UI制御（スコア表示・クリア画面等）

**命名規則**:
- classic版フォルダ: `Game<3桁ID>_<PascalCaseタイトル>`
  - 例: `Game001_BlockFlow/`, `Game021_BladeDash/`
- remake版フォルダ: `Game<3桁ID>v2_<PascalCaseタイトル>`
  - 例: `Game001v2_BlockFlow/`, `Game021v2_BladeDash/`
- スクリプト: `<PascalCaseタイトル><役割>.cs`
  - 例: `BlockFlowGameManager.cs`, `BlockController.cs`
- 名前空間: classic版は `namespace Game<ID>_<Title>`、remake版は `namespace Game<ID>v2_<Title>` で競合を防ぐ
  - 例: `namespace Game001_BlockFlow`、`namespace Game001v2_BlockFlow`

**依存関係**:
- 依存可能: `Scripts/Common/`、Unity標準API
- 依存禁止: 他のゲームのスクリプト（`Game002_*` 等）、classic版とremake版の相互依存

---

### MiniGameCollection/Assets/Editor/SceneSetup/

**役割**: 各ゲームのシーンを自動構成するEditorスクリプト

**配置ファイル**:
- `Setup<ID>_<Title>.cs`: Unity Editorメニューから実行するセットアップスクリプト

**命名規則**:
- classic版: `Setup<3桁ID>_<PascalCaseタイトル>.cs`
  - 例: `Setup001_BlockFlow.cs`
- remake版: `Setup<3桁ID>v2_<PascalCaseタイトル>.cs`
  - 例: `Setup001v2_BlockFlow.cs`

**メニューパス**:
- classic版: `Assets/Setup/<ID> <タイトル>` でEditorメニューに表示
  - 例: `[MenuItem("Assets/Setup/001 BlockFlow")]`
- remake版: `Assets/Setup/<ID>v2 <タイトル>` でEditorメニューに表示
  - 例: `[MenuItem("Assets/Setup/001v2 BlockFlow")]`

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
| CollectionSelectスクリプト | `Scripts/CollectionSelect/` | PascalCase | `CollectionSelectManager.cs` |
| TopMenuスクリプト | `Scripts/TopMenu/` | PascalCase | `TopMenuManager.cs` |
| ゲームスクリプト（classic） | `Scripts/Game<ID>_<Title>/` | `<Title><役割>.cs` | `BlockFlowGameManager.cs` |
| ゲームスクリプト（remake） | `Scripts/Game<ID>v2_<Title>/` | `<Title><役割>.cs` | `BlockFlowGameManager.cs` |
| Editorスクリプト（classic） | `Editor/SceneSetup/` | `Setup<ID>_<Title>.cs` | `Setup001_BlockFlow.cs` |
| Editorスクリプト（remake） | `Editor/SceneSetup/` | `Setup<ID>v2_<Title>.cs` | `Setup001v2_BlockFlow.cs` |

### 新ゲーム追加時に変更するファイル（remakeモード）

| ファイル | 変更内容 |
|---------|---------|
| `Resources/GameRegistry.json` | remakeエントリの `implemented` を `true` に更新 |
| `Scenes/<ID>v2_<Title>.unity` | 新規作成（空シーン） |
| `Scripts/Game<ID>v2_<Title>/` | フォルダ・スクリプト新規作成 |
| `Editor/SceneSetup/Setup<ID>v2_<Title>.cs` | 新規作成 |

---

## 命名規則まとめ

### Unityシーン名
- classic版: `<3桁ID>_<PascalCaseタイトル>`
  - 例: `001_BlockFlow`, `021_BladeDash`, `071_BeatTiles`
- remake版: `<3桁ID>v2_<PascalCaseタイトル>`
  - 例: `001v2_BlockFlow`, `021v2_BladeDash`, `071v2_BeatTiles`

### C#スクリプト
- フォーマット: PascalCase（Unity標準）
- ゲーム固有: `<Title><役割>.cs`
- 名前空間（classic）: `namespace Game<ID>_<Title>`
- 名前空間（remake）: `namespace Game<ID>v2_<Title>`

### Editorメニューパス
- classic版: `Assets/Setup/<ID> <Title>`（例: `Assets/Setup/001 BlockFlow`）
- remake版: `Assets/Setup/<ID>v2 <Title>`（例: `Assets/Setup/001v2 BlockFlow`）

### GameRegistry.jsonのcollectionフィールド
- `"classic"`: v1オリジナル。001〜101全本が `implemented: true`
- `"remake"`: v2リメイク版。実装済みのみ `implemented: true`（未実装は `false`）

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
