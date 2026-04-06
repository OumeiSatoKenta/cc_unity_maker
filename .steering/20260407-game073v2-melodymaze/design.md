# Design: Game073v2_MelodyMaze

## namespace
`Game073v2_MelodyMaze`

## スクリプト構成

| クラス | ファイル | 担当 |
|--------|---------|------|
| `MelodyMazeGameManager` | MelodyMazeGameManager.cs | 状態管理・StageManager/InstructionPanel統合 |
| `MazeManager` | MazeManager.cs | 迷路グリッド・音符ノード・経路・入力処理 |
| `MelodyMazeUI` | MelodyMazeUI.cs | スコア・タイマー・コンボ・判定・ステージ表示 |

## 状態遷移

```
WaitingInstruction → Playing → (ゴール到達)
                                ├─ 正解率≥50% → StageClear → (次ステージ or AllClear)
                                └─ 正解率<50%  → GameOver
                     (タイムアップ) → GameOver
```

## MelodyMazeGameManager

```csharp
[SerializeField] StageManager _stageManager;
[SerializeField] InstructionPanel _instructionPanel;
[SerializeField] MazeManager _mazeManager;
[SerializeField] MelodyMazeUI _ui;
```

- `Start()`: InstructionPanel.Show(...) → OnDismissed += StartGame
- `StartGame()`: _stageManager.OnStageChanged += OnStageChanged; StartFromBeginning()
- `OnStageChanged(int stageIndex)`: _mazeManager.SetupStage(config, stageIndex)
- `OnAllStagesCleared()`: ShowAllClear
- `OnStageClear(int score)` / `OnGameOver(int score)`: UI表示

## MazeManager（コアメカニクス）

### 迷路表現（簡略グリッド）

- グリッドサイズ: Stage1: 5x5, Stage2: 6x6, Stage3: 7x7, Stage4: 7x7, Stage5: 8x8
- セルタイプ: Empty / Path / Junction / NoteNode / Goal / Start
- 入力: **スワイプ検出**（MouseDelta > threshold）→ 方向決定
- 入力は MazeManager に一元管理

### レスポンシブ配置（必須）

```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;   // HUD用
float bottomMargin = 3.0f; // ボタン用
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, maxCellSize);
// 迷路の中央はY=0付近に配置（topMargin-bottomMargin のオフセット調整）
float mazeOriginY = (topMargin - bottomMargin) * 0.5f;
```

### 音符ノードシステム

- 各ノードは `NoteIndex`（0〜11）を持ち、お手本メロディの何番目の音かを示す
- 正解ノード: お手本メロディと一致する音符
- デコイノード（Stage4+）: 似た音程だが不正解のノード
- 動くノード（Stage5）: BPMに合わせてランダムに隣接ノードと入れ替わる

### タイミング判定

- プレイヤーがノードに隣接した時、残り時間窓をUIで表示
- タップ → targetTime との差で判定:
  - Perfect: ±0.05秒
  - Great: ±0.12秒
  - Good: ±0.22秒
  - Miss: それ以外

### スコア計算

```
baseScore = 150pt（正解ノード通過）
comboMultiplier = min(3.0, 1.0 + combo * 0.15)
timingBonus: Perfect=+80, Great=+40, Good=+10, Miss=0 (全てcomboMultiplier適用)
wrongNode: -50pt, コンボリセット
```

### ステージ別パラメータ表

| 項目 | Stage1 | Stage2 | Stage3 | Stage4 | Stage5 |
|------|--------|--------|--------|--------|--------|
| gridSize | 5 | 6 | 7 | 7 | 8 |
| noteCount | 4 | 6 | 8 | 10 | 12 |
| junctionCount | 2 | 4 | 5 | 6 | 7 |
| timeLimit | 60 | 50 | 45 | 40 | 35 |
| previewPlays | ∞ | 5 | 3 | 2 | 1 |
| hasLoop | false | true | true | true | true |
| hasChord | false | false | true | true | true |
| hasDecoy | false | false | false | true | true |
| hasMoving | false | false | false | false | true |

### StageManager統合

