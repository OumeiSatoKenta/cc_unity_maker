# タスクリスト: Game064v2_AquaCity

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（海底背景、建物6種、魚3種、サメ）

## フェーズ2: C# スクリプト実装
- [x] AquaCityGameManager.cs（StageManager・InstructionPanel統合）
- [x] CityManager.cs（3x3グリッド・建物管理・5ステージ難易度・隣接ボーナス・サメイベント）
- [x] AquaCityUI.cs（人口・コイン・ショップ・ステージ表示・コンボ・警告）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup064v2_AquaCity.cs（InstructionPanel・StageManager・3x3グリッド・全配線）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: OumeiSatoKenta/cc_unity_maker#324（マージ済み）

### 計画と実績の差分
- SceneSetup・コンパイル・PlayMode検証はMCP未接続のためスキップ
- AquaCityUIの`_shopPanel`フィールド削除、CityManager.BuildingCostをpublic staticにするバグ修正が必要だった

### 学んだこと
- 複数クラス間で共有する定数は最初から`public static readonly`で定義すべき
- SceneSetupで配線するフィールドとUI.Start()で配線するボタンが重複しないよう設計段階で明確化が必要
- シャークコルーチン参照は`_sharkCoroutine = StartCoroutine(...)`で保持しないとStopCoroutineできない

### 次回への改善提案
- コンボ処理のUpdate()ガードを設計段階で考慮（全Update禁止 vs 常時実行の分類）
- カメラサイズ参照をSetupStage内でキャプチャするパターンを設計書に明記
