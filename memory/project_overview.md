---
name: cc_unity_maker プロジェクト概要
description: プロジェクトの目的・設計方針・実装ロードマップの現在地
type: project
---

## 目的
社内の非エンジニアが Claude Code に話しかけるだけで、cc_game_ideas の 100 本のゲームアイデアを Unity ゲームとして量産するワークフローシステム。

**Why:** コードが書けない社員でもゲームを作れる体験を社内に広める。

**How to apply:** 実装の判断はすべて「非エンジニアが迷わず使えるか」を基準にする。

## 設計の核心
- Unity は 1 プロジェクト（`MiniGameCollection/`）のみ。新ゲーム = シーン追加
- TopMenu にカテゴリ別タブ（ミニゲーム集スタイル）
- Claude Code が仕様展開 → C# 生成 → SceneSetup → push まで全自動
- ステータス管理は GitHub Projects（テーブルビュー）

## 完成済みドキュメント（`docs/`）
- `product-requirements.md` ✅
- `functional-design.md` ✅
- `architecture.md` ✅
- `repository-structure.md` ✅
- `development-guidelines.md` ✅
- `glossary.md` ✅
- `roadmap.md` ✅（実装順序の定義）

## 次のアクション（`docs/roadmap.md` 参照）

### Phase 1: 基盤構築
```
/add-feature GitHubラベルとIssueテンプレートのセットアップ
/add-feature 全100ゲームのIssue一括登録スクリプト
/add-feature Unityプロジェクト初期構築
/add-feature 共通スクリプト実装（SceneLoader・GameRegistry・BackToMenuButton）
/add-feature GameRegistry.json初期データ（全100ゲーム・未実装状態）
```

### Phase 2: TopMenu
```
/add-feature TopMenuシーン（カテゴリタブ・ゲームカードUI）
```

### Phase 3: 動作確認用ゲーム
```
/add-feature ゲーム001 BlockFlow実装
```

### Phase 4: ガイド
```
/add-feature GETTING_STARTED.md（初回セットアップガイド）
```

## ゲームアイデアソース
- リポジトリ: `https://github.com/OumeiSatoKenta/cc_game_ideas`
- ファイル: `ideas/ideas.md`（100本、カテゴリ7種、工数S/M/L）
