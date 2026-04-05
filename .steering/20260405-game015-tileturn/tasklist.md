# タスクリスト: Game015v2_TileTurn

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, TileNormal, TileLinked, TileLocked, TileFlipped, TileCorrect, TileOverlay）

## フェーズ2: C# スクリプト実装
- [x] TileTurnGameManager.cs（StageManager・InstructionPanel統合）
- [x] TileCell.cs（タイルデータ・タイプ判定・コンポーネント）
- [x] TileManager.cs（コアメカニクス・入力処理・5ステージ難易度対応）
- [x] TileTurnUI.cs（ステージ表示・コンボ表示・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup015v2_TileTurn.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
