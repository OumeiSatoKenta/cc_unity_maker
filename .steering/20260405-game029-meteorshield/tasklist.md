# タスクリスト: Game029v2_MeteorShield

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Star, Shield, MeteorSmall, MeteorLarge, MeteorSplit）

## フェーズ2: C# スクリプト実装
- [x] MeteorShieldGameManager.cs（StageManager・InstructionPanel統合、ステージ時間制クリア）
- [x] MeteorObject.cs（隕石タイプ管理・移動・HP・分裂ロジック）
- [x] ShieldController.cs（マウスドラッグ入力・移動・跳ね返し・連鎖判定）
- [x] MeteorSpawner.cs（ステージ別スポーン制御・時間管理）
- [x] MeteorShieldUI.cs（星HP・スコア・コンボ・時間・ステージ表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup029v2_MeteorShield.cs（InstructionPanel・StageManager配線・全UI生成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
