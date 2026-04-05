# タスクリスト: Game011v2_FoldPaper

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, PaperCell, FoldLine, TargetCell, Shadow）

## フェーズ2: C# スクリプト実装
- [x] FoldPaperGameManager.cs（StageManager・InstructionPanel統合・スコア計算）
- [x] FoldPaperManager.cs（折り紙コアメカニクス・5ステージ難易度・レスポンシブ配置）
- [x] FoldPaperUI.cs（ステージ/スコア/手数/Undo/コンボ表示・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup011v2_FoldPaper.cs（全フィールド配線・InstructionPanel・StageManager）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

### 実装完了日
2026-04-04

### 計画と実績の差分
- 計画通り5ステージ・StageManager・InstructionPanel統合を実装完了
- レビューで `ResetStage()` に `_isActive = true` が漏れていた（コードレビューで検出・修正済み）
- `Mouse.current` null チェック・コルーチン内nullガードもレビューで追加

### 学んだこと
- `HandleMovesExhausted()` で `_isActive = false` にした後、`ResetStage()` でリセット時に必ず `_isActive = true` を復元する必要がある（忘れやすいパターン）
- カメラシェイクコルーチン内で `Camera.main` をキャッシュしないとシーン遷移時にNullRefが発生する

### 次回への改善提案
- 折り紙メカニクスのような複雑なグリッド変換ロジックはユニットテストで事前検証が望ましい
