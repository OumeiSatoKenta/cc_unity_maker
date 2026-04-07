# タスクリスト: Game038v2 FlyBird

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・鳥・パイプ・コイン・地面）

## フェーズ2: C# スクリプト実装
- [x] FlyBirdGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] BirdController.cs（タップ入力・Rigidbody2D物理・ビジュアルフィードバック）
- [x] PipeSpawner.cs（パイプ動的生成・移動・5ステージ難易度対応）
- [x] FlyBirdUI.cs（スコア・ステージ・コンボ・クリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup038v2_FlyBird.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- PR: OumeiSatoKenta/cc_unity_maker#299（マージ済み）

### 計画と実績の差分
- PipePair の移動バグ（_basePosition.x 上書き問題）は設計時に想定外だったが、コードレビューで検出して修正
- PipeSpawner への BirdController 注入が当初 FindFirstObjectByType だったが、SerializeField に変更
- SceneSetup での spawnerSO.ApplyModifiedProperties() の呼び出しタイミング（_birdController セット前に呼んでいた）を修正

### 学んだこと
- SerializedObject.ApplyModifiedProperties() はすべてのフィールドを設定した後に1回だけ呼ぶこと
- Unity タグ（Ground, Obstacle, Coin, ScoreTrigger）は SceneSetup 実行前に TagManager.asset に登録が必要

### 次回への改善提案
- SceneSetup でゲーム固有のタグを自動追加する処理を最初から組み込む
- BirdController など相互依存があるコンポーネントは、SceneSetup の変数宣言順序に注意する
