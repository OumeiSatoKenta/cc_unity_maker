# タスクリスト: Game050v2_BubbleSort

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Bubble_Green, Bubble_Yellow, Bubble_Blue, Bubble_Red, Bubble_Purple, Bubble_Fixed, Bubble_Timer, Bubble_Bomb, BubbleSelected）

## フェーズ2: C# スクリプト実装
- [x] BubbleCell.cs（バブルデータ: 色・タイプ・タイマー・SpriteRenderer制御）
- [x] BubbleSortGameManager.cs（StageManager・InstructionPanel統合・スコア管理・状態遷移）
- [x] BubbleGridManager.cs（グリッド管理・入力処理・入れ替え・3連消し・固定/タイマー/爆弾バブル・ソート判定）
- [x] BubbleSortUI.cs（HUD・クリア/ゲームオーバーパネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup050v2_BubbleSort.cs（InstructionPanel・StageManager配線・全UI構成含む）

## フェーズ4: GameRegistry.json 更新
- [ ] implemented: true に変更（remakeエントリー）

## 実装後の振り返り
（実装完了後に記入）
