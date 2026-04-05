# タスクリスト: Game010v2_GearSync

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（SmallGear, LargeGear, PowerSource, GoalGear, Belt, Background）

## フェーズ2: C# スクリプト実装
- [x] GearSyncGameManager.cs（StageManager・InstructionPanel統合）
- [x] GearSyncManager.cs（グリッド管理・伝達シミュレーション・5ステージ対応）
- [x] GearSyncUI.cs（UI管理・パーツリスト・テスト結果表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup010v2_GearSync.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
- 実装完了日: 2026-04-03
- StageManager.AdvanceStage() が存在せずCompleteCurrentStage()に修正
- InstructionPanel のフィールド名が `_panel` でなく `_panelRoot` だった（他ゲームから確認必須）
- reShowBtn を InstructionPanel 配線前に生成するよう順序修正
- ベルト接続は「2点タップ」方式で実装（1点置くだけでは不十分）
- Camera.main はAwake/SetupStageでキャッシュし、コルーチン内でnullチェック
