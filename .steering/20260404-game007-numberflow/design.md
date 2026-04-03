# Design: Game007v2 NumberFlow

## Namespace
`Game007v2_NumberFlow`

## スクリプト構成

### NumberFlowGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear）
- SerializeField: `StageManager _stageManager`, `InstructionPanel _instructionPanel`, `NumberFlowManager _flowManager`, `NumberFlowUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): stageManager.OnStageChanged/OnAllStagesCleared 購読 → StartFromBeginning()
- OnStageChanged(int stage): flowManager.SetupStage() + ui.UpdateStage()
- OnAllStagesCleared(): GameClearパネル表示
- OnStageClear(int score, int stars): スコア集計・StageClearパネル表示
- コンボカウンター管理

### NumberFlowManager.cs
- コアメカニクス: グリッド生成・経路管理・特殊マス処理
- SetupStage(StageManager.StageConfig config, int stageIndex): ステージ別パラメータ設定
- グリッドサイズ: Stage1=4x4, 2-3=5x5, 4-5=6x6
- 特殊マス: StageConfig.speedMultiplierでwallCountを、complexityFactorでspecialTypeを判定
- マス種別: Normal, Wall, WarpA, WarpB, DirectionLimited(Up/Down/Left/Right)
- 入力処理: Mouse.current / Touchscreen.current で現在位置を取得、Physics2D.OverlapPoint で対象セル判定
- 経路追跡: `List<Vector2Int> _path` でタップ/スワイプ順にセルを積む
- 最後のセルをタップ→1手戻す
- ハイライト: 選択済みセルはラインRenderer で繋ぐ
- 全マス埋め判定→GameManager.OnStageClear() コール
- レスポンシブ配置:
  ```
  float camSize = Camera.main.orthographicSize; // 5
  float camWidth = camSize * Camera.main.aspect;
  float topMargin = 1.2f;
  float bottomMargin = 2.8f;
  float availableH = camSize * 2f - topMargin - bottomMargin;
  float cellSize = Mathf.Min(availableH / gridN, camWidth * 2f / gridN, 1.8f);
  ```
- ビジュアルフィードバック:
  - マス選択時: スケールパルス 1.0→1.25→1.0 (0.1秒)
  - クリア時: 全セルに緑フラッシュ (SpriteRenderer.color → 緑→白 0.3秒)
  - 間違いリセット時: カメラシェイク (0.1秒, 振幅0.1)
- スコア計算:
  - baseScore = gridN * gridN * 100 * stageIndex
  - 取消0回ボーナス: ×2
  - タイムボーナス: targetTime以内でさらに×1.5
  - コンボ: ×(1 + (combo-1)*0.2)

### NumberFlowUI.cs
- UpdateStage(int current, int total)
- UpdateScore(int score)
- UpdateCombo(int combo)
- UpdateProgress(int filled, int total)
- UpdateTimer(float elapsed)
- ShowStageClearPanel(bool show, int score = 0, int stars = 0)
- ShowGameClearPanel(int totalScore)

## SceneSetup: Setup007v2_NumberFlow.cs
- MenuItem: `Assets/Setup/007v2 NumberFlow`
- スプライト出力先: `Assets/Resources/Sprites/Game007v2_NumberFlow/`
- 必要スプライト:
  - Background.png (960x1920)
  - CellNormal.png (128x128) — 青系グラデーション
  - CellWall.png (128x128) — グレー系
  - CellWarpA.png (128x128) — 紫系
  - CellWarpB.png (128x128) — 橙系
  - CellDirection.png (128x128) — シアン系、矢印付き
  - NumberBg.png (128x128) — ヒント数字の背景
- GameManager(NumberFlowGameManager), StageManager, NumberFlowManager を生成・配線
- StageManager._totalStages = 5
- Canvas (ScreenSpaceOverlay, sortOrder=10, 1080x1920)
- HUD配置（上部）:
  - StageText (top-left)
  - ScoreText (top-right)
  - ComboText (top-center)
  - ProgressText (top-center-left)
  - TimerText (top-center-right)
- ボタン（下部）:
  - ResetButton (bottom-left, Y=65)
  - BackToMenuButton (bottom-right, Y=20)
- InstructionPanel (フルスクリーンオーバーレイ, Canvas sortOrder=100)
- StageClearPanel: 中央, 「ステージクリア！」テキスト + 「次のステージへ」ボタン + スコア・星表示
- GameClearPanel: 中央, 「全ステージクリア！」+ 合計スコア

## StageManager統合
- StageConfig パラメータの利用:
  - `speedMultiplier` → wallCount (0=wall0, 1.0=wall0, 1.5=wall2, 2.0=wall2)
  - `countMultiplier` → hintCount (2.0=5, 1.5=3, 1.0=2)
  - `complexityFactor` → specialType (0=none, 1=warp, 2=direction)

## ステージ別パラメータ表
| Stage | speedMultiplier | countMultiplier | complexityFactor | GridSize | HintCount | SpecialType |
|-------|----------------|----------------|-----------------|----------|-----------|-------------|
| 1 | 1.0 | 2.0 | 0.0 | 4x4 | 5 | None |
| 2 | 1.0 | 1.5 | 0.0 | 5x5 | 3 | None |
| 3 | 1.5 | 1.0 | 0.0 | 5x5 | 2 | Wall |
| 4 | 2.0 | 1.5 | 1.0 | 6x6 | 3 | Warp |
| 5 | 2.0 | 1.0 | 2.0 | 6x6 | 2 | Direction |

## ビジュアルフィードバック詳細
1. **マス選択アニメ**: DOTween使用せず、Coroutineでscale変化 1.0→1.25→1.0 (0.1秒 per half)
2. **クリアフラッシュ**: 全セルのSpriteRenderer.colorを0.05秒ごとに緑→白→緑×3回
3. **カメラシェイク**: Time.deltaTimeで0.1秒間、Camera位置をRandom.insideUnitCircle*0.1でオフセット

## 判断ポイントの実装設計
- プレイヤーがフォークポイント（3方向以上に進める）に達したとき → セル背景色を微妙に変えてヒント（高スコア狙いでは使わない）
- ワープマス到達時: 移動先をパルスアニメで示す
- 方向制限マス: 矢印アイコンで視覚的に制限方向を表示
