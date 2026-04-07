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

- 実装完了日: 2026-04-06
- PRマージ: #313

### バグ修正（設計時に見逃したポイント）
1. **ステージクリアが発火しない**: SinkNailAnimation コルーチン内で CurrentNailIndex++ するが、GameManager側は即時チェックしていたため最後の釘で発火しなかった。OnAllNailsDriven イベントをコルーチン完了後に発火するよう修正。
2. **スケール蓄積**: HighlightCurrentNail() が localScale に 1.1f を掛け続けて釘が大きくなり続けた。_nailBaseScales リストに初期スケールを保存して絶対値適用に修正。
3. **ハンマーアニメーション競合**: _hitAnimCoroutine（釘用）と _hammerAnimCoroutine（ハンマー用）が共用されていたため分離。

### 学んだこと
- コルーチン内で状態変更する場合、完了タイミングをイベントで外部に通知する設計が重要
- 複数アニメが走るコンポーネントはコルーチンフィールドを用途別に分離すること
