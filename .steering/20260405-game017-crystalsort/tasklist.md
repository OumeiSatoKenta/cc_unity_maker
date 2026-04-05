# タスクリスト: Game017v2_CrystalSort

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Crystal×6色, Rainbow, Frozen, Bottle, BottleCapped, BottleTimer, BottleSelected, BottleComplete）

## フェーズ2: C# スクリプト実装
- [x] CrystalSortGameManager.cs（StageManager・InstructionPanel統合）
- [x] BottleManager.cs（コアメカニクス: 瓶・クリスタル管理、入力処理、移動ロジック、5ステージ難易度対応）
- [x] CrystalSortUI.cs（ステージ表示・スコア・残り手数・コンボ・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup017v2_CrystalSort.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

実装完了日: 2026-04-05

### 計画と実績の差分
- 設計通り3クラス構成で実装完了
- CrystalSortUI から未使用ボタンフィールド4本を除去（設計で見落とし、レビューで検出）
- BottleManager._ui を FindFirstObjectByType から SerializeField に変更（レビューで [必須] 指摘）
- Coroutine内のnullガード追加（コンテキスト圧縮後に再適用）
- Rainbow移動時のコンボ追跡バグを修正（Normalクリスタルのみ_lastMovedColorを更新）

### 学んだこと
- UIクラスにボタン参照を持たせても SceneSetup で配線しない限り無意味。設計段階でボタンは GameManager 側が OnClick 経由で持つ設計にすべき
- コルーチン内の null ガードは、Destroy が発生しうるゲームでは必須。SetupStage() で ClearAll() を呼ぶパターンは特に注意
- 特殊クリスタル（Rainbow/Frozen）がある場合、pool生成の数学的整合性を事前に計算してassertすること

### 次回への改善提案
- 設計書（design.md）にpool計算の整合性チェック式を必須記載する
- UIクラスのSerializeFieldにボタン参照を追加する際は、SceneSetup配線と必ずセットで設計する
