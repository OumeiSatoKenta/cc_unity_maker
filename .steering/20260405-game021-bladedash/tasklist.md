# タスクリスト: Game021v2_BladeDash

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Player, Blade, Coin, Background, LaneLine）

## フェーズ2: C# スクリプト実装
- [x] BladeDashGameManager.cs（StageManager・InstructionPanel統合）
- [x] BladeRunner.cs（コアメカニクス・5ステージ難易度対応・スワイプ入力）
- [x] BladeDashUI.cs（ステージ/スコア/コンボ/各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup021v2_BladeDash.cs（InstructionPanel・StageManager・全UI配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-05
- 計画と実績の差分:
  - `_stageConfigs` SerializedProperty は StageManager に存在しないため、Setup スクリプトのカスタム設定ブロックを削除し、StageManager のデフォルト生成に委ねた。BladeRunner 側は `complexityFactor`（float→int キャスト）で刃タイプ比率を制御するため問題なし。
  - Setup 実行時に MCP ExecuteMenuItem が内部的に PlayMode 中と判定される事象が発生。2回目の実行で正常に完了した。
- 学んだこと:
  - `StageManager._configs` はプライベートかつ Awake() 生成のため、SerializedObject 経由での設定は不可能。ゲーム固有パラメータは GameManager または CoreManager 側で持つ設計にすべき。
- 次回への改善提案:
  - Setup スクリプトで StageManager のカスタム設定が必要な場合は、`[SerializeField]` な設定配列を StageManager に追加するか、ゲーム側でパラメータを完全に管理する設計にする。
