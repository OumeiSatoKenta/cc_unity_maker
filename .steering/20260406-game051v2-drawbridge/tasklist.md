# タスクリスト: Game051v2_DrawBridge

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, LeftCliff, RightCliff, Ball, Goal, Rock, Wind, Coin）

## フェーズ2: C# スクリプト実装
- [x] DrawBridgeGameManager.cs（StageManager・InstructionPanel統合、5ステージ設定）
- [x] DrawingManager.cs（線描画・インク管理・物理EdgeCollider2D生成・入力処理）
- [x] BallController.cs（Rigidbody2D制御・GOボタン起動・ゴール/落下判定）
- [x] DrawBridgeUI.cs（インクゲージ・スコア・ステージ・各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup051v2_DrawBridge.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
