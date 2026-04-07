# タスクリスト: Game034v2 DropZone

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成

## フェーズ2: C# スクリプト実装
- [x] DropZoneGameManager.cs（StageManager・InstructionPanel統合）
- [x] FallingItem.cs（アイテム落下・ドラッグ追従）
- [x] DropZoneMechanic.cs（アイテム生成・ゾーン判定・5ステージ対応）
- [x] DropZoneUI.cs（スコア・ミス・コンボ・ステージ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup034v2_DropZone.cs（全配線・InstructionPanel・StageManager含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- **実装完了日**: 2026-04-06
- **計画と実績の差分**:
  - DropZoneMechanic から `_ui` フィールドを削除（未使用フィールドのためレビューで指摘）
  - `FallingItem` に `_callbackFired` フラグと `HasBeenProcessed` プロパティを追加（コールバック二重発火防止）
  - `ClearItems()` に `_activeDropCount = 0` のリセット追加（ステージ切り替え時のカウント不整合防止）
  - `HandleDrop` / `OnItemFellOff` に `HasBeenProcessed` ガード追加（同フレームでの二重処理防止）
  - `HandleInput` にタッチ入力（`Touchscreen.current`）対応追加
  - `DropZoneUI.Initialize` に `RemoveAllListeners()` 追加（リスタート時の重複リスナー防止）
- **学んだこと**:
  - FallingItem はコルーチン中に `Deactivate()` が呼ばれるケースがあるため、コールバックフラグは必須
  - `_activeDropCount` はリスト以外にも明示的リセットが必要（`ClearItems` のスコープ外のデクリメントを考慮）
  - 未使用 SerializeField はSceneSetup実装前に削除する習慣をつける
- **次回への改善提案**:
  - コアメカニクスのアイテム管理は設計段階から「処理済みフラグ」を盛り込む
  - `_ui` 未使用フィールドはレビュー前に自己チェックで検出できるよう意識する
