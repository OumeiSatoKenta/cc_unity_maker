# Design: Game046v2_SqueezePop

## namespace
`Game046v2_SqueezePop`

## スクリプト構成

### SqueezePopGameManager.cs
- GameManager全体を管理
- StageManager・InstructionPanel統合
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): stageManager.StartFromBeginning()
- OnStageChanged(int): BalloonManagerにSetupStage()を渡す
- OnAllStagesCleared(): 全ステージクリア演出

### BalloonManager.cs
- 風船の生成・配置・入力処理を一元管理
- **入力処理**: Mouse.current.leftButton / PointerDown・PointerUp
- BallooniItem: 各風船の状態（Normal/Bomb/Shield/Moving）、膨張率
- SetupStage(StageManager.StageConfig, int stageIndex): ステージ別パラメータ適用
- 押下中: 膨張アニメーション（scale増加）
- リリース: サイズ判定 → Perfect/Normal/Fail/Explode
- 連鎖判定: 直近ポップから1秒以内の隣接タップ
- 爆弾バブル: MAX超えで周囲1マスを破壊
- シールドバブル: 2回目のポップで本ポップ

### SqueezePopUI.cs
- HUD表示: タイマー、スコア、残りターゲット数、コンボ表示
- StageClearPanel、GameOverPanel、AllClearPanel
- UpdateHUD(float timeLeft, int score, int combo, int remaining)
- ShowStageClear()、ShowGameOver(int score)、ShowAllClear(int score)

## 入力処理フロー
```
PointerDown on Balloon → _pressedBalloon = balloon, _pressStartTime = Time.time
Update loop → if _pressedBalloon != null: balloon.Inflate(Time.deltaTime * inflateSpeed)
PointerUp → 判定:
  - ratio < minRatio → Fail (萎む)
  - minRatio <= ratio <= maxRatio → Perfect Pop (300pt)
  - maxRatio < ratio < explodeRatio → Normal Pop (100pt)
  - ratio >= explodeRatio → Explode (ペナルティ)
```

## ステージ別パラメータ
| Stage | Count | TimeLimit | inflateSpeed | explodeTime | BombCount | HasMoving |
|-------|-------|-----------|--------------|-------------|-----------|-----------|
| 1     | 8     | 40s       | 0.7          | 1.5s        | 0         | false     |
| 2     | 12    | 40s       | 0.8          | 1.3s        | 0         | false     |
| 3     | 16    | 35s       | 0.9          | 1.2s        | 0         | false     |
| 4     | 20    | 35s       | 1.0          | 1.1s        | 3         | false     |
| 5     | 25    | 30s       | 1.1          | 1.0s        | 3         | true      |

## InstructionPanelの4テキスト
- title: "SqueezePop"
- description: "風船を長押しで膨らませてポップさせよう"
- controls: "長押しで膨らませ、指を離してポップ！膨らませすぎると破裂！"
- goal: "全ての風船をポップさせてステージクリア！"

## ビジュアルフィードバック設計
1. **パーフェクトポップ**: スケールパルス (1.0→1.5→0.0 Destroy、0.25秒) + "PERFECT!" テキスト浮き上がり
2. **爆弾爆発**: 赤フラッシュ + 周囲の風船もスケールパルスで消滅 + カメラシェイク
3. **コンボ**: コンボ数テキストがスケールアップで強調表示 (×2, ×3...)
4. **ノーマルポップ**: 膨らんで消滅 (scale→1.3→0)

## スコアシステム
- 通常ポップ: 100点
- パーフェクトポップ: 300点
- 連鎖ボーナス: combo >= 4 → ×2.0、combo 2-3 → ×1.5
- 残り時間ボーナス: 残秒×50点
- 全パーフェクトクリア: 最終スコア×3

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;    // HUD用
float bottomMargin = 2.8f; // UIボタン用
float availableHeight = camSize * 2f - topMargin - bottomMargin;
// グリッド配置: 最大4列×最大7行
int cols = 4;
float cellSize = Mathf.Min(availableHeight / rows, camWidth * 2f / cols) * 0.85f;
```

## SceneSetup構成方針
- MenuItem: "Assets/Setup/046v2 SqueezePop"
- StageManager: GameManagerの子として生成
- InstructionPanel: Canvas上のフルスクリーンオーバーレイ
- BalloonManager: GameManagerの子
- ゲームエリア: 中央（y: -0.5〜3.5 相当）
- ボタン最小サイズ: (150, 55)
- 下部UIマージン: 2.8ユニット確保

## StageManager統合
- OnStageChanged → BalloonManager.SetupStage(config, stageIndex)
- OnAllStagesCleared → ShowAllClear演出
- ステージカスタム設定はSetCustomConfigs()で上書き
