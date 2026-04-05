# タスクリスト: Game022v2_GravityBall

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Ball, Obstacle, GravityZone, TriggerObstacle）

## フェーズ2: C# スクリプト実装
- [x] GravityBallGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] GravityBallController.cs（重力反転・横スクロール・障害物生成・衝突判定・5ステージ対応）
- [x] GravityBallUI.cs（ステージ・スコア・コンボ表示、各パネル制御）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup022v2_GravityBall.cs（全GameObjects生成・InstructionPanel・StageManager・UI配線）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り
（実装完了後に記入）
