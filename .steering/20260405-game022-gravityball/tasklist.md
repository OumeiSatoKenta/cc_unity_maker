# タスクリスト: Game022v2_GravityBall

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Ball, Obstacle, GravityZone, TriggerObstacle）

## フェーズ2: C# スクリプト実装
- [x] GravityBallGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] GravityBallController.cs（重力反転・横スクロール・障害物生成・衝突判定・5ステージ対応）
- [x] GravityBallUI.cs（ステージ・スコア・コンボ表示、各パネル制御）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup022v2_GravityBall.cs（全GameObjects生成・InstructionPanel・StageManager・UI配線）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

- 実装完了日: 2026-04-05
- PR: OumeiSatoKenta/cc_unity_maker#282（マージ済み）

### 計画と実績の差分
- 計画通り全フェーズ完了。追加修正として `ComputeLayout()` メソッド抽出（SetupStage時のレイアウト再計算）とデッドメソッド削除が発生。

### 学んだこと
- SetupStage()でも `Camera.main.orthographicSize` から再計算が必要（Start()のみでは不十分）
- `_isActive = false` はOnGameOver呼び出しの前に設定しないと二重呼び出しリスクがある
- 距離ベースのスクロールゲームは物理エンジン不使用でオブジェクトプールが効果的

### 次回への改善提案
- 重力ゾーンの視覚エフェクトをより鮮明に（現状は半透明スプライトのみ）
