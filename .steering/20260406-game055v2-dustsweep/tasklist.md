# タスクリスト: Game055v2_DustSweep

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, DustTexture, HiddenItem1〜5, BrushIcon等）

## フェーズ2: C# スクリプト実装
- [x] DustSweepGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] DustBoard.cs（Texture2Dベースのダストマップ・スワイプ処理・5ステージ要素）
- [x] DustSweepUI.cs（HUD・タイマー・清潔度バー・ブラシ切替・クリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup055v2_DustSweep.cs（InstructionPanel・StageManager配線・DustBoard・全UI構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
- 実装完了日: 2026-04-06
- スクリプト・SceneSetup・スプライトは既に存在していたため、コードレビュー後に必須修正のみ適用
- 修正内容:
  1. OnStageChanged の 0-based→1-based 変換漏れを修正
  2. DangerZone ペナルティ「-5秒」の表示と実際のタイマー減算処理を一致させるため SubtractTime(5f) を追加
  3. PlaceHiddenItems での _totalDust 二重カウントバグを修正（既存ホコリのある位置をスキップ）
  4. DustSweepUI.Awake のボタン二重リスナー登録を除去（SceneSetup の AddPersistentListener に統一）
- 次回への改善提案: UpdateTextures のフルピクセル再走査はパフォーマンスに影響するため、ダーティフラグ方式への改善を検討
