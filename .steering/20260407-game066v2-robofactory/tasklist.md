# タスクリスト: Game066v2_RoboFactory

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（idle カテゴリ：紫/ピンク系パレット）

## フェーズ2: C# スクリプト実装
- [x] RoboFactoryGameManager.cs（StageManager・InstructionPanel統合）
- [x] FactoryManager.cs（コアメカニクス・ロボット管理・資源収集・建設・研究・5ステージ難易度対応）
- [x] RoboFactoryUI.cs（ステージ表示・コンボ表示・資源表示・ロボット管理UI）

## フェーズ3: SceneSetup Editor スクリプト
- [ ] Setup066v2_RoboFactory.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: #327（マージ済み）

### 計画と実績の差分
- スプライト生成で PIL ValueError（rounded_rectangle の座標不正）が発生。draw_factory の chimney 描画をガード付きに修正して解決。
- コードレビューで GetTotalActiveRobots() が故障ロボットを含む計算バグを検出・修正。
- エネルギーショート時のドレイン継続による回復不能デッドロックを修正（shortage中はdrain=0）。
- Worker ロボットの故障を防止（Parts収集が止まり修理コストを賄えなくなるため）。
- RoboFactoryGameManager に OnDestroy でイベント解除を追加（メモリリーク対策）。

### 学んだこと
- idle ゲームは「回復不能デッドロック」が発生しやすい。エネルギー・修理コスト・リソース収集の三角関係を設計時に検証すること。
- Worker は基本リソース収集の要なので故障対象から除外するのが安全。

### 次回への改善提案
- idle系ゲームの設計では「最悪ケースでも回復手段があるか」を設計書に明示すること。
