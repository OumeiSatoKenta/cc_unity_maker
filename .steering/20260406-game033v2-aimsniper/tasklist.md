# タスクリスト: Game033v2 AimSniper

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Scope, Target, MovingTarget, Obstacle, WindIndicator）

## フェーズ2: C# スクリプト実装
- [x] TargetController.cs（ターゲット移動・ステート・ヒット処理）
- [x] AimSniperMechanic.cs（照準・射撃・入力一元管理・5ステージ対応）
- [x] AimSniperGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] AimSniperUI.cs（HUD・ステージ/スコア/弾数/パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup033v2_AimSniper.cs（InstructionPanel・StageManager配線・UI構築含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
