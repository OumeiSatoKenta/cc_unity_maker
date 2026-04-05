# タスクリスト: Game014v2 MagnetPath

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（磁石N極/S極、鉄球、ゴール、壁、背景）

## フェーズ2: C# スクリプト実装
- [x] MagnetPathGameManager.cs（StageManager・InstructionPanel統合、コンボスコア管理）
- [x] MagnetManager.cs（磁力シミュレーション・5ステージ対応・レスポンシブ配置）
- [x] MagnetPathUI.cs（HUD・クリア/ゲームオーバーパネル・コンボ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup014v2_MagnetPath.cs（InstructionPanel・StageManager配線・全UIレイアウト）

## フェーズ4: GameRegistry.json 更新
- [x] collection:remake の implemented を true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-05
- PR: #274 (マージ済み)
- 計画との差分: コードレビューで switch magnet の毎フレーム反転バグ・Camera.main の毎フレーム呼び出し・allDone ロジックの冗長性・ResetStage のnullガード・コルーチンのnullチェック不足が指摘され修正した
- 学んだこと: switch magnet のような「ボールが通過した瞬間に一度だけ作動」する要素には、`switchFired` のような発火済みフラグが必須。設計段階でこの観点を明示すべき
- 次回への改善提案: ステージ4の新要素（スイッチ磁石）のような one-shot トリガーパターンは design.md に「発火済みフラグ必須」として明記する
