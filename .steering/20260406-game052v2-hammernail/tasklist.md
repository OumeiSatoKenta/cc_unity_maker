# タスクリスト: Game052v2_HammerNail

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Board, Nail_Normal, Nail_Hard, Nail_Boss, Hammer）

## フェーズ2: C# スクリプト実装
- [x] TimingGauge.cs（ゲージ往復・PERFECT/GOOD/MISS判定・不規則速度対応）
- [x] NailManager.cs（釘の生成・管理・打撃処理・レスポンシブ配置）
- [x] HammerNailGameManager.cs（StageManager・InstructionPanel統合・スコア・コンボ管理）
- [x] HammerNailUI.cs（スコア・コンボ・ステージ・判定エフェクト・パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup052v2_HammerNail.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
