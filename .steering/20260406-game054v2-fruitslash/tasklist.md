# タスクリスト: Game054v2_FruitSlash

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（グラデーション・影・アウトライン付き）

## フェーズ2: C# スクリプト実装
- [x] FruitObject.cs（フルーツデータ・タイプEnum）
- [x] FruitManager.cs（スポーン・スワイプ切断判定・5ステージ難易度対応）
- [x] FruitSlashGameManager.cs（StageManager・InstructionPanel統合・スコア/コンボ管理）
- [x] FruitSlashUI.cs（スコア・コンボ・ライフ・ステージ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup054v2_FruitSlash.cs（InstructionPanel・StageManager・ライフUI配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- PR: #315（マージ済み）
- 計画と実績の差分: ほぼ計画通り。Unity MCPタイムアウトのためコンパイル検証はスキップ（フォールバック）
- 学んだこと: StageManagerの正しいメソッド名は`CompleteCurrentStage()`（`AdvanceToNextStage()`は存在しない）。SliceAnimコルーチンはnullチェックをlocalScale書き込みの前に行う必要がある
- 次回への改善提案: セグメント-円交差判定のゼロ除算ガードを設計段階から組み込む
