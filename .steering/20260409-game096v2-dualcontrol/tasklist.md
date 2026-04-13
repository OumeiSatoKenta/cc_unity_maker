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

**実装完了日**: 2026-04-09

**計画と実績の差分**:
- 計画通りに3スクリプト＋SceneSetup＋スプライト8枚を実装
- コードレビューで指摘されたコルーチンループ中のnullチェック漏れを修正（StopAllCoroutines追加）
- HandleInputのcam変数スコープエラー（コンパイルエラー）を1回修正が必要だった

**学んだこと**:
- HandleInput内でCamera.mainをローカル変数に取得する場合、使用前に宣言する必要がある（宣言後に使う箇所が先に現れると CS0103エラー）
- SetupStage冒頭でStopAllCoroutines()を呼ぶことで前ステージのコルーチンを一括停止できる

**次回への改善提案**:
- Camera.mainのキャッシュ変数はメソッド先頭に宣言するパターンを設計書に明記する
- コルーチン使用時はStopAllCoroutines()をSetupStage/RestartGame冒頭に追加する原則を設計書に記述する
