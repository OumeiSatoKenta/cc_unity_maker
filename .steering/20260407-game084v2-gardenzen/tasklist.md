# タスクリスト: Game084v2 GardenZen

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（グラデーション・影・アウトライン付き）

## フェーズ2: C# スクリプト実装
- [x] GardenZenGameManager.cs（StageManager・InstructionPanel統合）
- [x] GardenManager.cs（コアメカニクス・グリッド配置・砂紋・スコア計算・5ステージ難易度対応）
- [x] GardenZenUI.cs（ステージ表示・一致度・パレット・提出ボタン対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup084v2_GardenZen.cs（InstructionPanel・StageManager配線含む）

## 実装後の振り返り

**実装完了日**: 2026-04-07

**計画と実績の差分**:
- 計画通り4フェーズ（スプライト・GameManager・GardenManager・SceneSetup）を完了
- GardenManager.cs が最も複雑で、複数のバグ修正が発生（スコア二重加算・タッチ入力不足・null安全性）

**修正対応したバグ**:
1. `StageManager.SetConfigs()` API を使うべき箇所で誤って `SetField` でプライベートフィールドに直接書き込もうとしていた（サイレント失敗）
2. `SubmitGarden()` でリトライ時もスコアが加算される二重加算バグ
3. タッチ入力（`Touchscreen.current`）のフォールバックが未実装でモバイル非対応
4. `Camera.main` への null チェック漏れ
5. コルーチン内で対象 Transform/SpriteRenderer が null の場合の例外
6. `GardenZenUI.ShowStageClear` で stars > 3 のとき `ArgumentOutOfRangeException` が発生する可能性

**学んだこと**:
- `StageManager` の API は `SetConfigs()` が正規ルート。`SetField` はフィールド名が合わない場合サイレントに警告のみ出して続行するため気づきにくい
- コルーチンは `yield return` の前後で毎フレーム参照が切れうる。`if (t == null) yield break` ガードは必須

**次回への改善提案**:
- SceneSetup でStageManager を配線する際は `sm.SetConfigs()` を必ず使い、`SetField("_stages", ...)` パターンを避けること
- コルーチンの冒頭で null チェックをパターン化したテンプレートを用意すると良い
