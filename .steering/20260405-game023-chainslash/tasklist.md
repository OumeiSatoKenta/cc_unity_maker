# タスクリスト: Game023v2 ChainSlash

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（EnemyRed, EnemyBlue, EnemyShield, EnemyBomb, ChainLink, Background）

## フェーズ2: C# スクリプト実装
- [x] ChainSlashGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] ChainSlashController.cs（敵生成・ドラッグ入力・鎖・斬撃・5ステージ対応・レスポンシブ配置）
- [x] ChainSlashUI.cs（スコア・タイマー・コンボ倍率・各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup023v2_ChainSlash.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り
（実装完了後に記入）
