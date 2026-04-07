# タスクリスト: Game069v2_DungeonDigger

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・ブロック・アイテム・生物・UI）

## フェーズ2: C# スクリプト実装
- [x] DungeonDiggerGameManager.cs（StageManager・InstructionPanel統合）
- [x] DigManager.cs（掘削コアメカニクス・5ステージ難易度対応）
- [x] DungeonDiggerUI.cs（ステージ表示・コンボ表示・アップグレードUI）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup069v2_DungeonDigger.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

**実装完了日**: 2026-04-07

**計画と実績の差分**:
- 計画通り全タスクを完了
- モンスターの専用GameObject管理（_monsterObj等）をデッドコードとして削除し、ブロックタイプとして処理する設計に統一

**コードレビューで発見・修正した問題**:
- BuyDrillUpgrade のオフバイワンバグ（`>=Length+1` → `>Length`）
- DungeonDiggerUI がシーンルートに浮いていた（GameManager子に配置）
- 未使用の_monsterObj/_monsterSrフィールド削除

**次回への改善提案**:
- StageConfigのspeedMultiplier/countMultiplierをDigManagerのautoRateなどに実際に反映させる
- タッチ入力（Touchscreen.current）対応を追加する
