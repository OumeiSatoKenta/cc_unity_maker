# タスクリスト: Game046v2_SqueezePop

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Balloon, BombBalloon, ShieldBalloon, PerfectEffect, BalloonPopped）

## フェーズ2: C# スクリプト実装
- [x] SqueezePopGameManager.cs（StageManager・InstructionPanel統合）
- [x] BalloonManager.cs（コアメカニクス・入力処理・5ステージ難易度対応）
- [x] SqueezePopUI.cs（HUD・StageClear・GameOver・AllClearパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup046v2_SqueezePop.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- PR: #307 (マージ済み)
- 主な差分: SceneLoader が static class であることを事前に確認せず `FindFirstObjectByType<SceneLoader>()` を記述してしまいコンパイルエラー。直接 `SceneManager.LoadScene("TopMenu")` で対処。
- 学んだこと: SceneLoader の static 化は Common インフラの重要な前提。Setup スクリプト生成時に必ず確認する。
- 次回改善: SceneSetup テンプレートの AddBackToMenuListener を最初から SceneManager 直呼び出しにする。
