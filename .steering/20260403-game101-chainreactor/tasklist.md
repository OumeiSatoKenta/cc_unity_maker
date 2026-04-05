# タスクリスト: Game101_ChainReactor

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（グラデーション・影・アウトライン付き）

## フェーズ2: C# スクリプト実装
- [x] ChainReactorGameManager.cs（StageManager・InstructionPanel統合）
- [x] ReactorManager.cs（コアメカニクス・5ステージ難易度対応・連鎖処理）
- [x] ChainReactorUI.cs（ステージ表示・コンボ表示・タイマー対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup101_ChainReactor.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
- 実装完了日: 2026-04-03
- add-feature v2 初のテスト実装として成功
- v2の新要素（StageManager, InstructionPanel, 高品質スプライト）が正常にコンパイル・配線された
- 5ステージ × 新ルール追加の設計が計画段階で具体化できた
- コード量はv1の約2倍（6566行 vs ~3000行）だが、ゲーム性は大幅に深化
- 連鎖処理のコルーチン設計が複雑 → HasPendingExplosions()で同期制御
