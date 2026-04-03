# Design: Game003_GravitySwitch

## スクリプト構成

### namespace: `Game003_GravitySwitch`

| クラス | ファイル | 責務 |
|-------|---------|------|
| `GravitySwitchGameManager` | GravitySwitchGameManager.cs | ゲーム状態管理、StageManager/InstructionPanel統合、スコア管理 |
| `GravityManager` | GravityManager.cs | 盤面管理、ボール移動ロジック、重力切替処理、5ステージ対応 |
| `GravitySwitchUI` | GravitySwitchUI.cs | HUD表示、パネル管理、ボタンイベント |

### GameManager 参照
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] GravityManager _gravityManager`
- `[SerializeField] GravitySwitchUI _ui`

### GravityManager 参照
- `GravitySwitchGameManager _gameManager` → `GetComponentInParent<GravitySwitchGameManager>()`

## 盤面・ステージデータ設計

### セル種別
```csharp
public enum CellType { Empty, Wall, Hole, Goal, SlidingFloor }
```

### ステージデータ（ScriptableObjectではなく配列でハードコード）
```csharp
public struct StageData {
    public int gridSize;
    public CellType[,] cells;
    public Vector2Int ballStart;    // ボール1開始位置
    public Vector2Int ball2Start;   // ボール2開始位置（ステージ5のみ）
    public Vector2Int goalPos;      // ゴール1位置
    public Vector2Int goal2Pos;     // ゴール2位置（ステージ5のみ）
    public bool hasHole;            // 穴あり（ステージ3〜）
    public bool hasSlidingFloor;    // 移動床あり（ステージ4〜）
    public bool hasTwoBalls;       // 2ボール（ステージ5）
    public int moveLimit;           // 手数制限（0=なし）
    public int minMoves;            // ★3クリアの最小手数
}
```

### レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;  // 6
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 3.0f; // UI Button領域（重力ボタン4個 + リセット）
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// gridSize=5のとき: 12-1.2-3.0=7.8, cellSize=7.8/5=1.56
// gridSize=7のとき: 12-1.2-3.0=7.8, cellSize=7.8/7=1.11
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.6f);
```

## 入力処理フロー

1. `GravitySwitchUI` の4方向ボタン（↑↓←→）から `GravitySwitchGameManager.OnGravityButton(Direction)` を呼ぶ
2. `GameManager` がゲーム状態チェック後 `GravityManager.ApplyGravity(Direction)` を呼ぶ
3. `GravityManager` がボール移動をシミュレートし、移動完了後にコールバックを返す
4. 穴落下 → `GameManager.OnFallIntoHole()` → ゲームオーバー
5. ゴール到達 → `GameManager.OnReachGoal()` → ステージクリア判定

## SceneSetup 構成方針

- `[MenuItem("Assets/Setup/003 GravitySwitch")]`
- 盤面セルは `GameObject` として生成（SpriteRenderer付き）
- ボール: Circle形状スプライト
- ゴール: Star/Target形状スプライト
- 穴: Dark circular sprite
- 壁: Rounded square sprite

## StageManager 統合

```csharp
// GameManager.Start()
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

// OnStageChanged(int stageIndex)
_gravityManager.SetupStage(stageIndex);
_ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
_ui.HideAllPanels();
_state = GameState.Playing;
```

### ステージ別パラメータ表

| Stage | gridSize | hasHole | hasSlidingFloor | hasTwoBalls | moveLimit | minMoves |
|-------|---------|---------|----------------|------------|----------|---------|
| 1 (idx 0) | 5 | false | false | false | 0 | 3 |
| 2 (idx 1) | 6 | false | false | false | 0 | 5 |
| 3 (idx 2) | 6 | true | false | false | 0 | 4 |
| 4 (idx 3) | 7 | true | true | false | 15 | 7 |
| 5 (idx 4) | 7 | true | false | true | 20 | 10 |

## InstructionPanel 内容

