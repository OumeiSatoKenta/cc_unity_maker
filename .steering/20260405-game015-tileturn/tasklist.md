# タスクリスト: Game015v2_TileTurn

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, TileNormal, TileLinked, TileLocked, TileFlipped, TileCorrect, TileOverlay）

## フェーズ2: C# スクリプト実装
- [x] TileTurnGameManager.cs（StageManager・InstructionPanel統合）
- [x] TileCell.cs（タイルデータ・タイプ判定・コンポーネント）
- [x] TileManager.cs（コアメカニクス・入力処理・5ステージ難易度対応）
- [x] TileTurnUI.cs（ステージ表示・コンボ表示・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup015v2_TileTurn.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-05
- PR: #275（マージ済み）
- 計画との差分: TileManager で TileTurnUI への参照が GetComponent ではなく SerializeField が必要だったことを2軸レビューで発見・修正。Flipped タイルの初期状態バグ（初期から正解判定される）も修正。
- 学んだこと: TileType.Flipped の正解条件は `!IsFlipped && CurrentRotation==0` の複合条件。Locked タイルは initRot=0 で開始しないと正解数に即カウントされる。
- 次回への改善提案: Flipped タイルの「初期フリップ状態」を要件段階で明示的に設計書に書くとレビューが楽になる。
