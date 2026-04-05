# Design: Game016v2 LightSwitch

## namespace
`Game016v2_LightSwitch`

## スクリプト構成

### LightSwitchGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] BulbManager _bulbManager
- [SerializeField] LightSwitchUI _ui
- Start(): InstructionPanel.Show() → OnDismissed → StartGame()
- StartGame(): _stageManager.StartFromBeginning()
- OnStageChanged(int stage): BulbManager.SetupStage(config, stageNum)
- OnAllStagesCleared(): Clear状態へ
- OnStageClear(int remaining, int maxMoves, bool undoUsed): スコア計算・コンボ更新
- OnGameOver(): GameOver状態へ
- OnNextStage() / OnRetry() / OnReturnToMenu() / ShowInstructions()

### BulbManager.cs
- コアメカニクス: 電球グリッド管理・タップ処理・目標パターン管理
- [SerializeField] LightSwitchGameManager _gameManager
- 各種スプライト SerializeField
- SetupStage(StageManager.StageConfig config, int stageNum): ステージ設定適用
- タップ処理: Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint
- 電球状態反転: タップ電球+隣接4方向
- ステージ別特殊電球処理:
  - Stage2: 色付き電球（赤/青）、同色のみ連動
  - Stage3: ワープ電球、対角方向にも波及
  - Stage4: 遅延電球（1手遅れ反転、_pendingFlips Queue）
  - Stage5: シフト目標（5手ごとにPatternShift()）
- Undo機能: Stack<bool[]> _history、UndoMove()
- CheckComplete(): 全電球と目標パターンの比較
- ビジュアルフィードバック:
  - 正解: スケールパルス(1.0→1.3→1.0, 0.2秒) + 色フラッシュ(黄)
  - ミス(上限超え): 赤フラッシュ全体
  - Undo: ブルーパルス
- レスポンシブ配置: Camera.main.orthographicSize から動的計算

### LightSwitchUI.cs
- UpdateStage(int stage, int total)
- UpdateScore(int score)
- UpdateMoves(int remaining, int max)
- UpdateUndo(int remaining)
- ShowStageClearPanel(int stageScore, int combo)
- ShowGameOverPanel()
- ShowClearPanel(int totalScore)
- HideAllPanels()
- UpdateTargetPattern(bool[] pattern, int gridSize): 目標パターン小窓更新

## ステージ別パラメータ

| Stage | gridSize | maxMoves | maxUndo | 特殊電球 |
|-------|----------|----------|---------|---------|
| 1 | 3 | 10 | 3 | なし |
| 2 | 4 | 18 | 3 | 色付き電球（2色） |
| 3 | 4 | 20 | 2 | ワープ電球（対角波及） |
| 4 | 5 | 28 | 2 | 遅延電球（1手遅れ） |
| 5 | 5 | 32 | 1 | 全要素+シフト目標 |

StageManager.StageConfig の使い方:
- speedMultiplier → 未使用（パズルゲームのため）
- countMultiplier → gridSize乗数（3=1.0, 4=1.33, 5=1.67）
- complexityFactor → 特殊電球の割合（0.0〜1.0）

実際のgridSizeはステージ番号から直接決定（config依存しない）

## 電球データ構造

```csharp
class BulbCell {
    int row, col;
    bool isOn;
    BulbType type; // Normal, Colored(Red/Blue), Warp, Delayed
    int colorGroup; // 0=none, 1=Red, 2=Blue
    GameObject obj;
    SpriteRenderer sr;
}
```

## InstructionPanel内容
- title: "LightSwitch"
- description: "電球をタップして目標のパターンを作ろう。隣の電球も連動するよ"
- controls: "電球をタップでオン/オフ切替（隣接も反転）"
- goal: "少ない手数で目標パターンを完成させよう"

## ビジュアルフィードバック設計
1. **クリア演出**: 各電球がウェーブ状にスケールパルス（行×0.05秒の遅延）+ 全体黄色フラッシュ
2. **ミス（手数超過）**: 全電球の赤フラッシュ（0.5秒）
3. **通常タップ**: タップした電球の微小スケールパルス（1.0→1.15→1.0, 0.1秒）
4. **Undo**: タップ電球の青パルス

## スコアシステム
- ステージ基礎点 = 1000 × ステージ番号
- スコア = 基礎点 × (1 + 残り手数/上限)
- Undo未使用ボーナス: ×1.5
- コンボ乗算: 2連続=×1.3, 3連続=×1.6, 5連続=×2.0

## ステージ別新ルール
- Stage 1: 基本ルール（隣接反転）、3×3、全消灯が目標
- Stage 2: **色付き電球**追加。赤グループと青グループが独立して連動
- Stage 3: **ワープ電球**追加。特定電球タップで対角方向にも反転波及
- Stage 4: **遅延電球**追加。タップ後に1手遅れて反転（先読みが必要）
- Stage 5: 全要素統合+**シフト目標**（5手ごとに目標パターン変化）

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;  // 5.0
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;   // HUD
float bottomMargin = 3.0f; // CanvasUI
float available = (camSize * 2f) - topMargin - bottomMargin;
float cellSize = Mathf.Min(available / gridSize, (camWidth * 2f - 0.4f) / gridSize, 1.8f);
Vector2 origin = new Vector2(-(gridSize-1)*cellSize*0.5f, camSize - topMargin - cellSize*0.5f);
```

## SceneSetup構成方針
- Menu: `Assets/Setup/016v2 LightSwitch`
- シーン: `Assets/Scenes/016v2_LightSwitch.unity`
- Sprite.Create禁止 → AssetDatabase.LoadAssetAtPath<Sprite>()
- EventSystem: InputSystemUIInputModule使用
- 全フィールドをSerializedObjectで配線
- ボタン: sizeDelta最小(150,55)
- UI配置:
  - 上部: StageText(左), ScoreText(右), MoveText(中央)
  - 中央右: TargetPatternPanel（目標パターン小窓）
  - 下段: Undoボタン + メニューへ戻るボタン
