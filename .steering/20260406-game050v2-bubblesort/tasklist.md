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

**実装完了日**: 2026-04-06

**計画と実績の差分**:
- StageManagerの`_stages`フィールドがSerializedObjectで見えないことが判明。SetConfigs()をGameManager.StartGame()で呼ぶパターンに変更。
- BubbleGridManager: isChainバグ（最初のマッチもchain扱い）を修正。
- OnTimerBubbleExpiredに`_isProcessing`ガードを追加（スワップ中のタイマー期限切れ競合防止）。
- SetColorIndex内の`_sr.color`nullチェック漏れを修正。

**学んだこと**:
- StageManagerの`_configs`はprivateでシリアライズ不可。SetConfigs()をGameManager側で呼ぶのが正しいパターン。
- isChain判定は「現在のマッチ前に既にfoundAnyがtrueか」で判断する（`bool isChain = foundAny; foundAny = true;`の順序が重要）。

**次回への改善提案**:
- SceneSetup内でStageManagerをSerializedObjectで設定しようとする誤りは過去にも発生。design.mdにSetConfigs()パターンを明記する。
