# タスクリスト: Game035v2_WaveRider

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Surfer, Rock, Whirlpool, Wave, Shield）

## フェーズ2: C# スクリプト実装
- [x] WaveRiderGameManager.cs（StageManager・InstructionPanel統合）
- [x] WaveMechanic.cs（コアメカニクス・3レーン移動・ジャンプ・障害物・5ステージ難易度対応）
- [x] WaveRiderUI.cs（ステージ表示・コンボ表示対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup035v2_WaveRider.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（035 remakeエントリー）

## 実装後の振り返り

- **実装完了日**: 2026-04-06
- **PR**: OumeiSatoKenta/cc_unity_maker#296（マージ済み）
- **計画と実績の差分**:
  - 「バランスゲージ」は実装複雑度を考慮してシンプルな「岩ヒット=ゲームオーバー」に簡略化（設計時に決定）
  - コードレビューで4件の[必須]指摘（SceneLoader.LoadMenu()の誤呼び出し、ReturnToMenuメソッド欠落、_helpButtonワイヤリング欠落、_obstacleCountデクリメント欠落）を即修正
- **学んだこと**:
  - `StopAllCoroutines()` はトリックアニメ等を巻き込む恐れがあるため、専用コルーチン参照（`_slideLaneCo`）で管理する
  - SceneSetup での `AddButtonOnClick` と InstructionPanel の `_helpButton` パターンは、`_helpButton` フィールドのワイヤリングのみで十分（Show()内で登録される）
- **次回への改善提案**:
  - `SceneLoader.LoadMenu()` ではなく `SceneLoader.BackToMenu()` を使うことをdevelopment-guidelines等に明記する
