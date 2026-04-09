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
（実装完了後に記入）
