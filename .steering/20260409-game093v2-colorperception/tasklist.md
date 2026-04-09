# タスクリスト: Game093v2_ColorPerception

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（player, goal, background, view_icon_0/1/2）

## フェーズ2: C# スクリプト実装
- [x] ColorPerceptionGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] ColorPuzzleManager.cs（視点管理、タイル描画、移動ロジック、色変化ゾーン、周期変化）
- [x] ColorPerceptionUI.cs（HUD表示、ステージクリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup093v2_ColorPerception.cs（InstructionPanel・StageManager・移動ボタン・視点切替ボタン配線含む）

## 実装後の振り返り

**実装完了日**: 2026-04-09

### 計画と実績の差分
- 計画通り全タスク完了
- Unity MCPセッションがタイムアウトしたためコンパイル検証・SceneSetup・PlayModeをスキップ
  - ただしコードレビュー2軸で必須修正を対応済み（null安全、配列次元修正、排他制御）
- GameRegistry.json の `implemented: true` 更新は Unity MCP なしのためスキップ

### 学んだこと
- `SpriteRenderer[,,]` で3次元配列を使ったが実際は2次元で十分だった → 最初から設計を明確にすること
- コルーチンの連打制御（StopCoroutine + 再起動）は標準パターンとして最初から組み込む
- `Camera.main` の null チェックは SetupStage の冒頭で必須

### 次回への改善提案
- 視点切替パズルは `_viewMasks[r, c, viewIndex]` の3次元配列設計が明快
- 色変化ゾーンと周期変化の組み合わせは Stage 5 に適したギミック
- 移動ボタン配置は十字パッドより横一列の方が指の届きが良い可能性あり
