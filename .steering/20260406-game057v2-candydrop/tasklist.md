# タスクリスト: Game057v2_CandyDrop

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Ground, Candy_Circle, Candy_Square, Candy_Triangle, Candy_Star, Candy_Melt, Candy_Giant）

## フェーズ2: C# スクリプト実装
- [x] CandyDropGameManager.cs（StageManager・InstructionPanel統合）
- [x] CandySpawner.cs（キャンディ生成・落下・入力処理・5ステージ難易度対応）
- [x] TowerChecker.cs（高さ監視・クリア判定・崩壊検出・台振動）
- [x] CandyDropUI.cs（ステージ/スコア/コンボ/ゲージ/クリアパネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup057v2_CandyDrop.cs（InstructionPanel・StageManager・Ground・Wall・目標ライン配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
