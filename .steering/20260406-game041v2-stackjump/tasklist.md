# タスクリスト: Game041v2 StackJump

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, block, base, perfect_effect）

## フェーズ2: C# スクリプト実装
- [x] StackJumpGameManager.cs（StageManager・InstructionPanel統合）
- [x] StackJumpMechanic.cs（ブロックスライド・タップ停止・カット・Perfect判定・コンボ・5ステージ対応）
- [x] StackJumpUI.cs（段数/スコア/コンボ/パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup041v2_StackJump.cs（InstructionPanel・StageManager配線含む完全自動シーン構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- **実装完了日**: 2026-04-06
- **計画との差分**: 概ね計画通り。Y軸スライドを「奥行き表現」としてY座標移動で実装（2D表現の制約）
- **学んだこと**: SceneSetupでのボタン重複配線（GameManager直接 + UI経由の二重登録）を構造レビューで早期検出できた。StackJumpのような物理なしのシンプル積み上げは `ClearAllBlocks` でのコルーチン停止が必須
- **次回への改善提案**: CameraShakeをoffsetベース管理に分離するパターンを定型化する
