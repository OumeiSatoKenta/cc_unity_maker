# タスクリスト: ゲーム001 BlockFlow実装

## 実装タスク

- [x] `Scripts/Game001_BlockFlow/BlockFlowGameManager.cs` を作成する（ゲーム全体制御・クリア判定）
- [x] `Scripts/Game001_BlockFlow/BlockController.cs` を作成する（ブロックのスワイプ移動・色管理）
- [x] `Scripts/Game001_BlockFlow/BoardManager.cs` を作成する（盤面生成・ブロック配置・隣接チェック）
- [x] `Scripts/Game001_BlockFlow/BlockFlowUI.cs` を作成する（手数表示・クリアパネル・メニューへ戻る）
- [x] `Editor/SceneSetup/Setup001_BlockFlow.cs` を作成する（シーン自動構成）
- [x] `GameRegistry.json` の001を `implemented: true` に更新する
- [x] Unity MCP で全スクリプトを検証する（コンパイルエラー0・コンソールエラー0）

## レビューセクション

**完了日**: 2026-03-29

**作成物**:
- `BlockFlowGameManager.cs` — ゲーム制御・手数管理・クリア判定
- `BlockController.cs` — ブロックのスワイプ移動・色管理（5色対応）
- `BoardManager.cs` — 盤面生成・ランダム配置・移動処理・BFS隣接グループ判定
- `BlockFlowUI.cs` — 手数表示・クリアパネル・リスタート/メニューボタン
- `Setup001_BlockFlow.cs` — シーン自動構成（ブロックプレハブ・Canvas・UI・EventSystem）
- `GameRegistry.json` — 001を implemented: true に更新
