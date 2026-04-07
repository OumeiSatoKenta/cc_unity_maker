# タスクリスト: Game077v2_BeatRunner

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成

## フェーズ2: C# スクリプト実装
- [x] BeatRunnerGameManager.cs（StageManager・InstructionPanel統合）
- [x] BeatManager.cs（ビート管理・入力処理・障害物生成・5ステージ難易度対応）
- [x] BeatRunnerUI.cs（ステージ表示・コンボ・ライフ・判定テキスト・クリアパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup077v2_BeatRunner.cs（InstructionPanel・StageManager配線含む）

## 実装後の振り返り

- 実装完了日: 2026-04-07
- **計画と実績の差分**: BeatManagerの`_beatsSpawned`カウンターを設計段階で明示していなかったため、コードレビューで`[必須]`指摘を受けた。スポーン制限と完了カウンターを別管理することは当初から必要だったが設計書から漏れていた。
- **学んだこと**: リズムゲームでは「スポーン済み数」「完了数（判定済み）」「ライフ残数」の3カウンターを明確に分離する必要がある。`SetActive(false)`時に`StopAllCoroutines()`を呼ぶことでコルーチンリークを防ぐパターンが有効。
- **次回への改善提案**: 設計書の段階でカウンター変数を列挙し、それぞれが「何を数えるか」を明記する。コルーチン管理（`Coroutine`フィールド保持 vs `StopAllCoroutines`）の方針を設計書に含める。Unity MCP タイムアウト時は`implemented: true`を設定しないルールを遵守（今回適用済み）。
