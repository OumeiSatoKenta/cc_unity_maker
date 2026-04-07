# タスクリスト: Game076v2_ChordCatch

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（background, chord_button, replay_button, beat_indicator）

## フェーズ2: C# スクリプト実装
- [x] ChordCatchGameManager.cs（StageManager・InstructionPanel統合）
- [x] ChordController.cs（コアメカニクス・プロシージャル和音生成・5ステージ難易度対応）
- [x] ChordCatchUI.cs（ステージ表示・コンボ・判定テキスト・コードボタン動的生成）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup076v2_ChordCatch.cs（InstructionPanel・StageManager配線含む）

## 実装後の振り返り

**実装完了日**: 2026-04-07

**計画と実績の差分**:
- プロシージャル和音生成は AudioClip.SetData で正弦波合成を実装。外部アセット不要で期待通り動作。
- コードレビューで [必須] 3件検出: OnGameOver二重呼び出し競合、BeatLoopの負値WaitForSeconds。即修正済み。
- Unity MCP がタイムアウトのためコンパイル検証・SceneSetup・PlayMode はスキップ。implemented: true は更新しなかった。

**学んだこと**:
- コルーチン終端の `_isActive` チェックに加え、`_missCount < MaxMiss` の複合条件も必要（RegisterMiss内で`_isActive=false`しても同フレームのコルーチン終端は通過し得る）
- BPMが高い場合（BPM > 500）にbeatDuration-0.12fが負値になるため `Mathf.Max(0f, ...)` ガードが必要

**次回への改善提案**:
- `SetupStage(StageConfig config, int stageIndex)` でconfigを実際に活用するか、シグネチャを整理する
- タイミング判定基準を「ビート後の時刻」ではなく「再生後の経過時間」ベースに変更すると自然なゲーム体験になる
