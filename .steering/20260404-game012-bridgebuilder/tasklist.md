# タスクリスト: Game012v2_BridgeBuilder

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Anchor, WoodPlank, SteelBeam, Rope, Car, Water, Goal）

## フェーズ2: C# スクリプト実装
- [x] BridgeBuilderGameManager.cs（StageManager・InstructionPanel統合・スコア計算）
- [x] BridgeManager.cs（橋設計・荷重計算・テスト走行・5ステージ対応）
- [x] BridgeBuilderUI.cs（UI表示・パーツボタン・テストボタン）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup012v2_BridgeBuilder.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-04
- コードレビューで4件の[必須]指摘を修正（スプライト選択バグ・DistanceToSegmentの誤実装・0-based/1-based混在・コルーチン二重起動）
- StageManagerの引数は0-basedで渡されるため、SetupStage内では+1して1-basedに変換することを徹底
- `Sprite[] _spanTypes` をAwakeではなくSetupStage時に初期化する（SerializedFieldはAwake時点では未設定）
