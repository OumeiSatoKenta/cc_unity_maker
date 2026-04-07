# タスクリスト: Game080v2_FreqFight

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・敵・UI要素）

## フェーズ2: C# スクリプト実装
- [x] FreqFightGameManager.cs（StageManager・InstructionPanel統合）
- [x] FreqFightManager.cs（コアメカニクス・周波数判定・5ステージ難易度対応）
- [x] FreqFightUI.cs（ステージ表示・コンボ・判定テキスト・HP・ビートガイド）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup080v2_FreqFight.cs（InstructionPanel・StageManager・全UI配線含む）

## 実装後の振り返り

- 実装完了日: 2026-04-07
- PR: https://github.com/OumeiSatoKenta/cc_unity_maker/pull/338（mainマージ済み）

**計画と実績の差分:**
- `_sliderTrackRect2` の配線バグ（SliderTrack2オブジェクト作成後もSetFieldがs2RTを参照したまま）がコードレビューで検出され修正が必要だった
- Unity MCPがタイムアウトしたためSceneSetup実行・PlayMode検証はスキップ

**学んだこと:**
- デュアルエネミー構成では、スライダートラックの参照を慎重に管理する。SliderTrack1/2は独立オブジェクトとして作成し、SetField参照も各変数を明示的に使う
- `OnBeat()`でJudge呼び出し間に`_isActive`チェックが必要。同ビートで両敵が倒れる場合の二重OnStageClear防止

**次回への改善提案:**
- デュアルUI要素（スライダーx2、HPバーx2、マーカーx2）がある場合、SceneSetup実装時に変数命名を1/2で統一し、SetFieldセクションをペアでレビューする
