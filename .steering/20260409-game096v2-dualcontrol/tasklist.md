# タスクリスト: Game096v2 DualControl

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, char_left, char_right, obstacle, goal, switch_on, switch_off, door）

## フェーズ2: C# スクリプト実装
- [x] DualControlGameManager.cs（StageManager・InstructionPanel統合、スコア・シンクロ管理）
- [x] ControlManager.cs（2キャラ操作・障害物生成・衝突判定・スイッチ連動・ビジュアルフィードバック）
- [x] DualControlUI.cs（HUD・パネル表示・ボタンイベント）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup096v2_DualControl.cs（全GameObjectの生成・配線・シーン保存）

## 実装後の振り返り
（実装完了後に記入）
