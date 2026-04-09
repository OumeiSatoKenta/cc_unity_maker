# タスクリスト: Game100v2_DreamRun

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成
  - runner.png（キャラクター）
  - runner_jump.png（ジャンプ中キャラ）
  - obstacle_ground.png（地上障害物）
  - obstacle_air.png（空中浮遊障害物）
  - fragment.png（夢の断片アイテム）
  - background.png（背景）
  - background_layer1.png（視差背景レイヤー1）
  - gravity_zone.png（重力反転ゾーン表示）

## フェーズ2: C# スクリプト実装
- [x] DreamRunGameManager.cs（StageManager・InstructionPanel統合）
- [x] DreamRunManager.cs（コアメカニクス・5ステージ難易度・スクロール・障害物生成）
- [x] DreamRunUI.cs（ステージ表示・スコア・ライフ・コンボ・クリアパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup100v2_DreamRun.cs（全GameObject自動構成・InstructionPanel・StageManager配線）

## 実装後の振り返り
（実装完了後に記入）