```csharp
_instructionPanel.Show(
    "003",
    "GravitySwitch",
    "重力を切り替えてボールをゴールに導くパズル",
    "4方向ボタンまたはスワイプで重力切替",
    "ボールをゴールまで転がそう"
);
```

## ビジュアルフィードバック設計

### 演出1: ゴール到達時スケールパルス
- ゴールオブジェクトが 1.0 → 1.5 → 1.0 にスケールアニメーション（0.3秒）
- ボールがゴールに重なった瞬間に発動

### 演出2: ゲームオーバー時赤フラッシュ + カメラシェイク
- `SpriteRenderer.color` を白→赤→白にフラッシュ（0.5秒）
- カメラを ±0.1 ユニットでランダムシェイク（0.4秒）
- 穴に落ちた瞬間に発動

### 演出3: 重力切替時ボール移動トレイル
- ボール移動中に残像エフェクト（スケール0.5〜1.0の透明度低下スプライト）
- 移動距離が3セル以上の場合のみ発動

## スコアシステム

```csharp
// ステージクリア時
int moveBonus = Mathf.Max(0, (_stageData.moveLimit > 0 ? _stageData.moveLimit : 20) - _movesUsed);
int baseScore = moveBonus * 100 * (stageIndex + 1);
float comboMultiplier = 1f + (_comboCount * 0.2f); // 連続クリアで+0.2倍
int stageScore = (int)(baseScore * comboMultiplier);
// ★評価
int starRating = _movesUsed <= _stageData.minMoves ? 3
               : _movesUsed <= _stageData.minMoves + 2 ? 2 : 1;
```

## ステージ別新ルール表

| Stage | 新ルール |
|-------|---------|
| 1 | 基本の重力切替のみ（壁に当たるまで滑る） |
| 2 | 複数ステップで迂回が必要な複雑壁配置（直線1手では届かない） |
| 3 | **穴（Hole）追加**: ボールが穴マスに入ったらゲームオーバー |
| 4 | **移動床 + 手数制限**: 1手ごとに移動床が1マスずれる、手数制限15手 |
| 5 | **2ボール同時操作**: 2個のボールが同じ重力で同時に動く、両方ゴールへ（手数制限20手） |

## 判断ポイントの実装設計

### トリガー条件
- ゴール到達に複数ルートが存在する（プレイヤーが最短を選択）
- 穴の隣のマスを通過する際（回避 or 通過のリスク判断）
- ステージ5でボール1をゴールに入れるとボール2が危険な位置になるケース

### 報酬/ペナルティ
- 最短手数クリア: +★3、スコア×2.0
- 穴近傍通過成功: +50点テクニカルボーナス
- 穴落下: ゲームオーバー（手数消費なし、リスタート）
- 手数超過: ゲームオーバー（ステージ4〜）

## Buggy Code 防止

- `_isActive` フラグ: `GravityManager.Update()` にてボール移動中は入力を無効化
- Texture2D / Sprite: `OnDestroy()` でクリーンアップ
- Physics2D 不使用: グリッドベースなので配列インデックスでボール位置管理
- ゴール・穴の判定は `Vector2Int` 比較で行う（座標精度問題なし）

## SceneSetup 配線フィールド一覧

```
GravitySwitchGameManager:
  _stageManager → StageManager
  _instructionPanel → InstructionPanel
  _gravityManager → GravityManager
  _ui → GravitySwitchUI

GravitySwitchUI:
  _stageText → StageText (TMP)
  _scoreText → ScoreText (TMP)
  _moveText → MoveText (TMP)
  _stageClearPanel → StageClearPanel
  _gameClearPanel → GameClearPanel
  _gameOverPanel → GameOverPanel
  _nextStageButton → NextStageButton
  _retryButton → RetryButton
  _menuButton → MenuButton (in GameOverPanel)
  _menuButton2 → MenuButton (常時表示)
  _resetButton → ResetButton
  _upButton, _downButton, _leftButton, _rightButton → 4方向ボタン
```
