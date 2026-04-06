# Design: Game050v2_BubbleSort

## namespace
`Game050v2_BubbleSort`

## スクリプト構成

| クラス | ファイル | 担当 |
|--------|---------|------|
| `BubbleSortGameManager` | BubbleSortGameManager.cs | ゲーム状態管理・StageManager/InstructionPanel統合・スコア管理 |
| `BubbleGridManager` | BubbleGridManager.cs | グリッド管理・バブル入れ替え・ソート判定・3連消し・入力処理 |
| `BubbleCell` | BubbleCell.cs | 個々のバブルデータ（色・タイプ・タイマー）+ SpriteRenderer |
| `BubbleSortUI` | BubbleSortUI.cs | HUD表示（手数・スコア・ステージ・進捗）・クリア/ゲームオーバーパネル |

## 盤面・ステージデータ設計

### グリッドレイアウト

```
Stage 1: 3列×3行 = 9マス / 2色
Stage 2: 4列×4行 = 16マス / 3色
Stage 3: 4列×5行 = 20マス / 4色
Stage 4: 5列×5行 = 25マス / 4色
Stage 5: 5列×6行 = 30マス / 5色
```

### BubbleType enum
```csharp
public enum BubbleType { Normal, Fixed, Timer, Bomb }
```

### 色設定（casual カラーパレット）
- Color 0: #4CAF50 (緑)
- Color 1: #FFEB3B (黄)
- Color 2: #2196F3 (青) [Stage2以降]
- Color 3: #F44336 (赤) [Stage3以降]
- Color 4: #9C27B0 (紫) [Stage5のみ]
- Fixed: #9E9E9E (灰)

## 入力処理フロー

`BubbleGridManager.Update()` で一元管理:

```
1. Mouse.current.leftButton.wasPressedThisFrame 検出
2. Mouse.current.position.ReadValue() でスクリーン座標取得
3. Camera.main.ScreenToWorldPoint で変換
4. Physics2D.OverlapPoint でBubbleCellコライダーをヒット判定
5. 1つ目クリック → _selectedCell に記録、ハイライト表示
6. 2つ目クリック → 隣接チェック、隣接なら入れ替え（1手消費）
7. 入れ替え後 → 3連消しチェック → ソート完了チェック
```

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;  // = 5
float camWidth = camSize * Camera.main.aspect;  // ≒ 2.8
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 2.8f; // UIボタン領域
float availableHeight = camSize * 2f - topMargin - bottomMargin;
// = 10 - 1.2 - 2.8 = 6.0

// gridRows = ステージ別
float cellSize = Mathf.Min(
    availableHeight / gridRows,
    camWidth * 2f / gridCols,
    1.2f  // maxCellSize
);

// グリッド中心: Y = -topMargin/2 + bottomMargin/2 = 0.8f 下方向
Vector2 gridCenter = new Vector2(0f, (camSize - topMargin) - availableHeight * 0.5f - topMargin * 0.5f);
```

## SceneSetup の構成方針

- `[MenuItem("Assets/Setup/050v2 BubbleSort")]`
- カメラ: orthographicSize = 5, 背景色 = ライムグリーン（casual）
- GameManager → StageManager（子）
- Canvas上にInstructionPanel（sortOrder最前面）
- BubbleGrid GameObject → BubbleGridManager コンポーネント
- HUD: ステージ・手数・進捗・スコアテキスト
- ボタン: Undo（下段左）、Menu（最下部）
- StageClearPanel・AllClearPanel・GameOverPanel

## StageManager 統合

```csharp
// GameManager.Start()
_instructionPanel.Show("050v2", "BubbleSort",
    "色バブルを並び替えてソートを完成させよう",
    "隣り合う2つのバブルを順にタップして入れ替えよう。同じ色を3つ以上揃えると消えてボーナス！",
    "全バブルを色ごとにまとめて並び替えたらクリア！手数が少ないほど高得点");
_instructionPanel.OnDismissed += StartGame;

// StartGame()
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

