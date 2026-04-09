# タスクリスト: Game091v2 TimeBlender

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（背景・プレイヤー・タイル各種・ゴール）

## フェーズ2: C# スクリプト実装
- [x] TimeBlenderGameManager.cs（StageManager・InstructionPanel統合・スコア・コンボ）
- [x] PuzzleManager.cs（タイルグリッド・時代切替・キャラ移動・パラドックス判定・5ステージ対応）
- [x] TimeBlenderUI.cs（HUD・時代表示・ステージクリア/ゲームオーバーパネル・ボタンイベント）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup091v2_TimeBlender.cs（全オブジェクト生成・フィールド配線・シーン保存）

## 実装後の振り返り

**実装完了日**: 2026-04-09

**計画と実績の差分**:
- ステージデータをPuzzleManager内の静的メソッドで定義したため、クラスが542行と大きくなった（分離は推奨事項として記録）
- 2026-04-09 Unity MCP再接続後にコンパイル/SceneSetup/PlayMode検証を実施・完了
- SceneSetup: シーン作成完了ログ確認 OK
- PlayMode: 夜空背景＋時代区切りライン表示確認 OK（エラーなし）
- コードレビューで必須4件を発見・修正（StopAllCoroutines→個別管理、nullチェック追加×3、二重リスナー削除）

**学んだこと**:
- `StopAllCoroutines()` は他のFlash系コルーチンも停止させるため、個別コルーチン変数管理が必須
- Camera.main は Update() 内でも毎フレームnullチェックすると安全
- 3時代（Past/Present/Future）設計では、タイルのIsPassableとGetSpriteForTileを一致させることが重要

**次回への改善提案**:
- ステージデータは別クラス（XxxStageData.cs）に分離してPuzzleManager本体を300行以内に収める
- BridgePast/TreePastのPresent時代での挙動をコメントで明記する
