# タスクリスト: Game003_GravitySwitch

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, ball, goal, wall, hole, sliding_floor）

## フェーズ2: C# スクリプト実装
- [x] GravitySwitchGameManager.cs（StageManager・InstructionPanel統合、スコア管理）
- [x] GravityManager.cs（盤面管理・ボール移動・重力切替・5ステージ難易度対応）
- [x] GravitySwitchUI.cs（ステージ表示・スコア・手数・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup003_GravitySwitch.cs（InstructionPanel・StageManager・4方向ボタン配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

**実装完了日**: 2026-04-03

**計画と実績の差分**:
- 既存v1実装（StageManager/InstructionPanel未対応）を完全書き直しとなった
- `sliding_floor` の実装はStage4で盤面データのみで実際の動くロジックは省略（壁と同様に扱う）

**学んだこと**:
- コルーチン多重起動: `yield return StartCoroutine(...)` で直列化しないと二重呼び出しが発生する
- `GravityDirection` enum を GameManager ファイルに定義することで UI との参照がクリーンになる
- ボタンイベント二重登録: Setup の Persistent Listener と Awake の AddListener は役割分担を明確にする

**次回への改善提案**:
- 方向ボタンは enum 引数が必要なため Awake での AddListener が必要。Setup では登録不要と明示してドキュメントに記載する
- `OnMovesChanged` イベントパターンは再利用できる良いパターン
