# タスクリスト: Game049v2 CloudHop

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Player, Cloud_Normal, Cloud_Spring, Cloud_Thunder, Cloud_Moving, Coin）

## フェーズ2: C# スクリプト実装
- [x] CloudHopGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] CloudHopController.cs（プレイヤー制御・ジャンプ・左右移動・急降下・コンボ）
- [x] CloudObject.cs（雲の種類・消失タイマー・バネ/雷/動く効果）
- [x] CloudSpawner.cs（雲生成・縦スクロール・コイン生成・5ステージ対応）
- [x] CoinObject.cs（コインの当たり判定）
- [x] CloudHopUI.cs（高度・スコア・コンボ・コイン数・パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup049v2_CloudHop.cs（全フィールド配線・InstructionPanel・StageManager含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
