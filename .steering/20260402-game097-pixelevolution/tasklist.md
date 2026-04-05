# タスクリスト: Game097_PixelEvolution

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成

## フェーズ2: C# スクリプト実装
- [x] PixelEvolutionGameManager.cs
- [x] EvolutionManager.cs（コアメカニクス・進化パターン）
- [x] PixelEvolutionUI.cs

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup097_PixelEvolution.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
- 実装完了日: 2026-04-02
- ランタイム Texture2D + Sprite.Create によるピクセルアート描画を採用
- レビューで瞳描画バグ、Sprite リーク、未使用フィールドを指摘され即修正
- AddPersistentListener にラムダは使えないため、ラッパーメソッド（OnBranchSelectedA/B）パターンを確立
- 進化パスの組み合わせ（2^3=8パターン）によるリプレイ性を実現
