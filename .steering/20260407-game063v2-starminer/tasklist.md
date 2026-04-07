# タスクリスト: Game063v2_StarMiner

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（Background, Star1-5, DrillIcon, DroneIcon, OreIcon）

## フェーズ2: C# スクリプト実装
- [x] StarMinerGameManager.cs（StageManager・InstructionPanel統合）
- [x] MiningManager.cs（コアメカニクス・5ステージ・コンボ・ドローン・嵐）
- [x] StarMinerUI.cs（ステージ・スコア・コンボ・アップグレードUI）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup063v2_StarMiner.cs（全配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

**実装完了日:** 2026-04-07

**計画と実績の差分:**
- StageManager API名が想定と異なった（`GetCurrentConfig()` → `GetCurrentStageConfig()`、`AdvanceStage()` → `CompleteCurrentStage()`）
- `SetField(sm, "_stages", stages)` が機能せず、`sm.SetConfigs(stages)` の公開メソッドを使う必要があった
- `countMultiplier` が `int` 型（`float` ではない）のため Setup スクリプトを修正
- `droneOre / 2f` の型変換エラー（CS0266）を明示的キャストで解消
- PlayMode スクリーンショットは MCP 制限で Canvas UI 未表示（ゲーム本体に問題なし）

**学んだこと:**
- v2 実装では必ず `StageManager.cs` を読んで公開APIを確認すること
- `StageConfig.countMultiplier` は `int` 型であることを設計書に明記すべき
- `OnStageChanged` は 0-based index を渡すため、switch 分岐では `stageIndex + 1` に変換が必要

**次回への改善提案:**
- design.md に StageManager の公開メソッド一覧を記載するテンプレートを追加
- countMultiplier の型を requirements.md に明示する慣例を作る
