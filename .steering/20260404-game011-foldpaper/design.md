# Design: Game011v2 FoldPaper

## namespace
`Game011v2_FoldPaper`

## スクリプト構成

### FoldPaperGameManager.cs
ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] FoldPaperManager _foldPaperManager`
- `[SerializeField] FoldPaperUI _ui`
- Start() → InstructionPanel.Show() → OnDismissed += StartGame
- StartGame() → StageManager.StartFromBeginning()
- OnStageChanged(int stage) → FoldPaperManager.SetupStage() + UI更新
- OnAllStagesCleared() → 最終クリアパネル表示
- OnFoldResult(bool clearReached, int movesUsed, int movesLimit, bool undoUnused)
  - スコア計算: 基本点 × コンボ倍率 × Undoボーナス + 残り手数ボーナス
  - 星評価: movesUsed <= minMoves → ★3 / movesUsed <= limit*0.7 → ★2 / else → ★1
- OnRetry() → FoldPaperManager.ResetStage()
- OnNextStage() → StageManager.CompleteCurrentStage()
- OnReturnToMenu() → SceneManager.LoadScene("TopMenu")

### FoldPaperManager.cs
コアメカニクス。折り紙の状態管理と入力処理を担う。

**データ構造:**
```csharp
// セル状態: true=紙あり, false=紙なし
bool[,] _grid;
// 折り線: (type=Horizontal/Vertical/Diagonal45/Diagonal135, index)
List<FoldLine> _foldLines;
// 目標グリッド
bool[,] _targetGrid;
// 履歴（Undo用）
Stack<bool[,]> _history;
```

**SetupStage(StageManager.StageConfig config, int stage):**
- ステージパラメータに基づいてグリッドサイズ・折り線・目標形を設定
- ステージ別パラメータ:
  - Stage 1: 4×4 / maxFolds=4 / type=Horizontal,Vertical
  - Stage 2: 4×4 / maxFolds=6 / type=Horizontal,Vertical + 重なり判定
  - Stage 3: 5×5 / maxFolds=8 / type=+Diagonal
  - Stage 4: 5×5 / maxFolds=10 / type=+Flip
  - Stage 5: 6×6 / maxFolds=12 / type=All + 60秒制限

**入力処理（一元管理）:**
- Mouse.current.leftButton.wasPressedThisFrame で毎フレーム確認
- Physics2D.OverlapPoint でヒット判定
- 折り線タップ → SelectedFoldLine を設定（ハイライト表示）
- 紙エリアタップ → selectedFoldLineに基づいてFold処理
  - タップ位置と折り線の位置関係から方向を決定

**Fold実行:**
1. 現在グリッドを _history にプッシュ
2. 折り線を軸に指定方向のセルを反転/移動
3. 重なり判定（Stage 2以降）: 重なりは消える（XOR演算）
4. CheckGoal() で目標形と比較
5. movesLeft-- して手数チェック

**ビジュアルフィードバック:**
- 折り線選択時: SpriteRenderer.color をゴールドにフラッシュ（0.2秒）
- クリア時: 各セルにスケールパルスアニメーション（1.0 → 1.3 → 1.0, 0.2秒）
- 手数オーバー時: カメラシェイク + 赤フラッシュ

**レスポンシブ配置（必須）:**
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD
float bottomMargin = 3.2f; // UIボタン（Undo, Reset等）
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.0f);
```

### FoldPaperUI.cs
UI表示管理
- `[SerializeField] TextMeshProUGUI _stageText`
- `[SerializeField] TextMeshProUGUI _scoreText`
- `[SerializeField] TextMeshProUGUI _movesText`（残り手数）
- `[SerializeField] TextMeshProUGUI _undoText`（Undo残り回数）
- `[SerializeField] TextMeshProUGUI _comboText`
- `[SerializeField] GameObject _stageClearPanel`
- `[SerializeField] TextMeshProUGUI _stageClearScoreText`
- `[SerializeField] TextMeshProUGUI _stageClearStarsText`
- `[SerializeField] GameObject _gameClearPanel`
- `[SerializeField] TextMeshProUGUI _gameClearScoreText`
- `[SerializeField] Image _targetSilhouetteImage`（右上、目標シルエット表示）

## 盤面・ステージデータ設計

