# タスクリスト: Game067v2_TapDojo

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（武道家・背景・技アイコン等）

## フェーズ2: C# スクリプト実装
- [x] TapDojoGameManager.cs（StageManager・InstructionPanel統合）
- [x] DojoManager.cs（タップ・コンボ・自動修行・技・大会・特訓 コアメカニクス）
- [x] TapDojoUI.cs（MP/段位/コンボ/自動速度/ステージ表示、各パネル制御）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup067v2_TapDojo.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: #328（マージ済み）
- 計画と実績の差分: TrainingTimerPanelをSceneSetupで生成する作業が追加必要だった（設計書に漏れ）
- 学んだこと: アイドル系はステート管理が複雑（自動修行・特訓・大会・師範試験が並走）。コルーチン安全管理（StopAllCoroutines + _isActiveガード）が特に重要
- 次回への改善提案: 設計書のSceneSetup配線リストにTimerPanel等の補助パネルも明示的に列挙する
