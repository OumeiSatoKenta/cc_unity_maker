# Design: Game020v2 EchoMaze

## namespace
`Game020v2_EchoMaze`

## スクリプト構成

| ファイル | クラス | 役割 |
|---------|------|------|
| EchoMazeGameManager.cs | EchoMazeGameManager | ゲーム状態管理・スコア・StageManager/InstructionPanel統合 |
| MazeController.cs | MazeController | 迷路生成・プレイヤー移動・エコー計算・5ステージ難易度 |
| EchoMazeUI.cs | EchoMazeUI | UI表示・エコーインジケーター・探索マップ |
| Setup020v2_EchoMaze.cs | Setup020v2_EchoMaze (Editor) | SceneSetup Editorスクリプト |

## 盤面・ステージデータ設計

```csharp
// StageManager.StageConfig の speedMultiplier / countMultiplier / complexityFactor を活用
// speedMultiplier → 移動する壁の開閉速度
// countMultiplier → 迷路サイズ係数  
// complexityFactor → 撹乱エリアの割合

struct StageParams {
    int gridSize;          // 5, 7, 9, 9, 11
    int moveLimit;         // 30, 50, 70, 60, 80
    bool hasFloorTypes;    // Stage2以降
    bool hasMovingWalls;   // Stage3以降
    bool hasEchoDisturb;   // Stage4以降
    bool hasTwoFloors;     // Stage5のみ
}
```

## 入力処理フロー
- 方向ボタン4個（UI Button）→ `MazeController.TryMove(direction)` 呼び出し
- 中央エコーボタン → `MazeController.EmitEcho()` 呼び出し
- マップボタン → `EchoMazeUI.ToggleMap()` 呼び出し
- 全入力は Canvas UI ボタン経由（`Mouse.current` は使用しない）

## SceneSetup 構成方針
- Camera: orthographicSize=5, 背景色=黒
- Canvas: ReferenceResolution(1080, 1920)
- GameManager > StageManager (子)
- GameManager > MazeController (子)
- GameManager > EchoMazeUI (子)

## StageManager統合

```csharp
void Start() {
    _instructionPanel.Show("020v2", "EchoMaze", "音のエコーだけを頼りに...", 
                            "方向ボタンで移動、中央ボタンでエコー発信", 
                            "エコーを聞いて壁を避け、ゴールに到達しよう");
    _instructionPanel.OnDismissed += StartGame;
}

void StartGame() {
    _stageManager.StartFromBeginning();
}

_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stage) {
    _mazeController.SetupStage(stage, _stageManager.GetCurrentStageConfig());
    _ui.UpdateStageDisplay(stage);
}
```

## InstructionPanel内容
- title: "EchoMaze"
- description: "音のエコーだけを頼りに見えない迷路を進もう"
- controls: "方向ボタンで移動、中央ボタンでエコー発信"
- goal: "エコーを聞いて壁を避け、ゴールに到達しよう"

## ビジュアルフィードバック設計

1. **移動成功時（探索コンボ）**:
   - プレイヤーオブジェクトのスケールパルス: 1.0 → 1.3 → 1.0 (0.15秒)
   - 訪問済みマスの色フラッシュ（薄い青→通常色）

2. **壁ぶつかり時**:
   - プレイヤーオブジェクトの赤フラッシュ (SpriteRenderer.color = red, 0.2秒)
   - 小さな揺れアニメーション（X軸±0.1f、0.15秒）

3. **エコーインジケーター更新**:
   - 4方向の棒グラフがスムーズに変化（壁距離0=最大強度、距離が遠い=弱）
   - 床素材に応じたインジケーターの色変化（Stage2以降）

## スコアシステム
- 基本スコア = ステージ基礎点(1000) × (1 + 残り移動数/移動上限)
- エコー発信未使用ボーナス：×1.5
- マップ未確認ボーナス：×2.0
- 最短経路クリア：×3.0
- 探索コンボ（新規マス連続探索）：×1.0/×1.2/×1.5（コンボ数3以上で最大）

## ステージ別新ルール表
| Stage | 新要素 | 詳細 |
|-------|------|------|
| 1 | 基本エコー | 5×5迷路、シンプルなエコー強度表示 |
| 2 | 床素材 | 石(青)・木(緑)・水(赤)でエコーインジケーター色変化 |
| 3 | 移動する壁 | 5歩ごとにランダムな壁が開閉 |
| 4 | 撹乱エリア | 特定マスでエコー値がランダム化（罠！） |
| 5 | 二層迷路 | ポータル（階段）で上下層を切り替え |

## 判断ポイントの実装
- エコー発信使用フラグ `_usedEcho` → ステージ終了時にボーナス計算
- マップ表示フラグ `_usedMap` → ステージ終了時にボーナス計算
- 移動回数カウンタ → 残り移動数表示・ゲームオーバー判定

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;  // 5f
float camWidth = camSize * Camera.main.aspect; // ~2.8f (9:16)
float topMargin = 1.2f;
float bottomMargin = 3.0f;  // 方向ボタン群のため広め
float availableHeight = camSize * 2f - topMargin - bottomMargin;
// 11×11の場合: 6.8 / 11 ≒ 0.618 → cellSize = min(0.618, ...) 
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 0.8f);
```

## SceneSetup 配線必須フィールド
**EchoMazeGameManager:**
- `_stageManager` (StageManager)
- `_instructionPanel` (InstructionPanel)
- `_mazeController` (MazeController)
- `_ui` (EchoMazeUI)

**MazeController:**
- `_gameManager` (EchoMazeGameManager)
- `_ui` (EchoMazeUI)
- `_playerSprite` (Sprite)
- `_cellSprite` (Sprite)
- `_goalSprite` (Sprite)
- `_portalSprite` (Sprite)

**EchoMazeUI:**
- `_stageText` (TMP)
- `_scoreText` (TMP)
- `_moveLimitText` (TMP)
- `_echoNorthBar`, `_echoSouthBar`, `_echoEastBar`, `_echoWestBar` (Image)
- `_mapPanel` (GameObject)
- `_stageClearPanel` (GameObject)
- `_clearPanel` (GameObject)
- `_gameOverPanel` (GameObject)
- 各パネル内のテキスト・ボタン
