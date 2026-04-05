# タスクリスト: Game028v2_RopeSwing

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Player, Platform, GoalPlatform, Anchor, Rope）

## フェーズ2: C# スクリプト実装
- [x] RopeSwingGameManager.cs（StageManager・InstructionPanel統合）
- [x] RopeController.cs（振り子物理・飛行・着地判定・5ステージ対応）
- [x] PlatformManager.cs（足場生成・移動・崩壊・ロープアンカー管理）
- [x] RopeSwingUI.cs（ステージ・スコア・コンボ・各種パネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup028v2_RopeSwing.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