```csharp
public void SetupStage(StageManager.StageConfig config, int stageIndex)
{
    // パラメータをstageIndexから決定（configのspeedMultiplierで微調整）
    _gridSize = gridSizes[stageIndex];
    _noteCount = noteCounts[stageIndex];
    _timeLimit = timeLimits[stageIndex] / config.speedMultiplier;
    _maxPreviewPlays = maxPreviews[stageIndex];
    _hasChord = stageIndex >= 2;
    _hasDecoy = stageIndex >= 3;
    _hasMoving = stageIndex >= 4;
    
    GenerateMaze();
    PlaceNoteNodes();
    ResetPlayerPosition();
    _isActive = true;
}
```

### OnStageChanged 購読

```csharp
_stageManager.OnStageChanged += (stageIndex) => {
    var config = _stageManager.GetCurrentStageConfig();
    _mazeManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
};
```

## InstructionPanel内容

```csharp
_instructionPanel.Show(
    "073",
    "MelodyMaze",
    "音符を繋げてメロディを完成させる音楽パズル",
    "スワイプで方向を選んでキャラを進めよう。音符ノードではタイミングよくタップ！",
    "お手本と同じメロディになるルートでゴールを目指そう"
);
```

## ビジュアルフィードバック設計

1. **ノード通過成功**: NoteNodeのSpriteRendererを黄→白のフラッシュ（0.2秒）＋スケールパルス（1.0→1.3→1.0）
2. **ミス/間違いノード**: 赤フラッシュ（0.3秒）＋カメラシェイク（0.12秒）
3. **コンボ達成**: コンボテキストがスケールアップ（1.0→1.4→1.0）＋色変化（白→シアン）
4. **タイミング判定テキスト**: PERFECT=シアン, GREAT=緑, GOOD=黄, MISS=赤 でフロートアップして消える

## スコアシステム

- 正解ノード通過: 150pt × comboMultiplier
- タイミングボーナス: Perfect+80, Great+40, Good+10（× comboMultiplier）
- コンボ倍率: min(3.0, 1.0 + combo × 0.15)
- 間違いノード: -50pt、コンボリセット
- ランク: S(95%以上) / A(80%以上) / B(60%以上) / C(それ以下) ※メロディ一致率ベース

## MelodyMazeUI

必須表示要素:
- StageText: "Stage X / 5"
- ScoreText: スコア数値
- ComboText: コンボカウンター（アニメーション付き）
- TimerText: 残り秒数（カウントダウン）
- PreviewText: お手本残り回数
- JudgementText: PERFECT/GREAT/GOOD/MISS（一時表示）
- StageClearPanel: 「次のステージへ」ボタン
- AllClearPanel: 最終クリア + スコア表示
- GameOverPanel: ゲームオーバー + リトライボタン
- BackButton: メニューへ戻る
- PreviewButton: お手本再生

## SceneSetup (Setup073v2_MelodyMaze.cs)

- `[MenuItem("Assets/Setup/073v2 MelodyMaze")]`
- Camera: backgroundColor=黒系, orthographicSize=6
- Background: rhythm系カラー（シアン/マゼンタ）
- GameManager → StageManager（子）
- GameManager → MazeManager（子）
- Canvas: InputSystemUIInputModule
- HUD: StageText(上左), ScoreText(上右), ComboText(中央上部), TimerText(上中央), PreviewText(上左下)
- PreviewButton: ゲーム画面下部左
- BackButton: 最下部左
- StageClearPanel / AllClearPanel / GameOverPanel: 中央オーバーレイ
- InstructionPanel: 全画面オーバーレイ（最前面）

## 判断ポイントの実装設計

- **分岐点到達時**: `OnPlayerReachedJunction(int junctionId)` → 利用可能な方向を強調表示
  - 各方向の先に待つ音符を「？」で隠す（Stage3以降）
  - 選択報酬: 正解方向 → 150pt ボーナス、不正解 → -50pt
- **タイミング判定トリガー**: ノード到達直後1.5秒間がタップウィンドウ
  - Perfect窓: ±0.05秒（到達から0.75秒後が理想タイミング）
  - 残り時間をノード周囲のリング縮小で可視化

## Buggy Code 防止

- `_isActive` ガード: MazeManager.Update() の先頭で確認
- タグ/レイヤー比較: `gameObject.name` を使用
- Texture2D: `OnDestroy()` で `Destroy(tex)` を実行
- スワイプ入力: フレーム間のマウス移動量 `Mouse.current.delta.ReadValue()` を使用
