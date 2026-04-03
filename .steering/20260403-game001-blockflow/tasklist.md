# タスクリスト: Game001_BlockFlow (v2リメイク)

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（block_red/blue/green/yellow, fixed_block, warp_tile, ice_block, board_bg, background）

## フェーズ2: C# スクリプト実装
- [x] BlockFlowGameManager.cs（StageManager・InstructionPanel統合、スコア管理）
- [x] BoardManager.cs（盤面生成・ブロック移動・スワイプ入力・クリア判定・5ステージ対応）
- [x] BlockFlowUI.cs（ステージ表示・スコア・手数・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup001_BlockFlow.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true を維持（既にtrue）

## 実装後の振り返り
（実装完了後に記入）
