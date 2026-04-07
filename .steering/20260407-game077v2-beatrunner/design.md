# Design: Game077v2 BeatRunner

## namespace
`Game077v2_BeatRunner`

## スクリプト構成

### BeatRunnerGameManager.cs
- GameManager本体。StageManager・InstructionPanel統合
- ゲーム状態: Playing / StageClear / AllClear / GameOver
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] BeatManager _beatManager`
- `[SerializeField] BeatRunnerUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager.StartFromBeginning()
- OnStageChanged(int stage): BeatManagerにStageConfig渡す
- OnAllStagesCleared(): AllClearパネル表示
- OnStageClear(): StageClearパネル表示
- OnGameOver(): ゲームオーバーパネル表示

### BeatManager.cs  
- ビート生成・タイミング判定・障害物スポーン・ランナー制御を統合
- ビートシーケンスはステージ別に事前定義（障害物の種類とタイミング）
- `SetupStage(StageManager.StageConfig config, int stageIndex)`: ステージパラメータ適用
- 入力処理: `Mouse.current.leftButton.wasPressedThisFrame` で画面タップ検出
  - 上半分タップ → Jump
  - 下半分タップ → Slide
- ビートタイミング判定: 入力時刻とビートタイム差でPerfect/Great/Good/Miss判定
  - Perfect: ±0.08秒以内
  - Great: ±0.15秒以内
  - Good: ±0.25秒以内
  - Miss: それ以上 or 障害物に衝突
- 障害物タイプ: JumpObstacle（低い壁）、SlideObstacle（空中の棒）
- コンボ・スコア計算
- 速度管理: baseSpeed × speedMultiplier × beatBoost

### BeatRunnerUI.cs
- スコア・コンボ・ライフ・判定テキスト・進行バー表示
- StageClear/AllClear/GameOverパネル表示メソッド
- 判定テキストのフェードアウトアニメーション（Coroutine）
- コンボ数のスケールアニメーション

## 盤面・ステージデータ設計

ステージ設定（BeatManager内）:
```
Stage 0: BPM=100, beatInterval=0.6, patterns=[Jump×8], speedBase=3f
Stage 1: BPM=120, beatInterval=0.5, patterns=[Jump,Slide交互×10], speedBase=4f
Stage 2: BPM=140, beatInterval=0.43, patterns=[連続2パターン×12+Coin], speedBase=5f
Stage 3: BPM=160, beatInterval=0.375, patterns=[密度高め+Aerial×14], speedBase=6f
Stage 4: BPM=180, beatInterval=0.33, patterns=[複合+BossZone×16], speedBase=7f
```

各ステージ20ビートでStageClear（全5ステージ = 100ビート）

## 入力処理フロー
1. BeatManager.Update()でMouse.current監視
2. タップ検出 → 画面Y座標でJump/Slide判定
3. 現在ビートとの時間差でPerfect/Great/Good/Miss判定
4. 判定に応じてスコア加算・速度変化・コンボ更新

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
// ランナー固定位置: X=-camWidth*0.35, Y=-camSize*0.3（下部1/3の高さ）
// 地面: Y=-camSize*0.55
// ゲーム領域: Y=-camSize*0.8 ～ camSize*0.6
// Canvas UI下部マージン: 3.0ユニット確保
```

ランナーは画面左側1/3に固定（X固定）、コース（障害物・地面）が右から左へスクロール

## SceneSetup構成方針
- MenuItem: "Assets/Setup/077v2 BeatRunner"
- BeatManager子オブジェクト:
  - Runner（SpriteRenderer）
  - ObstaclePool（障害物プール親）
  - GroundScrollers（地面タイル群）
- StageManager: GameManagerの子
- InstructionPanel: 専用Canvas（sortOrder=100）

## StageManager統合
- OnStageChanged: BeatManager.SetupStage(config, stageIndex) でBPM・パターン・速度を更新
- OnAllStagesCleared: ゲームクリア処理
- ステージ別パラメータ表:
  | Stage | speedMultiplier | countMultiplier | complexityFactor |
  |-------|----------------|-----------------|-----------------|
  | 1     | 1.0f           | 1               | 0.0f            |
  | 2     | 1.2f           | 1               | 0.25f           |
  | 3     | 1.4f           | 1               | 0.5f            |
  | 4     | 1.6f           | 1               | 0.75f           |
  | 5     | 1.8f           | 1               | 1.0f            |

## InstructionPanel内容
- title: "BeatRunner"
- description: "ビートに乗って走り抜けろ！リズムゲームランナー"
- controls: "画面上半分タップでジャンプ、下半分タップでスライド。ビートに合わせてアクション！"
- goal: "楽曲終了まで走り切ろう。Perfect連続でスピードアップ！コンボでハイスコアを目指せ！"

## ビジュアルフィードバック設計
1. **判定テキストポップアップ**: "PERFECT!"/"GREAT!"/"GOOD"/"MISS" がランナー上部に表示され、0.8秒でフェードアウト
   - Perfect: 金色 + 1.0→1.4→1.0スケールパルス
   - Miss: 赤色 + カメラシェイク（0.2秒、振幅0.3）
2. **コンボアニメーション**: コンボ数更新時に1.0→1.3→1.0スケールパルス（0.15秒）
3. **障害物ヒット**: SpriteRenderer.color赤フラッシュ（0.1秒）
4. **速度ゲージ**: スライダーUIがリアルタイム更新

## スコアシステム
- Perfect: 100 × (1.0 + combo × 0.1) ※最大3.0倍
- Great: 60 × (1.0 + combo × 0.05) ※最大2.0倍
- Good: 20 × 1.0（倍率なし）
- Miss: 0、コンボリセット、ライフ-1

## ステージ別新ルール表
- Stage 1: ジャンプ障害物のみ（入力1種類）
- Stage 2: スライド障害物追加（2種類の使い分け必須）
- Stage 3: コイン収集要素（取らなくても進めるが高スコア狙いに必要）
- Stage 4: 空中障害物（ジャンプすると当たる）+BPM加速区間
- Stage 5: ボスゾーン（終盤8ビート連続アクション、失敗でライフ2消費）

## 判断ポイントの実装設計
- **障害物接近時**: 画面右端から3ユニット手前にビートマーカーライン表示 → プレイヤーはここでアクション
- **障害物種別判断**: JumpObstacle（Y=-1.5、高さ1.5）vsSlideObstacle（Y=0.5、空中）で視覚的に区別
- **コンボリスク**: speed = baseSpeed × (1.0 + combo × 0.03)、コンボ20で60%加速

## スプライト一覧
- runner_idle.png (64x64): ランナーキャラ（立ち）
- runner_jump.png (64x64): ジャンプ状態
- runner_slide.png (64x64): スライド状態
- obstacle_jump.png (48x96): ジャンプ障害物（縦長の壁）
- obstacle_slide.png (96x32): スライド障害物（横長の棒）
- coin.png (32x32): コイン
- ground_tile.png (128x32): 地面タイル
- background.png (512x512): 背景
- beat_marker.png (16x512): ビートマーカーライン（縦線）
