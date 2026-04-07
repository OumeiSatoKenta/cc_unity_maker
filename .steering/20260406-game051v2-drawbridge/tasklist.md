# タスクリスト: Game051v2_DrawBridge

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, LeftCliff, RightCliff, Ball, Goal, Rock, Wind, Coin）

## フェーズ2: C# スクリプト実装
- [x] DrawBridgeGameManager.cs（StageManager・InstructionPanel統合、5ステージ設定）
- [x] DrawingManager.cs（線描画・インク管理・物理EdgeCollider2D生成・入力処理）
- [x] BallController.cs（Rigidbody2D制御・GOボタン起動・ゴール/落下判定）
- [x] DrawBridgeUI.cs（インクゲージ・スコア・ステージ・各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup051v2_DrawBridge.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- 計画と実績の差分:
  - BreakableLineComponent の `_threshold` デフォルト値を 0 から `float.MaxValue` に修正（コードレビューで発見）
  - FallDetector の設計を削除し、BallController の FixedUpdate y位置チェックに一本化（structural review指摘）
  - AllClearパネルのボタン配線をSceneSetupのAddListenerからSerializeFieldへ移動（structural review指摘）
  - ClearLines と CreatePhysicsLine の _currentLineObj 管理バグを修正（secondary review指摘）
  - `Tag: Ball is not defined` エラーは manage_editor add_tag で事前登録して解決
- 学んだこと:
  - PhysicsMaterial2D は AssetDatabase.CreateAsset で保存しないとシーン保存後に消える
  - 動的タグ設定（`obj.tag = "Ball"`）はUnityのTagManagerに事前登録が必要
  - BreakableLineComponent のデフォルト閾値は「壊れない」方向に安全側に設定すること
- 次回への改善提案:
  - 物理マテリアル生成パターンは SetupXXX.cs のボイラープレートに含める
  - タグ登録チェックをSceneSetup冒頭に追加するパターンを標準化する
