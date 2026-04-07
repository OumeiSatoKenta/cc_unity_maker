# タスクリスト: Game036v2_CoinStack

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Coin, HeavyCoin, LightCoin, TargetLine, PerfectEffect）

## フェーズ2: C# スクリプト実装
- [x] CoinStackGameManager.cs（StageManager・InstructionPanel統合、5ステージ・スコア・コンボ管理）
- [x] CoinMechanic.cs（スライド・落下・積み重ね・崩壊判定・ビジュアルフィードバック・5ステージ新要素）
- [x] CoinStackUI.cs（ステージ表示・スコア・コンボ・クリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup036v2_CoinStack.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- **実装完了日**: 2026-04-06
- **PR**: #297（マージ済み）
- **計画と実績の差分**: ほぼ計画通り。コードレビューで2件の[必須]修正（CameraShake null check、remainingCoins負値防止）が発生したが軽微。
- **学んだこと**: DropCoin coroutine内でDeactivate後に処理が継続するバグはよくあるパターン。_isActiveガードをループ内部にも入れることが重要。
- **次回への改善提案**: CollapseアニメーションでRigidbody2D追加後のクリーンアップタイミングも検討する余地あり。
