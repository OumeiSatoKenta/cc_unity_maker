# タスクリスト: Game017v2_CrystalSort

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Crystal×6色, Rainbow, Frozen, Bottle, BottleCapped, BottleTimer, BottleSelected, BottleComplete）

## フェーズ2: C# スクリプト実装
- [x] CrystalSortGameManager.cs（StageManager・InstructionPanel統合）
- [x] BottleManager.cs（コアメカニクス: 瓶・クリスタル管理、入力処理、移動ロジック、5ステージ難易度対応）
- [x] CrystalSortUI.cs（ステージ表示・スコア・残り手数・コンボ・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup017v2_CrystalSort.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
