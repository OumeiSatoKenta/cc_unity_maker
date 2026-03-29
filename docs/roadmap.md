# 実装ロードマップ

## 実行コマンド一覧

```bash
# Phase 1: 基盤構築（この順で実行）
/add-feature GitHubラベルとIssueテンプレートのセットアップ
/add-feature 全100ゲームのIssue一括登録スクリプト
/add-feature Unityプロジェクト初期構築
/add-feature 共通スクリプト実装（SceneLoader・GameRegistry・BackToMenuButton）
/add-feature GameRegistry.json初期データ（全100ゲーム・未実装状態）

# Phase 2: TopMenu実装
/add-feature TopMenuシーン（カテゴリタブ・ゲームカードUI）

# Phase 3: 動作確認用ゲーム実装
/add-feature ゲーム001 BlockFlow実装

# Phase 4: 非エンジニア向けガイド
/add-feature GETTING_STARTED.md（初回セットアップガイド）
```

---

## Phase 1: 基盤構築

### 1-1. GitHubラベルとIssueテンプレートのセットアップ

**目的**: Issue管理の準備

**作成物**:
- GitHubラベル（カテゴリ・工数・ステータス）
- `.github/ISSUE_TEMPLATE/game-spec.md`
- `scripts/setup-labels.sh`

---

### 1-2. 全100ゲームのIssue一括登録スクリプト

**目的**: GitHub Projectsで全100ゲームを管理できる状態にする

**作成物**:
- `scripts/create-all-issues.sh`（gh CLIで100件のIssueを作成）
- GitHub Projectsのテーブルビュー設定手順（`docs/`に追記）

**完了条件**: GitHub Projectsでフィルター・ステータス変更ができる

---

### 1-3. Unityプロジェクト初期構築

**目的**: `unity/` フォルダにUnity 6プロジェクトの骨格を作る

**作成物**:
```
unity/
├── Assets/
│   ├── Scenes/           # 空フォルダ
│   ├── Scripts/
│   │   ├── Common/       # 空フォルダ
│   │   └── TopMenu/      # 空フォルダ
│   ├── Editor/
│   │   └── SceneSetup/   # 空フォルダ
│   └── Resources/        # 空フォルダ
├── Packages/
│   └── manifest.json     # Unity 6標準パッケージ
└── ProjectSettings/
    └── ProjectVersion.txt # Unity 6000.x
```

---

### 1-4. 共通スクリプト実装

**目的**: 全ゲームで使う共通ロジックを実装する

**作成物**:
- `Scripts/Common/SceneLoader.cs`
- `Scripts/Common/GameRegistry.cs`
- `Scripts/Common/BackToMenuButton.cs`

---

### 1-5. GameRegistry.json初期データ

**目的**: TopMenuが全ゲームを認識できる状態にする

**作成物**:
- `Resources/GameRegistry.json`（全100ゲームのエントリ、`implemented: false`）

---

## Phase 2: TopMenu実装

### 2-1. TopMenuシーン

**目的**: ミニゲーム集のハブ画面を実装する

**作成物**:
- `Scripts/TopMenu/TopMenuManager.cs`（カテゴリタブ切り替え・ゲームカード生成）
- `Scripts/TopMenu/GameCardUI.cs`（カード1枚のUI制御）
- `Assets/Scenes/TopMenu.unity`（空シーン）
- `Editor/SceneSetup/SetupTopMenu.cs`（TopMenuシーン自動構成）

**完了条件**: SetupTopMenu を実行 → Play → カテゴリタブが切り替わりゲームカードが表示される

---

## Phase 3: 動作確認用ゲーム実装

### 3-1. ゲーム001 BlockFlow実装

**目的**: エンドツーエンドのワークフローを検証する

**作成物**:
- `Scripts/Game001_BlockFlow/`（GameManager・BlockController・UI）
- `Assets/Scenes/001_BlockFlow.unity`
- `Editor/SceneSetup/Setup001_BlockFlow.cs`
- `GameRegistry.json` に `implemented: true` で更新

**完了条件**:
- TopMenuの「パズル」タブにBlockFlowのカードが表示される
- カードをタップ → ゲーム開始 → プレイ → 「メニューへ戻る」でTopMenuに戻れる

---

## Phase 4: 非エンジニア向けガイド

### 4-1. GETTING_STARTED.md

**目的**: 初めてこのプロジェクトに参加する非エンジニアが2時間以内に1本目を完成できるガイド

**作成物**:
- `docs/GETTING_STARTED.md`（セットアップ〜最初のゲーム完成まで全手順）

---

## 依存関係グラフ

```
1-1 GitHubラベル・テンプレート
  ↓
1-2 Issue一括登録スクリプト
  ↓ （並行可能）
1-3 Unityプロジェクト初期構築
  ↓
1-4 共通スクリプト実装
  ↓
1-5 GameRegistry.json初期データ
  ↓
2-1 TopMenuシーン
  ↓
3-1 ゲーム001 BlockFlow実装
  ↓
4-1 GETTING_STARTED.md
```

※ 1-1と1-2はGitHub側の作業、1-3〜1-5はUnityプロジェクト側の作業のため並行実施可能。

---

## 完了後にできること

| できること | 対応Phase |
|-----------|----------|
| GitHubでゲームの進捗をフィルター・管理 | Phase 1完了後 |
| TopMenuでゲームを選んで遊ぶ（001のみ） | Phase 3完了後 |
| 非エンジニアが自力でゲームを追加できる | Phase 4完了後 |
| 工数Sのゲームを月5本ペースで量産 | Phase 4完了後〜 |
