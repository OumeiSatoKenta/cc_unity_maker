# タスクリスト: Game088v2_AlchemyPet

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・素材・ペット・釜）

## フェーズ2: C# スクリプト実装
- [x] AlchemyPetGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] AlchemyManager.cs（コアメカニクス：素材管理・合成ロジック・育成・5ステージ対応）
- [x] AlchemyPetUI.cs（ステージ表示・スコア・コンボ・図鑑・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup088v2_AlchemyPet.cs（InstructionPanel・StageManager・全UI配線含む）

## 実装後の振り返り

### 実装完了日
2026-04-09

### 計画と実績の差分
- 計画通りに3スクリプト + SceneSetup + スプライト生成を完了
- 2026-04-09 Unity MCP再接続後にコンパイル/SceneSetup/PlayMode検証を実施・完了
- SceneSetup: シーン作成完了ログ確認 OK
- PlayMode: 背景スプライト表示確認 OK（エラーなし）

### 学んだこと
- `onClick.AddListener` にラムダを使うとEditor上の配線は不可だが、Start()でAddListenerすれば動的に配線できる（ボタン配線パターンの明確化）
- 素材スロット配置のUX: スロット番号を明示的に選択させるより、自動ラウンドロビン方式の方がシンプル
- AlchemyManagerはレシピDBが大きいため、buildRecipesのシンプルな文字列キー方式が有効

### 次回への改善提案
- 伝説ペット条件のような「外部状態に依存する合成条件」はGameManagerとの連携設計を事前に設計書に詳細化すること
- 大きなゲーム（工数L）はスクリプトが複雑になりやすいので、レシピDBは別クラスに分割検討
