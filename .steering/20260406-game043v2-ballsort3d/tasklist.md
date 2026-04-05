# タスクリスト: Game043v2_BallSort3D

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（background, tube, ball_r/g/b/y/m, lid, lock_icon）

## フェーズ2: C# スクリプト実装
- [x] BallSort3DGameManager.cs（StageManager・InstructionPanel統合）
- [x] BallSort3DMechanic.cs（コアメカニクス・5ステージ難易度・Undo・デッドロック検出）
- [x] BallSort3DUI.cs（ステージ・スコア・手数・コンボ・タイマー表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup043v2_BallSort3D.cs（InstructionPanel・StageManager・全UI配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