// OnStageChanged(int stageIndex)
var config = _stageManager.GetCurrentStageConfig();
_gridManager.SetupStage(config, stageIndex + 1);
```

### ステージ別パラメータ表

| Stage | speedMult | countMult | complexityFactor | cols | rows | colors | moves |
|-------|-----------|-----------|------------------|------|------|--------|-------|
| 1 | 1.0 | 1.0 | 0.0 | 3 | 3 | 2 | 10 |
| 2 | 1.0 | 1.0 | 0.0 | 4 | 4 | 3 | 15 |
| 3 | 1.0 | 1.2 | 0.2 | 4 | 5 | 4 | 18 |
| 4 | 1.5 | 1.2 | 0.2 | 5 | 5 | 4 | 20 |
| 5 | 1.5 | 1.5 | 0.3 | 5 | 6 | 5 | 22 |

## InstructionPanel 内容

```csharp
_instructionPanel.Show(
    "050v2",
    "BubbleSort",
    "色バブルを並び替えてソートを完成させよう",
    "隣り合う2つのバブルを順にタップして入れ替えよう。同じ色を3つ以上揃えると消えてボーナス！",
    "全バブルを色ごとにまとめて並び替えたらクリア！手数が少ないほど高得点"
);
```

## ビジュアルフィードバック設計

1. **入れ替え成功時（スケールパルス）**: 入れ替え対象の2バブルを 1.0 → 1.3 → 1.0（0.2秒）でスケールポップ
2. **3連消し時（色フラッシュ + スケール）**: 消えるバブルを白くフラッシュ → 縮小消去 + "+500 COMBO!" テキスト表示
3. **タイマーバブル変化（赤フラッシュ）**: カウント0でランダム色変化する瞬間に SpriteRenderer を赤くフラッシュ
4. **選択中ハイライト**: 選択バブルのスケールを 1.15 に拡大 + 黄色アウトライン

## スコアシステム

| 要素 | 点数 |
|------|------|
| クリア基本点 | 1000点 |
| 残り手数ボーナス | 残手数 × 200点 |
| 3連消しボーナス | 500点 |
| 連続消しコンボ | コンボ数 × 300点 |
| 4連消し | 800点 |
| 5連消し | 1500点 |
| パーフェクトクリア | ×3倍 |

コンボ乗算:
- 1連: ×1
- 2連: ×2
- 3連以上: ×3

## ステージ別新ルール表

| Stage | 新ルール | 実装詳細 |
|-------|---------|---------|
| 1 | 基本のみ | 2色3×3グリッド。タップ入れ替えのみ |
| 2 | **3連消し** | 横/縦に同色3+つ連続で消去・上から補充 |
| 3 | **固定バブル（灰）** | `BubbleType.Fixed` セルは入れ替え不可、周囲のバブルで迂回 |
| 4 | **タイマーバブル** | `BubbleType.Timer`、3秒カウント（speedMult適用）でランダム色変化 |
| 5 | **爆弾バブル** | `BubbleType.Bomb`、3連消しに含まれると周囲1マスも消去 |

## 判断ポイントの実装設計

### トリガー条件
1. プレイヤーがバブル1つ目を選択した瞬間 → 残り手数表示を強調（プレッシャー）
2. 3連消しが可能な配置になった瞬間 → グロー効果（オプション選択を促す）
3. タイマーバブルのカウントが1になった瞬間 → テキストが赤く点滅（緊急度表示）

### 報酬/ペナルティ数値
- 3連消しで消した場合: +500点、手数節約なし（入れ替えに手数使用済み）
- タイマー変化を許した場合: ペナルティなし（ただしランダム色でソート難易度上昇）
- Undo使用: 手数回復なし（ペナルティとして機能）

## SceneSetup 配線フィールド一覧

```
BubbleSortGameManager:
  - _stageManager: StageManager
  - _instructionPanel: InstructionPanel
  - _gridManager: BubbleGridManager
  - _ui: BubbleSortUI

BubbleGridManager:
  - _gameManager: BubbleSortGameManager (GetComponentInParent)
  - _bubblePrefab: Sprite[] (5色分)
  - _fixedSprite: Sprite
  - _timerSprite: Sprite
  - _bombSprite: Sprite

BubbleSortUI:
  - _stageText, _movesText, _progressText, _scoreText
  - _comboText
  - _stageClearPanel, _allClearPanel, _gameOverPanel
  - _finalScoreText (各パネル内)
  - _nextStageButton, _restartButton, _menuButton, _undoButton
```

## Buggy Code 防止事項

- `_isActive` ガード: `BubbleGridManager.Update()` は `_isActive` で制御
- Texture/Sprite はすべて `File.WriteAllBytes` → `AssetDatabase.ImportAsset` → `LoadAssetAtPath` で保存
- `OnDestroy()` でイベント購読解除 + 動的生成Texture2Dを `Destroy()`
- Physics2D でバブル判定は `gameObject.name` でなく `GetComponent<BubbleCell>() != null` で型判定
