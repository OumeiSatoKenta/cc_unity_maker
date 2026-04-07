# タスクリスト: Game056v2_InflateFloat

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Balloon, Background, Obstacle, Coin, GoalFlag, Spike）

## フェーズ2: C# スクリプト実装
- [x] InflateFloatGameManager.cs（StageManager・InstructionPanel統合）
- [x] BalloonController.cs（風船物理・入力処理・5ステージ対応）
- [x] CourseManager.cs（コース生成・スクロール・障害物管理）
- [x] InflateFloatUI.cs（膨張ゲージ・ステージ表示・コンボ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup056v2_InflateFloat.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- PR: #317 (マージ済み)

### 計画と実績の差分
- 距離ベーススポーン方式への変更: 位置ベース比較がfloat精度問題になるため、`_traveledDist`累積方式に変更。
- ObstacleInfoコンポーネント追加: Spike挿入によるリストインデックスズレを防ぐため、isTopPipeフラグをコンポーネントで管理。

### 学んだこと
- 障害物リストに複数種類のオブジェクト（パイプ・スパイク）が混在する場合、インデックス計算は壊れやすい→コンポーネントパターンが安全
- UnityMCPがタイムアウトする場合はcompile/SceneSetupを手動で行う

### 次回への改善提案
- Rigidbody2D.linearVelocity はUnity 6対応API（旧バージョンでは .velocity を使うこと）
