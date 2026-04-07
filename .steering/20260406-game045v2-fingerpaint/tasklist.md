# タスクリスト: Game045v2 FingerPaint

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成

## フェーズ2: C# スクリプト実装
- [x] FingerPaintGameManager.cs（StageManager・InstructionPanel統合）
- [x] FingerPaintCanvas.cs（コアメカニクス・Texture2D描画・5ステージ対応）
- [x] FingerPaintUI.cs（一致率・インク・タイマー・パレット表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup045v2_FingerPaint.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
- 実装完了日: 2026-04-06
- 計画と実績の差分: ほぼ計画通り。コンパイルエラーが3件発生（qBtn宣言順序, GetCurrentConfig→GetCurrentStageConfig, GoNextStage→CompleteCurrentStage）したが順次修正。
- 学んだこと: StageManager/InstructionPanel APIは必ずgrep確認が必要。変数の前方参照（qBtn）はC#ではエラーになるためワイヤリングは生成後に行うこと。
- 次回への改善提案: Setup系でInstructionPanelの_helpButton配線は、qBtnObj生成・qBtn変数宣言の後に行う順序を設計段階から明確化する。
