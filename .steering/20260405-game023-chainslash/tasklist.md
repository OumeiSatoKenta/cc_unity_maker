# タスクリスト: Game023v2 ChainSlash

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（EnemyRed, EnemyBlue, EnemyShield, EnemyBomb, ChainLink, Background）

## フェーズ2: C# スクリプト実装
- [x] ChainSlashGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] ChainSlashController.cs（敵生成・ドラッグ入力・鎖・斬撃・5ステージ対応・レスポンシブ配置）
- [x] ChainSlashUI.cs（スコア・タイマー・コンボ倍率・各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup023v2_ChainSlash.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

**実装完了日**: 2026-04-05

**計画と実績の差分**:
- StageManager.StageConfig に `stage` フィールドが存在しないため CS1061 コンパイルエラー。`SetupStage` を `(StageConfig config, int stageNumber)` に変更して解決。
- Controller 内フレーム毎 GetComponent 設計ミスを構造レビューで指摘。SerializeField + SceneSetup 配線に変更。

**学んだこと**:
- StageConfig フィールドは `speedMultiplier`, `countMultiplier`, `complexityFactor`, `stageName` のみ。ステージ番号は別引数で渡す。
- コルーチンと ClearAllEnemies 競合防止には `isDestroyed` フラグが有効。

**次回への改善提案**:
- SetupStage シグネチャを設計段階で (StageConfig, int) の2引数と明記する。
