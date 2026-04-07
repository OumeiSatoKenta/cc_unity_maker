# タスクリスト: Game073v2_MelodyMaze

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（rhythm系カラーパレット: シアン/マゼンタ）

## フェーズ2: C# スクリプト実装
- [x] MelodyMazeGameManager.cs（StageManager・InstructionPanel統合）
- [x] MazeManager.cs（迷路グリッド・スワイプ入力・音符ノード・タイミング判定・5ステージ対応）
- [x] MelodyMazeUI.cs（ステージ・スコア・タイマー・コンボ・判定テキスト表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup073v2_MelodyMaze.cs（InstructionPanel・StageManager・全UI配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- 計画との差分: コードレビューで _spriteTimingRing フィールドが未使用として削除対象に。PlayMelodyPreview の scale 計算バグ（normalized * s * magnitude）を origScale * s に修正。SwapRandomNoteNodes でプレイヤー位置の除外が漏れていた。
- 学んだこと: タイミングタップとスワイプ入力が競合する場合は Update() 内で排他処理（return）が必要。コンテキスト圧縮後の作業継続では broken な dead code が残りやすいので注意。
- 次回への改善提案: Update() の入力処理は最初から明確な優先度分岐で書く。PlayMelodyPreview など coroutine 内でオブジェクトスケールを操作する際は開始前に origScale を保存する習慣をつける。
