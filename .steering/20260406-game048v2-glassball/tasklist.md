# タスクリスト: Game048v2_GlassBall

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, GlassBall, Goal, Nail, Hammer, Coin, ThinIce, WindEffect, CrackEffect）

## フェーズ2: C# スクリプト実装
- [x] GlassBallGameManager.cs（StageManager・InstructionPanel統合）
- [x] RailManager.cs（レール描画・インク管理・EdgeCollider2D）
- [x] GlassBallController.cs（物理転がり・衝撃管理・コイン収集）
- [x] GlassBallUI.cs（衝撃ゲージ・インク残量・コイン・ステージ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup048v2_GlassBall.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
