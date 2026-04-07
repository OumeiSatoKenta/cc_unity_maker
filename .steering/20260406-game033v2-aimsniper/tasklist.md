# タスクリスト: Game033v2 AimSniper

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Scope, Target, MovingTarget, Obstacle, WindIndicator）

## フェーズ2: C# スクリプト実装
- [x] TargetController.cs（ターゲット移動・ステート・ヒット処理）
- [x] AimSniperMechanic.cs（照準・射撃・入力一元管理・5ステージ対応）
- [x] AimSniperGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] AimSniperUI.cs（HUD・ステージ/スコア/弾数/パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup033v2_AimSniper.cs（InstructionPanel・StageManager配線・UI構築含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- PR: OumeiSatoKenta/cc_unity_maker#294（マージ済み）

### 計画と実績の差分
- SceneSetupでInstructionPanelのフィールド名が`_panel`ではなく`_panelRoot`だった（他のSetupを参照して修正）
- `_helpButton`の配線がCreateInstructionPanel内になかったため、呼び出し元でipSOWireを使って追加配線
- `AddButtonOnClick(startBtn, ip, "OnStartButtonPressed")`は不要（InstructionPanel.Start()内でAddListenerするため）削除

### 学んだこと
- InstructionPanelのシリアライズフィールド名は`_panelRoot`・`_helpButton`（他のSetupで確認必須）
- CreateInstructionPanelヘルパーに渡せないGameObjectは、戻り値後に別SerializedObjectで配線する

### 次回への改善提案
- CreateInstructionPanelのシグネチャに`helpButton`パラメータを追加して内部で配線できるようにする
