# タスクリスト: Game048v2_GlassBall

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, GlassBall, Goal, Nail, Hammer, Coin, ThinIce, WindEffect, CrackEffect）

## フェーズ2: C# スクリプト実装
- [x] GlassBallGameManager.cs（StageManager・InstructionPanel統合）
- [x] RailManager.cs（レール描画・インク管理・EdgeCollider2D）
- [x] GlassBallController.cs（物理転がり・衝撃管理・コイン収集）
- [x] GlassBallUI.cs（衝撃ゲージ・インク残量・コイン・ステージ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup048v2_GlassBall.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- **実装完了日**: 2026-04-06
- **PR**: #309 (マージ済み)
- **計画との差分**: EdgeCollider2Dによるレール物理は複雑すぎるため、waypoint追従方式（LinearVelocity）に変更。Stage4のハンマー移動ギミックは静的Boxとして簡略化。
- **学んだこと**:
  - RailManagerとGlassBallControllerの相互参照はSerializedFieldで直接配線が安全（FindFirstObjectByType不要）
  - ImpactFlashとDangerFlashは別Coroutineで管理しないと競合する
  - SetupStageとResetBallでは必ずStopAllCoroutinesしてコルーチン参照をnull化すること
- **次回への改善提案**: Unity MCPセッションが落ちている場合はコンパイル検証をスキップして問題ないが、SceneSetup実行結果の確認手段が欲しい
