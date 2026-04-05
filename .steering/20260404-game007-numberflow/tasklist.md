# タスクリスト: Game007v2 NumberFlow

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, CellNormal, CellWall, CellWarpA, CellWarpB, CellDirection, NumberBg）

## フェーズ2: C# スクリプト実装
- [x] NumberFlowGameManager.cs（StageManager・InstructionPanel統合、コンボ・スコア管理）
- [x] NumberFlowManager.cs（グリッド生成・経路管理・特殊マス・5ステージ難易度対応）
- [x] NumberFlowUI.cs（ステージ・スコア・タイマー・進捗・クリアパネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup007v2_NumberFlow.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

### 実装完了日
2026-04-04

### 計画と実績の差分
- `StageManager._stageConfigs` プロパティが存在せず、SceneSetupでの直接設定が不可能だった → `StageManager` のデフォルト設定を利用する方針に変更
- `HandlePointer` 内で `mouse` ローカル変数が参照できないコンパイルエラー → `isPress: bool` パラメータを渡す方式に修正
- `ApplyDirectionStage` でインデックス重複宣言が発生 → 旧宣言を削除して修正

### 学んだこと
- `StageManager` の `_stageConfigs` フィールドは SerializedProperty で直接設定できない（private配列）。カスタム設定は実行時に `SetConfigs()` を呼ぶか、デフォルト設定を利用する
- メソッドをリファクタリングしてスコープを変えると、外部変数参照がなくなりコンパイルエラーになる

### 次回への改善提案
- `HandlePointer` のように、メソッドで必要な状態はパラメータで明示的に渡すパターンを最初から意識する
- SceneSetupでStageManagerのカスタム設定が必要な場合は `SetConfigs()` 呼び出しを RuntimeInitializationOnLoad や GameManager.Start() で行う
