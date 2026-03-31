# タスクリスト: TopMenuシーン（カテゴリタブ・ゲームカードUI）

## 実装タスク

- [x] `Scripts/TopMenu/TopMenuManager.cs` を作成する（カテゴリタブ制御・カード生成）
- [x] `Scripts/TopMenu/GameCardUI.cs` を作成する（カード1枚のUI制御）
- [x] `Editor/SceneSetup/SetupTopMenu.cs` を作成する（TopMenuシーン自動構成Editorスクリプト）
- [x] Unity MCP で全スクリプトを検証する（エラー0・警告0・診断0）

## レビューセクション

**完了日**: 2026-03-29
**実績**: 全タスク完了。Unity MCP で全3スクリプトの検証パス。

**作成物**:
- `TopMenuManager.cs` — カテゴリタブ動的生成・カードリスト管理・タブハイライト
- `GameCardUI.cs` — カード表示・未実装グレーアウト・タップでシーン遷移
- `SetupTopMenu.cs` — Editorスクリプト（Canvas・タブ・スクロール・カードプレハブ・BuildSettings登録を全自動）

**次**: `/ship-pr` → `/add-feature ゲーム001 BlockFlow実装`
