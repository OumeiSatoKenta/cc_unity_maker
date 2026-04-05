# タスクリスト: Game018v2_TimeRewind

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Floor, Wall, Player, Goal, Switch, Ice, Bomb, Ghost）

## フェーズ2: C# スクリプト実装
- [x] TimeRewindGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] BoardManager.cs（盤面・コマ移動・特殊マス・移動履歴・巻き戻し・分身処理）
- [x] TimeRewindUI.cs（HUD・タイムラインパネル・各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup018v2_TimeRewind.cs（InstructionPanel・StageManager配線・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り
（実装完了後に記入）
