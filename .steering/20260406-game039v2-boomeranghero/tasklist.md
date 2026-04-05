# タスクリスト: Game039v2_BoomerangHero

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（グラデーション・影・アウトライン付き）
  - background.png（action カテゴリ: 赤系）
  - player.png（プレイヤーキャラ）
  - boomerang.png（ブーメラン）
  - enemy_normal.png（通常敵）
  - enemy_shielded.png（盾持ち敵）
  - enemy_moving.png（移動敵）
  - wall.png（反射壁）
  - shield.png（盾エフェクト）
  - hit_effect.png（ヒットエフェクト）

## フェーズ2: C# スクリプト実装
- [x] BoomerangHeroGameManager.cs（StageManager・InstructionPanel統合）
- [x] BoomerangMechanic.cs（コアメカニクス・ブーメラン発射・反射・ヒット判定・5ステージ対応）
- [x] BoomerangHeroUI.cs（ステージ表示・スコア・残弾・残敵・各種パネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup039v2_BoomerangHero.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
