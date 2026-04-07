# タスクリスト: Game071v2_BeatTiles

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成

## フェーズ2: C# スクリプト実装
- [x] BeatTilesGameManager.cs（StageManager・InstructionPanel統合）
- [x] NoteManager.cs（ノーツ生成・落下・判定・5ステージ対応）
- [x] BeatTilesUI.cs（スコア・コンボ・ライフ・判定テキスト表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup071v2_BeatTiles.cs（全フィールド配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: #330（マージ済み）
- 計画との差分: ホールドノーツの長押し継続判定は未実装（NoteType.Holdを生成するが判定はNormalと同等）。コードレビューで指摘済み。
- 学んだこと: `SetActive(bool active) => _isActive = false;` の引数無視バグは静的解析では見落としやすい。コードレビューが有効。
- 改善提案: AutoMissNotes内でのコレクション変更中RegisterMiss呼び出し問題は、toMissリスト方式で解決した。
