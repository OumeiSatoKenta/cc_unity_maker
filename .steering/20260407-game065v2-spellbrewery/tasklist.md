# タスクリスト: Game065v2_SpellBrewery

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（背景・材料5種・ポーション8種・醸造釜・エフェクト）

## フェーズ2: C# スクリプト実装
- [x] SpellBreweryGameManager.cs（StageManager・InstructionPanel統合）
- [x] BreweryManager.cs（材料・レシピ・醸造・販売・注文ロジック、5ステージ対応）
- [x] SpellBreweryUI.cs（ステージ表示・コンボ表示・注文パネル対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup065v2_SpellBrewery.cs（InstructionPanel・StageManager配線含む、全フィールド配線）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: #326（マージ済み）
- 計画との差分: ほぼ計画通り。MCP未接続のためSceneSetup/PlayMode検証はスキップ
- 学んだこと:
  - ビットマスクレシピシステムは8種類のポーションを辞書1つで管理でき拡張性が高い
  - Random.Range(0, max) の最大値を含まない仕様に注意（OrderLoopのバグ修正で検出）
  - アイドル系コルーチンチェーン（CameraShake等）はOnDestroyでStopAllCoroutinesするのが安全
- 次回への改善提案:
  - StageConfigにステージ固有パラメータを持たせてBreweryManagerのswitchを排除できる
