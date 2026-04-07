# タスクリスト: Game075v2_SoundGarden

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・植物3段階・害虫・土台）

## フェーズ2: C# スクリプト実装
- [x] Plant.cs（植物エンティティ、成長管理、ビジュアルフィードバック）
- [x] GardenController.cs（コアメカニクス・BPM・タップ判定・害虫管理・5ステージ対応）
- [x] SoundGardenGameManager.cs（StageManager・InstructionPanel統合）
- [x] SoundGardenUI.cs（ステージ・スコア・コンボ・タイマー・各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup075v2_SoundGarden.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: OumeiSatoKenta/cc_unity_maker#334（マージ済み）

### 計画と実績の差分
- 植物の動的生成（AddComponent）パターンにより`_spriteRenderer`がSerializeFieldでは機能せず、Awake()での自己取得に修正が必要だった
- StopCoroutine(string)では動的生成コルーチンを停止できないため、Dictionaryでルーチン参照を管理する実装に変更
- 成長計算のceil division問題（3 Perfect = 99 < 100）を発見・修正（34/22/11の固定値）

### 学んだこと
- AddComponentで生成したMonoBehaviourはSerializeFieldが機能しないため、Awake()での自己取得が必須
- StartCoroutine(methodRef)はstring名では停止できない → 参照をDictionaryで保持する設計が重要
- 整数除算の端数問題は「3回操作でゴール達成できるか」を検証する必要がある

### 次回への改善提案
- 動的生成コンポーネントを含む設計のレビューチェックリストにSerializeField/Awake確認を追加
