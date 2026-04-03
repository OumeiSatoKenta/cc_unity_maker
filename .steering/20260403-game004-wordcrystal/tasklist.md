# タスクリスト: Game004v2_WordCrystal

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, crystal_normal, crystal_hidden, crystal_bonus, crystal_poison, letter_tile）

## フェーズ2: C# スクリプト実装
- [x] CrystalObject.cs（クリスタルオブジェクト、タイプ管理）
- [x] LetterTile.cs（文字タイル）
- [x] WordManager.cs（コアメカニクス・クリスタル配置・単語判定・5ステージ難易度対応）
- [x] WordCrystalGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] WordCrystalUI.cs（タイマー・スコア・スロット・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup004v2_WordCrystal.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリ）

## 実装後の振り返り
（実装完了後に記入）
