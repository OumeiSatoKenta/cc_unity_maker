# タスクリスト: Game061v2_CookieFactory

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成

## フェーズ2: C# スクリプト実装
- [x] CookieFactoryGameManager.cs（StageManager・InstructionPanel統合）
- [x] CookieManager.cs（コアメカニクス・5ステージ難易度対応）
- [x] CookieFactoryUI.cs（ステージ表示・コンボ表示対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup061v2_CookieFactory.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- 計画と実績の差分: ほぼ計画通り。コードレビューで4件の[必須]指摘（低レート自動生産サイレント・コルーチン残存・VIP競合・VIPタイマーUI）があり、実装後に修正。
- 学んだこと:
  - UnityEventTools.AddPersistentListenerはラムダ不可。引数なしのwrapperメソッド必須。
  - `StageManager.NextStage()` → `CompleteCurrentStage()` のAPIチェックを事前に行うこと。
  - InstructionPanelの内部フィールドは`_panelRoot`（`_panel`ではない）。
  - 低レートの自動生産はaccumulatorパターンが必須（`(long)(rate * dt)` は0になる）。
- 次回への改善提案: SceneSetupのボタン配線でラムダを使いたい場合は最初からwrapperメソッドを設計段階で列挙しておく。
