# タスクリスト: Game024v2_BubblePop

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, BubbleNormalRed/Blue/Green, BubbleIron, BubbleSplit, BubbleGhost, Heart）

## フェーズ2: C# スクリプト実装
- [x] BubblePopGameManager.cs（StageManager・InstructionPanel統合、ライフ・コンボ・フィーバー管理）
- [x] BubbleController.cs（バブル生成・移動・入力・5ステージ難易度対応）
- [x] BubblePopUI.cs（ステージ表示・スコア・ライフ・コンボ・フィーバー・パネル制御）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup024v2_BubblePop.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（024 remakeエントリー）

## 実装後の振り返り

- 実装完了日: 2026-04-05
- PR: #284 (merged)
- 計画と実績の差分: ほぼ計画通り。レビューで4件の[必須]指摘（タッチ入力未対応・Splitスコア漏れ・Ghostフラグ未更新・else冗長条件）をキャッチして修正。
- 学んだこと: Ghost bubbleのcolorRevealedフラグはセッター処理まで実装する意識が必要。SplitバブルはOnBubblePopped呼び出し忘れに注意。
- 次回への改善提案: TapBubble内でSplit/Normalの処理分岐時に必ずOnBubblePopped呼び出しがあるかチェックするセルフチェック項目に追加する。
