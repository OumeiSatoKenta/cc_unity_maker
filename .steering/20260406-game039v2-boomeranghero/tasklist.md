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

**実装完了日**: 2026-04-06

**計画と実績の差分**:
- 計画通りに4フェーズを完了。コードレビュー指摘の[必須]5件をすべて修正。
- `StopAllCoroutines()`をコルーチン個別停止に変更、CheckGameOver()の状態チェック追加、SetupStage()でのコルーチン停止、Camera.mainのnullガード、DefeatAnimationの安全チェックを実装。

**学んだこと**:
- コルーチン管理はフィールドに`Coroutine`型を保持して個別停止するのが安全。`StopAllCoroutines()`は他のコルーチン（FlashObjectなど）も止めてしまう。
- ステージ切り替え時に前のコルーチンを必ず停止しないと古いコルーチンが新ステージに干渉する。
- `_gameManager.State`チェックをコールバック側でも行う二重防御が有効。

**次回への改善提案**:
- コルーチン管理パターンをテンプレートに明記しておくと実装時に忘れない。
- EnemyControllerのような動的生成コンポーネントはyield後のnullチェックをデフォルトパターン化する。
