# タスクリスト: Game047v2_SpinBalance

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Platform, Coin, HeavyCoin, LightCoin, BounceCoin, MagnetCoin, BrakeIcon, DangerEffect）

## フェーズ2: C# スクリプト実装
- [x] SpinBalanceGameManager.cs（StageManager・InstructionPanel統合、スコア管理、ブレーキ未使用フラグ）
- [x] BalanceManager.cs（盤面回転制御、コマ管理、落下判定、ブレーキ機能、5ステージ難易度対応）
- [x] SpinBalanceUI.cs（タイマー・スコア・コマ数・コンボ倍率・危険表示・ブレーキアイコン）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup047v2_SpinBalance.cs（InstructionPanel・StageManager配線、Platform/Coin配置）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
