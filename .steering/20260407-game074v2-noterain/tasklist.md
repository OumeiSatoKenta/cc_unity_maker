# タスクリスト: Game074v2_NoteRain

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（note_normal, note_fake, note_accelerating, note_curve, catcher, judgeline, background）

## フェーズ2: C# スクリプト実装
- [x] Note.cs（音符オブジェクト: 落下・カーブ・加速・Fake対応）
- [x] NoteController.cs（音符生成・受け皿操作・キャッチ判定・5ステージ難易度対応）
- [x] NoteRainGameManager.cs（StageManager・InstructionPanel統合）
- [x] NoteRainUI.cs（スコア・コンボ・ライフ・判定テキスト・ステージ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup074v2_NoteRain.cs（InstructionPanel・StageManager配線含む、全UI構築）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remake エントリー）

## 実装後の振り返り
（実装完了後に記入）
