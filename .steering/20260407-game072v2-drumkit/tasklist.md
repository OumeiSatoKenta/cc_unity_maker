# タスクリスト: Game072v2 DrumKit

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（ドラムパッド6種・リング・背景・判定サークル）

## フェーズ2: C# スクリプト実装
- [x] DrumKitGameManager.cs（StageManager・InstructionPanel統合）
- [x] DrumPadManager.cs（パッド配置・リング縮小・判定処理・5ステージ難易度）
- [x] DrumKitUI.cs（ステージ/スコア/コンボ/判定テキスト/クリアパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup072v2_DrumKit.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- **実装完了日**: 2026-04-07
- **計画との差分**: 全タスクを計画通り完了。MCP未接続のためコンパイル/SceneSetup/PlayMode検証はスキップ
- **学んだこと**: DrumKitはBeatTilesと同じrhythmカテゴリのため、NoteManagerのパターンをDrumPadManagerに応用できた。ガイドリングの縮小アニメーションは子GameObjectのlocalScaleで実装
- **次回への改善**: リングを各パッドの子オブジェクトとして管理したため、パッドのスケール変化と独立してリングを制御できた（良いパターン）
