# タスクリスト: Game026v2_SliceNinja

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Fruit, Bomb, FrozenFruit, MiniFruit, MissIcon）

## フェーズ2: C# スクリプト実装
- [x] FlyingObject.cs（飛来物体タイプ・挙動・切断処理）
- [x] SliceManager.cs（スワイプ軌跡・切断判定・スポーン管理）
- [x] SliceNinjaGameManager.cs（StageManager・InstructionPanel統合、スコア・ゲームオーバー管理）
- [x] SliceNinjaUI.cs（スコア・コンボ・ミス・ステージ・各種パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup026v2_SliceNinja.cs（全GameObject生成・配線）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

**実装完了日**: 2026-04-05

**計画と実績の差分**:
- SceneSetupでStageManager SerializedObjectを使って初期configを設定しようとしたが、`_configs`フィールドはprivateのため操作不可。`SetConfigs()`メソッド呼び出しをStartGame()内に移動して解決。
- `countMultiplier`がintであることを失念し、SceneSetupでfloat値を設定しようとしてコンパイルエラー。countMultiplier=1,1,2,2,2に修正。
- SliceNinjaUI.Initialize()はStartGame()でも呼ぶ必要があった（SceneSetupのエディタ時呼び出しでは_gameManagerがnullのまま）。

**学んだこと**:
- StageManager.StageConfigのcountMultiplierはint型。SceneSetupで直接SerializedObject操作するよりStartGame()でSetConfigs()を使う方が安全。
- FlyingObjectのCoroutineは`this == null`と`_spriteRenderer != null`の両方をガードしないとMissingReferenceExceptionが発生する。
- DisableInput()は即座にスワイプ処理を止めるため、GameOver時のUI操作を安全にする重要なメソッド。

**次回への改善提案**:
- SceneSetupでStageConfigを設定する際は、SerializedObject操作ではなくStartGame()内SetConfigs()パターンを最初から採用する。
- FlyingObjectのような破棄タイミングが複雑なオブジェクトには、最初からnull安全coroutineテンプレートを適用する。
