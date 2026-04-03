# タスクリスト: Game007v2 NumberFlow

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, CellNormal, CellWall, CellWarpA, CellWarpB, CellDirection, NumberBg）

## フェーズ2: C# スクリプト実装
- [x] NumberFlowGameManager.cs（StageManager・InstructionPanel統合、コンボ・スコア管理）
- [x] NumberFlowManager.cs（グリッド生成・経路管理・特殊マス・5ステージ難易度対応）
- [x] NumberFlowUI.cs（ステージ・スコア・タイマー・進捗・クリアパネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup007v2_NumberFlow.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り
（実装完了後に記入）
