# タスクリスト: Game008v2_IcePath

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（player, ice, rock, crack, hole, redirect, friction, visited, background）

## フェーズ2: C# スクリプト実装
- [x] IcePathGameManager.cs（StageManager・InstructionPanel統合、Undo機能）
- [x] IceBoardManager.cs（盤面ロジック・5ステージ対応・スワイプ入力・レスポンシブ配置）
- [x] IcePathUI.cs（ステージ・スコア・手数・残りマス表示・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup008v2_IcePath.cs（全フィールド配線・InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（008 remake エントリ）

## 実装後の振り返り

- 実装完了日: 2026-04-03
- 計画と実績の差分: ほぼ計画通り。レビューで3つの必須修正（ResetBoard固定ステージ、コルーチン中のDestroy参照、Crackセルのtotalカウント調整）を修正した
- 学んだこと: Crackセルが破壊されてHoleになる場合は `_totalIceCells` を調整しないとクリア判定が永遠に成立しないバグが潜む。Slide内でVisitCellを呼ぶ設計では特に注意が必要
- 次回への改善提案: `StageLayouts`（文字列定義）は不要なデッドコードになりやすいため、最初からGridDataのみで管理する
