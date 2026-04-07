# タスクリスト: Game053v2_SlideBlitz

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Tile_Normal, Tile_Frozen, Tile_Blank）

## フェーズ2: C# スクリプト実装
- [x] TileObject.cs（タイルデータ・ビジュアルフィードバック）
- [x] SlideManager.cs（コアメカニクス・5ステージ難易度対応・レスポンシブ配置）
- [x] SlideBlitzGameManager.cs（StageManager・InstructionPanel統合）
- [x] SlideBlitzUI.cs（ステージ/タイマー/スコア/コンボ表示対応）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup053v2_SlideBlitz.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
- 実装完了日: 2026-04-06
- 計画と実績の差分: 特になし。5フェーズすべて計画通り完了
- 発生した主なバグと修正:
  - `AdvanceStage()` → `CompleteCurrentStage()` 名前修正
  - `CheckSolved()` でfrozenタイルが Mathf.Abs() なしで比較されておりステージ4がクリア不能だった
  - `ExecuteMove()` でfrozenタイルへの移動ガード漏れ
  - `SceneLoader` は static クラスで `FindFirstObjectByType<T>()` 不可 → `(UnityAction)SceneLoader.BackToMenu` で直接参照
  - Fisher-Yates shuffle導入（List.Sort のランダム比較は推移律違反）
- 次回への改善提案: SceneSetupの `AddMenuButtonListener()` パターンをテンプレート化して今後のSetupスクリプトに統一する
