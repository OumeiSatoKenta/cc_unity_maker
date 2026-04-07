# タスクリスト: Game062v2_MagicForest

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（木・苗・背景・動物）

## フェーズ2: C# スクリプト実装
- [x] MagicForestGameManager.cs（StageManager・InstructionPanel統合）
- [x] ForestManager.cs（コアメカニクス・グリッド管理・5ステージ難易度対応）
- [x] MagicForestUI.cs（ステージ表示・魔力・コンボ・クリアパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup062v2_MagicForest.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: #322（マージ済み）
- 計画と実績の差分:
  - `OnWorldTreeCompleted()` メソッドの追加が必要だった（Stage5のゲームクリアフローで `OnStageClear()` を経由してしまうバグを修正）
  - InstructionPanel のフィールド名 `_panel` → `_panelRoot` の修正が必要だった（既存コードと不一致）
  - `WitherRandomTree()` で `_totalTrees` が負になりうるバグを修正
- 学んだこと:
  - Stage5専用のクリア判定（WorldTree完成）は、通常のステージクリア判定をバイパスする必要がある
  - InstructionPanelのフィールド名は実装前に必ず実物を確認する
- 次回への改善提案:
  - ForestManager の `CheckStageClear()` でステージ別分岐が複雑になりがち → ステージ別ストラテジーパターンを検討
