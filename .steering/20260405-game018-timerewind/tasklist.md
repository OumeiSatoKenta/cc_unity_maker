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

**実装完了日**: 2026-04-05

**計画と実績の差分**:
- CancelRewind()メソッドがSceneSetupで必要になり、実装後に追加した
- ToggleWalls()が空実装で残っており、コードレビューで発覚→壁を全除去するシンプル実装に修正
- RewindTo()でタイムライン非表示漏れ、SlideOnIceとの競合など7件の[必須]バグをコードレビューで検出・修正

**学んだこと**:
- 巻き戻しメカニクスでは「コルーチン中の状態遷移」に特に注意が必要
- PlaceBombs()のグリッドサイズ依存バグは設計段階でパラメータを確認すべき
- ApplyCellEffect/CheckWinConditionの二重起動は責務分担を明確にすれば防げる

**次回への改善提案**:
- コルーチン管理は`_slidingCoroutine`など参照を保持してStopCoroutine(ref)を使うとより安全
- 特殊マスの実装は設計書に「どのメソッドが処理するか」を1対1で明記すると漏れが減る