```csharp
public struct StageData
{
    public int gridSize;      // 4, 5, or 6
    public int maxFolds;      // 折り上限手数
    public int minFolds;      // 最少手数（★3基準）
    public int undoCount;     // Undo可能回数（全ステージ3回固定）
    public bool[,] initialGrid;  // 初期紙配置（全セルtrue）
    public bool[,] targetGrid;   // 目標形状
    public List<FoldLineData> foldLines; // 利用可能な折り線
    public bool hasOverlapRule;  // Stage 2以降: 重なりXOR
    public bool hasDiagonal;     // Stage 3以降: 斜め折り
    public bool hasFlip;         // Stage 4以降: フリップ
    public int timeLimit;        // Stage 5: 60秒, それ以外: 0（無制限）
}
```

## StageManager統合
- `OnStageChanged` 購読: ステージ番号に応じて `GetCurrentStageConfig()` のspeedMultiplier等を利用
  - speedMultiplier → timeLimit調整
  - countMultiplier → gridSize選択（1.0=4x4, 1.5=5x5, 2.0=6x6）
  - complexityFactor → 斜め折り・フリップ解禁フラグ
- `OnAllStagesCleared` 購読: 全クリア演出

## InstructionPanel内容
- title: "FoldPaper"
- description: "折り線をタップして紙を折り、お手本の形を作ろう"
- controls: "折り線タップ→選択 / 紙の上下タップ→折る方向決定"
- goal: "手数以内に目標のシルエットと同じ形を作ろう"

## ビジュアルフィードバック設計
1. **成功時（クリア）**: 各セルが0.2秒かけて1.0→1.3→1.0のスケールパルス（順番に波及）
2. **失敗時（手数オーバー）**: カメラシェイク（0.3秒、振幅0.2f）+ 全セルが赤フラッシュ

## スコアシステム
- 基本スコア: 1000 × (stage + 1)
- 残り手数ボーナス: movesLeft × 50
- Undo未使用ボーナス: undoUnused ? × 1.5 : × 1.0
- コンボ乗算: 1.0 + (combo-1) × 0.2（上限 ×3.0）

## ステージ別新ルール表
| Stage | 新要素 | 詳細 |
|-------|-------|------|
| 1 | 基本折りのみ | 4×4グリッド、横・縦折りのみ、チュートリアル |
| 2 | 重なり判定 | 折った紙が別の紙と重なると消える（XOR）→戦略的な折り順が必要 |
| 3 | 斜め折り | 45度折り線追加。対角線で折れるため目標形の多様化 |
| 4 | フリップ折り | 折った後に折り部分を180度反転する特殊操作 |
| 5 | 時間制限 + 全要素 | 60秒タイマー追加。全テクニックの総合問題 |

## SceneSetup構成方針（Setup011v2_FoldPaper.cs）
- MenuItem: `Assets/Setup/011v2 FoldPaper`
- 背景: puzzle カテゴリ → 青系（#2196F3 ベース）
- Canvas構成:
  - HUD上部: Stage（左上）、Score（右上）、Combo（中央上）
  - 残り手数（左上サブ）、Undo残り（左上サブ2）
  - 右上: 目標シルエット表示エリア（100×100）
  - 下部操作ボタン: Undo（左）、Reset（右）横並び + メニューボタン最下部
  - StageClearPanel（中央オーバーレイ）
  - GameClearPanel（中央オーバーレイ）
  - InstructionPanel（フルスクリーン）
- ゲームオブジェクト: FoldPaperManager（GmObj子）、StageManager（GmObj子）
- 全フィールド配線:
  - GameManager: stageManager, instructionPanel, foldPaperManager, ui
  - FoldPaperManager: gameManager, ui, 各種スプライト
  - FoldPaperUI: 全テキスト、各パネル、ボタン
- ボタンイベント: Undo→foldMgr.Undo, Reset→foldMgr.ResetStage, Next→gm.OnNextStage, Menu→gm.OnReturnToMenu

## Buggy Code防止チェックリスト
- [ ] Physics2D タグ比較は gameObject.name/tag を使う
- [ ] `_isActive` ガード: Playing状態以外では入力を受け付けない
- [ ] OnDestroy() で動的生成Texture2D/Sprite をクリーンアップ
- [ ] 折り線と紙セルのGameObjectはSetupStage毎にDestroyして再生成

## 判断ポイントの実装設計
1. **どの折り線を選ぶか**: 全折り線が同時に表示（ハイライトなし状態）。タップで選択後0.5秒の確認時間
2. **Undo使用判断**: UndoボタンUI残り回数表示。0回時はボタンをグレーアウト
3. **手数効率**: 残り手数カウンターが3以下になると赤色に変化してプレッシャー演出
