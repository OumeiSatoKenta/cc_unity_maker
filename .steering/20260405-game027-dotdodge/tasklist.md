# タスクリスト: Game027v2_DotDodge

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Player, DotNormal, DotChaser, DotExpander, SafeZone）

## フェーズ2: C# スクリプト実装
- [x] DotDodgeGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] PlayerController.cs（ドラッグ追従、ニアミス検出、境界クランプ）
- [x] DotSpawner.cs（ドット生成・管理・5ステージ難易度対応）
- [x] DotDodgeUI.cs（HUD・各種パネル表示制御）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup027v2_DotDodge.cs（全オブジェクト配線・InstructionPanel・StageManager含む）

## フェーズ4: GameRegistry.json 更新
- [x] 027 remakeエントリーの implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
