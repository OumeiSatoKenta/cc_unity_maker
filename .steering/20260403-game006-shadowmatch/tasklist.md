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

**実装完了日**: 2026-04-03

**計画と実績の差分**:
- ShadowRenderer.csは当初予定していたが、Texture2D動的生成は不要と判断。ShadowObjectControllerに統合した。これにより実装がシンプルになった。
- コードレビューで`_lockZ`が実際はY軸ロックとして機能していた命名バグを発見し修正（`_lockYRot`に改名）。
- CalculateMatch()のY軸ラップアラウンド計算にバグがあり修正（NormalizeAngle追加）。

**学んだこと**:
- 3D影のシミュレーションを2D Unityで実現する際は、角度差分からガウス関数でマッチ率計算するアプローチが有効。
- StageConfigsの変数名は意図を明確に（lockX/lockYRotなど）。
- Coroutineの多重起動防止フラグを実装することで、ボタン連打による意図しない動作を防げる。

**次回への改善提案**:
- StageConfigs数とStageManager.TotalStagesのアサーション追加を標準パターンにする。
- ヒントUI更新はOnHintCountChangedイベント経由が疎結合で良い設計。
