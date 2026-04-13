# タスクリスト: Game097v2 PixelEvolution

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（ユニーク系カラーパレット使用）

## フェーズ2: C# スクリプト実装
- [x] PixelEvolutionGameManager.cs（StageManager・InstructionPanel統合）
- [x] EvolutionManager.cs（コアメカニクス・5ステージ難易度対応）
- [x] PixelEvolutionUI.cs（ステージ表示・コンボ表示対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup097v2_PixelEvolution.cs（InstructionPanel・StageManager配線含む）

## 実装後の振り返り

**実装完了日**: 2026-04-09

**計画と実績の差分**:
- 計画通りに3スクリプト + SceneSetup + 13スプライトを実装
- Unity MCPセッションがタイムアウトしたためコンパイル検証・SceneSetup・PlayMode検証はスキップ
- GameRegistry.jsonの`implemented: true`更新は未実施（Unity MCP検証未完了のため）

**学んだこと**:
- `AddVoidPersistentListener`は存在しないAPI、正しくは`AddPersistentListener`
- コードレビューで「進化レベルUI未更新」と「光量UIの欠落」が指摘され即修正→設計段階でのUI更新漏れを防ぐためにSetupStage以外でも`UpdateEvolutionLevel`を呼ぶことを設計書に明記すべき

**次回への改善提案**:
- 環境パラメータが3種類以上ある場合、SceneSetupのパネルサイズを最初から3行分確保する
- 全パラメータがレビューで確認できるよう、design.mdの「SceneSetupで配線が必要なフィールド」にUI更新メソッドの呼び出しタイミングも記述する

### 事後検証（2026-04-09）
- ✅ コンパイル: エラーなし（0件）
- ✅ SceneSetup: Assets/Setup/097v2 PixelEvolution 実行成功
- ✅ PlayMode: ランタイムエラーなし、プレイヤー・エネミースプライト表示確認済み（game_viewでも視認可能）
- ✅ GameRegistry.json: implemented: true に更新完了
