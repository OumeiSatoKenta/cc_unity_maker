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

## ロードマップ完了状況

全4フェーズ完了（2026-03-29）:
- Phase 1: 基盤構築 ✅（GitHub Issues 100件・ラベル・Unityプロジェクト・共通スクリプト・GameRegistry.json）
- Phase 2: TopMenu ✅（カテゴリタブ・ゲームカードUI・日本語フォント対応）
- Phase 3: ゲーム001 BlockFlow ✅（スワイプパズル・クリア判定・動作確認済み）
- Phase 4: GETTING_STARTED.md ✅（非エンジニア向け初回セットアップガイド）

## 次のアクション

- 工数Sのゲームを順次実装（`/add-feature ゲームXXX タイトル実装` で追加）
- GitHub Projects のテーブルビュー設定（手動）

## ゲームアイデアソース
- リポジトリ: `https://github.com/OumeiSatoKenta/cc_game_ideas`
- ファイル: `ideas/ideas.md`（100本、カテゴリ7種、工数S/M/L）
