# タスクリスト: Game089v2_IslandHop

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・島5種・施設11種・資源4種・UI要素）

## フェーズ2: C# スクリプト実装
- [x] IslandHopGameManager.cs（StageManager・InstructionPanel統合）
- [x] IslandManager.cs（島・施設・資源・シナジー・5ステージ難易度対応）
- [x] IslandHopUI.cs（ステージ表示・スコア・コンボ・資源・施設選択パネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup089v2_IslandHop.cs（InstructionPanel・StageManager配線含む）

## 実装後の振り返り

- **実装完了日**: 2026-04-09
- **計画と実績の差分**: 計画通り実装完了。設計書で定義した5ステージ・施設シナジー・天候イベント・訪問客リクエストをすべて実装。
- **学んだこと**:
  - IslandManager が大きくなりやすい（560行）。FacilityData を別ファイルに分離すべきだった
  - コードレビューで絵文字使用のルール違反が検出された（NotoSansJP未対応）。今後は最初から [木] 等の代替テキストを使う
  - BuildPulse/SynergyEffect コルーチンの _activeCoroutines への追加忘れは典型的なパターン。コルーチン作成時は即座に追加する習慣をつける
- **次回への改善提案**:
  - FacilityData 等のデータクラスは最初から別ファイルに分離する
  - 絵文字はNotoSansJP未対応のため使用しない（[木][石]等の代替テキストを使う）
  - Camera.main はSetupStage冒頭でキャッシュする
