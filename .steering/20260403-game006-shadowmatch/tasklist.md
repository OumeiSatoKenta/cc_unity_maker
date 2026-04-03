# タスクリスト: Game006v2 ShadowMatch

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・オブジェクト・影・ターゲットシルエット等）

## フェーズ2: C# スクリプト実装
- [x] ShadowMatchGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] ShadowObjectController.cs（ドラッグ入力・影計算・ステージ設定）
- [x] ~~ShadowRenderer.cs~~ (理由: 動的Texture2D生成は不要。SpriteRendererを直接操作する方式を採用しShadowObjectControllerに統合)
- [x] ShadowMatchUI.cs（ステージ表示・スコア・一致度・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup006v2_ShadowMatch.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（006 remake エントリ）

## 実装後の振り返り
（実装完了後に記入）
