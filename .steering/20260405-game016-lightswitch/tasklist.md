# タスクリスト: Game016v2_LightSwitch

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（BulbOn, BulbOff, BulbRed, BulbBlue, BulbWarp, BulbDelayed, Background）

## フェーズ2: C# スクリプト実装
- [x] LightSwitchGameManager.cs（StageManager・InstructionPanel統合）
- [x] BulbManager.cs（コアメカニクス・5ステージ難易度・特殊電球・Undo）
- [x] LightSwitchUI.cs（HUD・パネル管理・目標パターン表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup016v2_LightSwitch.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

**実装完了日**: 2026-04-05

**計画と実績の差分**:
- 計画通り4フェーズ完了（スプライト→スクリプト→SceneSetup→Registry更新）
- コードレビューで[必須]指摘が複数あり、BulbManager.csを全面書き直し（当初より1ラウンド多い）

**主な問題と解決**:
1. ColoredRed/Blue電球のチェッカーボード方式→行ベース方式に変更（隣接伝播が機能するため）
2. Undoシステムが`pendingDelays`キューと`shiftCount`を復元していなかった→UndoFrame構造体に統合
3. SetupStageメソッドが重複定義されていた（コピペミス）→単一メソッドにリファクタ
4. GenerateInitialStateが初期状態でクリア済みになる可能性→20回以内リトライループで対処

**学んだこと**:
- ライツアウト系は「隣接伝播の色フィルタ」設計が核心。設計段階でチェッカーボードか行ベースかを明確にすべき
- Undoシステムは「保存するべき全状態」を列挙してからUndoFrame設計する

**次回への改善提案**:
- design.mdに「Undoで復元が必要な全フィールドリスト」を明記する習慣をつける
