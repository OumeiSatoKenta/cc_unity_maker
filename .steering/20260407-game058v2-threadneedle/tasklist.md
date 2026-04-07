# タスクリスト: Game058v2_ThreadNeedle

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Needle, NeedleBody, Thread, NeedleHole）

## フェーズ2: C# スクリプト実装
- [x] ThreadNeedleGameManager.cs（StageManager・InstructionPanel統合）
- [x] NeedleController.cs（針揺れ・射出判定・ラウンド管理・5ステージ対応）
- [x] ThreadNeedleUI.cs（ステージ表示・コンボ・ミス・判定テキスト対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup058v2_ThreadNeedle.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: #320 → main マージ済み

### 計画と実績の差分
- 設計通り5ステージ・3クラス構成で実装完了
- dualHoleMode のMISS時バグ（_isShooting が true のまま残る）を実装後レビューで発見・修正
- `eulerAngles` を使った偏差計算バグをワールド座標直接参照に修正
- AddButtonClick のラムダ方式を UnityEventTools.AddPersistentListener に変更（シーン保存後消失対策）

### 学んだこと
- 回転付きオブジェクトの判定は `transform.position.x` で直接ワールド座標を取る方が確実
- `sed -i` はmacOSでは `sed -i ''` が必要。Editツールを使う方が安全
- dualHoleMode の分岐は「成功+2回目待機」と「MISS+即リセット」を明確に分ける必要がある

### 次回への改善提案
- 複数入力パスの混在（legacy + input system）に早期に気づくチェック項目を追加
- MISS時の状態リセット（_isShooting, _waitingFor*）はパターンとしてチェックリスト化する
