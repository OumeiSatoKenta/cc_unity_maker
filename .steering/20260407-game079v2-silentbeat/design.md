# Design: Game079v2_SilentBeat

## namespace
`Game079v2_SilentBeat`

## スクリプト構成

### SilentBeatGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] RhythmManager _rhythmManager
- [SerializeField] SilentBeatUI _ui
- Start(): _instructionPanel.Show() → OnDismissed += StartGame
- StartGame(): _stageManager.OnStageChanged += OnStageChanged, StartFromBeginning()
- OnStageChanged(int stage): RhythmManager.SetupStage(config, stageIndex)
- OnAllStagesCleared(): 最終クリア処理
- OnStageClear(), NextStage(), OnGameOver() など

### RhythmManager.cs
- コアメカニクス: BPM管理、ガイド拍再生、タイミング判定
- SetupStage(StageManager.StageConfig config, int stageIndex)
  - stageIndex 0: BPM=60, guideTaps=8, hasVisualPulse=true, tapCount=20
  - stageIndex 1: BPM=90, guideTaps=4, hasVisualPulse=false, tapCount=25
  - stageIndex 2: BPM=120, guideTaps=4, hasBpmChange=true, tapCount=30
  - stageIndex 3: BPM=80→120波形, guideTaps=4, tapCount=40
  - stageIndex 4: BPM=150, guideTaps=2, hasRandomChange=true, tapCount=50
- ガイドフェーズ: coroutine でガイド拍を視覚/音でカウント
- 無音フェーズ: プレイヤーのタップ間隔を計測
- JudgeTap(): 前回タップからの経過時間 vs 期待間隔で Perfect/Great/Good/Miss 判定
- コンボ管理: consecutiveMiss, comboCount
- 入力: Mouse.current.leftButton.wasPressedThisFrame + Touchscreen

### SilentBeatUI.cs
- UpdateStage, UpdateScore, UpdateCombo, ShowJudgement
- ShowStageClear, HideStageClear, ShowAllClear, ShowGameOver
- UpdateProgress(current, total), UpdateBpm(bpm), ShowGuidePhase, HideGuidePhase
- UpdateAccuracyIndicator(float deviation) 精度インジケーター更新

## ビジュアルフィードバック設計
1. **タップパルス**: タップエリアが一瞬スケールアップ（1.0→1.2→1.0、0.15秒）
2. **判定色フラッシュ**: Perfect=金色、Great=緑、Good=水色、Miss=赤のタップエリア色変化
3. **精度インジケーター**: タップのズレ方向（早い/遅い）を小さなバーで表示
4. **コンボ時**: スケール + 色変化（黄金色）の複合演出

## InstructionPanel 内容
- title: "SilentBeat"
- description: "無音の画面でリズムを感じ取り正確なタイミングでタップ"
- controls: "ガイド拍を聞いて覚え、無音になったら同じリズムでタップし続けよう！Perfect判定でコンボが繋がり高得点！"
- goal: "5ステージのリズムをマスターして完全内部時計を目指せ！"

## StageManager統合
- OnStageChanged購読でRhythmManager.SetupStage()を呼び出し
- speedMultiplier を BPM 計算の乗算係数として活用
- complexityFactor を BPM変化の頻度/量として活用

## ステージ別パラメータ
| Stage | BPM | guide | visual | tapCount | 新要素 |
|-------|-----|-------|--------|----------|--------|
| 1 | 60 | 8 | あり | 20 | 基本のみ |
| 2 | 90 | 4 | なし | 25 | 視覚ガイド除去 |
| 3 | 120 | 4 | なし | 30 | BPM変化 |
| 4 | 80→120 | 4 | なし | 40 | 波形BPM変化 |
| 5 | 150 | 2 | なし | 50 | ランダム変速 |

## スコアシステム
- Perfect(±20ms): 150pt × コンボ倍率（最大x4.0）
- Great(±50ms): 80pt × コンボ倍率（最大x2.5）
- Good(±100ms): 30pt
- Miss: 0pt、コンボリセット
- 全Perfect完了ボーナス: x3.0

## SceneSetup Setup079v2_SilentBeat.cs
- [MenuItem("Assets/Setup/079v2 SilentBeat")]
- カメラ: backgroundColor ダーク（#050510）、orthographicSize=6
- スプライト: Assets/Resources/Sprites/Game079v2_SilentBeat/
- GameManager (SilentBeatGameManager) + StageManager + RhythmManager
- Canvas + HUD (上部: StageText, ScoreText) + 下部UI
- タップエリア: 画面中央の大きなパネル（インタラクティブ）
- InstructionPanel 配線
- StageClearPanel, AllClearPanel, GameOverPanel
- 配線: gm._stageManager, gm._rhythmManager, gm._ui
- AddSceneToBuildSettings

## レスポンシブ配置
- ゲーム要素はすべてUI Canvas上で管理（タップエリアはCanvas Panel）
- タップエリア: 画面中央、高さ方向の中間帯（上部HUD + 下部ボタン除く領域）
- HUD上部: Y=-30〜-80（ステージ・スコア）
- タップエリア: 中央（Y=0近辺）、sizeDelta = (800, 700)程度
- 下部ボタン: Y=10〜80（メニューボタン）

## BPMズレ判定実装
```csharp
float expectedInterval = 60f / currentBpm;  // 秒
float actualInterval = Time.time - _lastTapTime;
float deviation = Mathf.Abs(actualInterval - expectedInterval);
if (deviation < 0.020f) { /* Perfect */ }
else if (deviation < 0.050f) { /* Great */ }
else if (deviation < 0.100f) { /* Good */ }
else { /* Miss */ }
```
