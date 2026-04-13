# タスクリスト: Game099v2_TouchMemory

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（パネル4色、背景、点灯パネル）

## フェーズ2: C# スクリプト実装
- [x] TouchMemoryGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] TouchMemoryManager.cs（コアメカニクス：パターン生成・再生・入力受付・判定、5ステージ対応）
- [x] TouchMemoryUI.cs（HUD、ステージ/ゲームオーバー/全クリアパネル、ボタンイベント）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup099v2_TouchMemory.cs（InstructionPanel・StageManager配線、パネル生成、全UI構築）

## コードレビュー結果

- structural: C → 必須修正適用済み（customData追加、_helpButton修正、Camera.mainキャッシュ、コルーチン管理）
- secondary: C → 必須修正適用済み（null guard、StopAllCoroutines、speedMultiplierクランプ）

## 実装後の振り返り

- **実装完了日**: 2026-04-09
- **計画と実績の差分**:
  - StageManager.StageConfig に customData フィールドが存在しなかった（コードレビューで発見・修正）
  - InstructionPanel の `_helpButton` フィールド名を誤記（`_reopenButton`）していた（コードレビューで発見・修正）
  - Camera.main のキャッシュ未実装・コルーチン管理の不備をレビューで検出・修正
  - Unity MCP セッションがタイムアウトのためコンパイル/SceneSetup/PlayMode検証はスキップ
- **学んだこと**:
  - StageConfig の拡張時は既存フィールドを事前確認する
  - InstructionPanel のフィールド名は他ゲームのSceneSetupから必ず確認する
  - Camera.main はキャッシュしてから使う（毎フレームの検索コスト+nullリスク回避）
- **次回への改善提案**:
  - SceneSetup実装前に InstructionPanel / StageManager のフィールド名一覧を再確認するステップを追加する

### 事後検証（2026-04-09）
- ✅ コンパイル: エラーなし（0件）
- ✅ SceneSetup: Assets/Setup/099v2 TouchMemory 実行成功
- ✅ PlayMode: ランタイムエラーなし、背景・スプライト表示確認済み
- ✅ GameRegistry.json: implemented: true に更新完了
