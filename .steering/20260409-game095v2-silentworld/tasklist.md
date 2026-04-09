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

**実装完了日**: 2026-04-09

**計画と実績の差分**:
- Unity MCPセッションがタイムアウトし続けたため、ステップ5-3（コンパイル検証）・5-4（SceneSetup実行）・5-5（PlayMode検証）をスキップした。GameRegistry.jsonの `implemented: true` 更新も未実施。
- コードレビューで[必須]6件を検出・全修正:（1）コルーチン多重発火防止、（2）タイマー切れとTrapHit競合、（3）Camera.mainキャッシュ、（4）ClearGridの安全ループ、（5）ボタン2重登録解消、（6）characterGo OnDestroyクリーンアップ

**学んだこと**:
- `SetActive(false)` の中に `StopAllCoroutines()` を入れることで、ステージ遷移中のコルーチン競合を防げる
- タイマー切れとダメージコルーチンの競合防止には、コルーチン末尾で `_isActive && _gameManager.IsPlaying` の二重チェックが有効
- `Camera.main` は `Awake()` でキャッシュして `_mainCamera` として使うべき（毎フレームの `FindObjectWithTag` 回避）
- グリッド配列のループは `GetLength(0)` / `GetLength(1)` で安全にサイズを取得すべき

**次回への改善提案**:
- 長押し観察のUXをさらに改善できる（観察エリアを視覚的に示すアニメーションを追加）

### 事後検証（2026-04-09）
- ✅ コンパイル: エラーなし（0件）
- ✅ SceneSetup: Assets/Setup/095v2 SilentWorld 実行成功
- ✅ PlayMode: ランタイムエラーなし、星空背景・スプライト表示確認済み
- ✅ GameRegistry.json: implemented: true に更新完了
