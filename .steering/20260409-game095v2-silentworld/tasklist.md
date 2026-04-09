# タスクリスト: Game095v2_SilentWorld

## フェーズ1: スプライトアセット生成

- [x] Python + Pillow で高品質スプライト画像を生成（グラデーション・影・アウトライン付き）
  - floor.png（床セル: 深紺グラデーション）
  - wall.png（壁セル: ネオングリーンアウトライン）
  - trap.png（トラップセル: 暗い外観、偽装）
  - exit.png（出口セル: ネオン紫グロー）
  - item.png（音符アイテム: ネオン緑輝き）
  - character.png（プレイヤー: 円形ネオン青）
  - background.png（背景: 深黒グラデーション）
  - hint_glow.png（ヒントグロー: 青白い半透明円）

## フェーズ2: C# スクリプト実装

- [x] SilentWorldGameManager.cs（StageManager・InstructionPanel統合・スコア・コンボ）
- [x] WorldManager.cs（グリッド生成・キャラ移動・トラップ判定・視覚ヒント・長押し観察）
- [x] SilentWorldUI.cs（HUD・ライフ・ヒント残数・タイマー・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト

- [x] Setup095v2_SilentWorld.cs（InstructionPanel・StageManager配線・グリッドプレビュー・全フィールド配線）

## 実装後の振り返り

（実装完了後に記入）
