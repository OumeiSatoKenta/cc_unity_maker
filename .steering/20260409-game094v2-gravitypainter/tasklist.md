# タスクリスト: Game094v2_GravityPainter

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, cell_empty, cell_wall, cell_absorb, paint_drop, target_overlay, color_palette）

## フェーズ2: C# スクリプト実装
- [x] GravityPainterGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] PaintManager.cs（グリッド管理・重力ロジック・投下・一致率計算・5ステージ難易度対応）
- [x] GravityPainterUI.cs（HUD更新・パネル表示・カラーパレット・重力ボタン）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup094v2_GravityPainter.cs（全配線・InstructionPanel・StageManager・重力ボタン・パレット含む）

## 実装後の振り返り

- 実装完了日: 2026-04-09
- スプライト・スクリプトは前セッションで実装済み。今回はSceneSetupスクリプト作成とバグ修正が主作業
- コードレビューで発見された重要バグ: `UpdateMatchRate` が `ApplyGravity` から呼ばれる際に `_remainingPaint <= 0` でゲームオーバーになる問題 → `checkGameOver` フラグで修正
- コルーチン安全性（PulseCell・FlashMovedCells）・Camera.main null チェック・配列境界チェックも追加
- `StopAllCoroutines()` をSetupStage冒頭に追加してステージ切替時の安全性を確保
- コンパイル成功・SceneSetup実行成功・PlayModeランタイムエラー0件
- PR: https://github.com/OumeiSatoKenta/cc_unity_maker/pull/354
