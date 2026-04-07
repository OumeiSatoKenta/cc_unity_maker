# タスクリスト: Game082v2_AquaPet

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（水槽背景・魚スプライト各種・餌スプライト）

## フェーズ2: C# スクリプト実装
- [x] AquaPetGameManager.cs（StageManager・InstructionPanel統合）
- [x] AquariumManager.cs（コアメカニクス・5ステージ難易度対応・魚管理）
- [x] AquaPetUI.cs（ステージ・スコア・水質・健康度・図鑑進捗表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup082v2_AquaPet.cs（InstructionPanel・StageManager・全UI配線含む）

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: OumeiSatoKenta/cc_unity_maker#340（マージ済み）

### 計画と実績の差分
- 計画通り4フェーズ（スプライト・スクリプト3本・SceneSetup）で完了
- コードレビューで4件の [必須] 指摘が発覚し、AquariumManager.cs と Setup082v2_AquaPet.cs を追加修正

### 主要な修正点
1. `camera.tag = "MainCamera"` 追加 → Camera.main が null になる問題を修正
2. `SetupStage()` / `SetActive(false)` で `StopAllCoroutines()` → コルーチン積み上がり防止
3. `PopAnimation()` に null ガード追加 → 魚オブジェクト破棄後の参照エラー防止
4. `GetAverageHealth()` を死魚除外ロジックに修正 → 健康バーが不正に低く表示される問題を修正

### 学んだこと
- シミュレーション系は魚オブジェクトのライフサイクル管理（生成・繁殖・死亡）が複雑なため、null ガードを徹底する必要がある
- `StopAllCoroutines()` はステージ遷移時の必須パターン（特に長い WaitForSeconds を使うコルーチン）

### 次回への改善提案
- シミュレーション系ゲームでは FishData のような内部クラスに null 安全な `IsAlive` プロパティを定義するとコードが簡潔になる
