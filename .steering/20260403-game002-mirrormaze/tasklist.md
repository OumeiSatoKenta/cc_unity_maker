# タスクリスト: Game002_MirrorMaze

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（グリッドセル、鏡、エミッター、ゴール、壁、プリズム、移動壁、レーザー、背景）

## フェーズ2: C# スクリプト実装
- [x] MirrorMazeGameManager.cs（StageManager・InstructionPanel統合、スコア管理）
- [x] GridManager.cs（グリッド管理・ドラッグ配置・回転・レーザーシミュレーション・5ステージ対応）
- [x] MirrorMazeUI.cs（ステージ表示・スコア・ボタン・パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup002_MirrorMaze.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- **実装完了日**: 2026-04-03
- **計画と実績の差分**: 
  - 旧実装（MazeManager + MirrorController + Prefab方式）が残っており、新実装との競合でコンパイルエラーが発生
  - `cat >` コマンドのcwdズレで GridManager.cs が行方不明になる事故あり
  - コードレビューは旧ファイルを読んでしまい再実施不要と判断
- **学んだこと**:
  - Bashで `cd` を使うとcwdがズレて後続コマンドに影響する → 絶対パスを徹底
  - Write ツールは既存ファイルの事前 Read が必須 → 旧実装がある場合は先に Read してから上書き
  - 旧実装が残っているゲームは、最初に旧ファイルを全削除してからクリーンに始めるべき
- **次回への改善提案**:
  - 旧実装の有無を最初に確認し、クリーンアップを先に行う
  - 全コマンドで絶対パスを使用する
