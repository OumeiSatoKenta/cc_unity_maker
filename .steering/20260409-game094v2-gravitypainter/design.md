# Design: Game094v2_GravityPainter

## namespace
`Game094v2_GravityPainter`

## スクリプト構成

### GravityPainterGameManager.cs
- ゲーム状態管理 (Playing / StageClear / AllClear / GameOver)
- StageManager・InstructionPanel を SerializeField で持つ
- スコア・コンボ管理
- PaintManager への SetupStage() 呼び出し
- OnStageChanged / OnAllStagesCleared 購読

### PaintManager.cs
- キャンバス（グリッド）のデータモデル管理
- セルタイプ: Normal / Wall / Absorb / Paint(color) / Target(color)
- 重力適用ロジック: 全Paintセルを指定方向に移動（壁でストップ、Absorbで消去、混合で色変化）
- お手本データ生成（ステージごとにハードコード）
- 一致率計算
- 投下ロジック: タップ位置のセルに選択色の絵の具を配置
- `SetupStage(StageManager.StageConfig config, int stageIndex)` でステージリセット
- ビジュアルフィードバック: 正解セルのスケールパルス、ミス時の赤フラッシュ
- GameManager へのコールバック: OnCellPainted, OnMatchRateChanged, OnGameOver

### GravityPainterUI.cs
- StageText, ScoreText, MatchRateText, PaintCountText, ComboText の更新
- StageClearPanel / AllClearPanel / GameOverPanel の表示制御
- ColorPalette ボタンのハイライト切替
- GravityButtons（上下左右）のアニメーション

## 盤面・ステージデータ設計

グリッドはセル二次元配列で管理:
```csharp
enum CellType { Empty, Wall, Absorb, Paint, Target }
struct Cell { CellType type; int colorIndex; }  // colorIndex: 0=empty, 1..N=色
```

お手本データ（targetGrid）は ints[gridSize, gridSize] でハードコード。

### ステージパラメータ
```
Stage 0 (index): gridSize=6, colorCount=1, paintBudget=8, hasWalls=false, hasAbsorb=false, hasMix=false, timeLimit=0
Stage 1:         gridSize=6, colorCount=2, paintBudget=10, hasWalls=false, hasAbsorb=false, hasMix=false, timeLimit=0
Stage 2:         gridSize=7, colorCount=3, paintBudget=10, hasWalls=true,  hasAbsorb=false, hasMix=false, timeLimit=0
Stage 3:         gridSize=7, colorCount=3, paintBudget=9,  hasWalls=true,  hasAbsorb=true,  hasMix=false, timeLimit=0
Stage 4:         gridSize=8, colorCount=4, paintBudget=10, hasWalls=false, hasAbsorb=false, hasMix=true,  timeLimit=60
```

StageManager の config を活用:
- `speedMultiplier` → timeLimit の係数（Stage5は1.0, 他は0=無制限）
- `countMultiplier` → paintBudget の乗数補正
- `complexityFactor` → 追加要素フラグの判定 (0=基本, 0.5=壁, 1.0=混合等)

## 入力処理フロー
1. GravityButtons → `PaintManager.ApplyGravity(direction)` を直接呼ぶ（UI経由）
2. キャンバスタップ → `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint` でセル判定
3. パレットボタン → `PaintManager.SelectColor(int index)` 呼び出し

using `UnityEngine.InputSystem;`

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;   // HUD領域
float bottomMargin = 3.5f; // ボタン領域（重力ボタン + パレット + メニュー）
float availableHeight = camSize * 2f - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.2f);
```

グリッド中心は (0, (availableHeight/2 + bottomMargin/2 - topMargin/2) の Y オフセット) で配置。

## SceneSetup 構成方針
- MenuPath: `Assets/Setup/094v2 GravityPainter`
- ファイル: `Setup094v2_GravityPainter.cs`
- 配線:
  - gm._stageManager = sm
  - gm._instructionPanel = ip
  - gm._paintManager = pm
  - gm._ui = ui
  - pm._gameManager = gm
  - UIボタン: GravityUp/Down/Left/Right → pm.ApplyGravity(direction)
  - ColorButtons → pm.SelectColor(index)
  - NextStageButton → gm.NextStage()
  - RestartButton → gm.RestartGame()

## StageManager 統合
- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
- `OnStageChanged(int stage)` → `_paintManager.SetupStage(config, stage)`
- StageConfigs (5 entries): speedMultiplier/complexityFactor を利用

## InstructionPanel 内容
- title: "GravityPainter"
- description: "重力で絵の具を流してアートを描こう"
- controls: "ボタンで重力方向を変え、タップで絵の具を投下"
- goal: "お手本と同じ絵を50%以上一致させてクリア！"

## ビジュアルフィードバック設計
1. **正解パルス**: セルが塗られたときに `transform.localScale` 1.0→1.3→1.0（0.2秒）
2. **重力適用フラッシュ**: 移動した絵の具セルが 0.1秒間、明るい色に変わる
3. **一致率達成**: MatchRateText がゴールドに点滅
4. **ゲームオーバー**: カメラシェイク + 赤フラッシュ

## スコアシステム
- 基本スコア: 一致率 × 10 × (stageIndex+1) × 100
- 効率ボーナス: 残り投下回数 × 20
- 連鎖ボーナス: 1重力変更で3セル以上塗れた場合 × 1.5
- ステージコンボ: 連続クリア回数に応じて 1.0/1.2/1.5倍

## 判断ポイントの実装設計
- 重力ボタン押下時: 何セル移動するか予測表示（ゴースト表示 or ハイライト）
- 投下コスト: 1回の投下で PaintBudget -1、0になると投下不可（重力変更のみ可能）
- 一致率は重力変更のたびにリアルタイム更新

## Buggy Code 防止
- Physics2D タグ比較: `gameObject.name` 使用
- 複数クラスの Update(): `_isActive` ガードで排他制御
- 動的生成 Texture2D/Sprite: `OnDestroy()` でクリーンアップ
- セル GameObject は `_cellObjects[r,c]` 配列で管理、SetupStage() で全破棄・再生成
