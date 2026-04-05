# タスクリスト: Game038v2 FlyBird

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・鳥・パイプ・コイン・地面）

## フェーズ2: C# スクリプト実装
- [x] FlyBirdGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] BirdController.cs（タップ入力・Rigidbody2D物理・ビジュアルフィードバック）
- [x] PipeSpawner.cs（パイプ動的生成・移動・5ステージ難易度対応）
- [x] FlyBirdUI.cs（スコア・ステージ・コンボ・クリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup038v2_FlyBird.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
