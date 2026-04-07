# タスクリスト: Game057v2_CandyDrop

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Ground, Candy_Circle, Candy_Square, Candy_Triangle, Candy_Star, Candy_Melt, Candy_Giant）

## フェーズ2: C# スクリプト実装
- [x] CandyDropGameManager.cs（StageManager・InstructionPanel統合）
- [x] CandySpawner.cs（キャンディ生成・落下・入力処理・5ステージ難易度対応）
- [x] TowerChecker.cs（高さ監視・クリア判定・崩壊検出・台振動）
- [x] CandyDropUI.cs（ステージ/スコア/コンボ/ゲージ/クリアパネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup057v2_CandyDrop.cs（InstructionPanel・StageManager・Ground・Wall・目標ライン配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- PR: #319 → main マージ済み

### 計画と実績の差分
- 計画通り5スクリプト + SceneSetup + 18スプライトを生成
- コードレビューで4件の[必須]修正が発生（_totalScore追加、HasLanded追加、GoalLineトランスフォーム配線、Camera.main nullチェック）

### 学んだこと
- TowerCheckerのGoalLine座標更新はSerializeFieldで_goalLineTransformを受け取る必要がある
- CandyController.HasLandedをpublicにしてTowerCheckerのGetTowerHeightでフィルタしないと飛行中キャンディも高さに含まれてしまう
- countMultiplierはint型なのでSerializedPropertyでは.intValueを使う（.floatValueは型ミスマッチ）

### 次回への改善提案
- SetupStageの冒頭Camera.main nullチェックはテンプレートに組み込む
- OnDestroy()でのイベント解除もGameManagerテンプレートに追加する
