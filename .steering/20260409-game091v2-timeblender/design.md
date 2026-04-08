# Design: Game091v2 TimeBlender (Remake)

## スクリプト構成

| クラス | ファイル | 役割 |
|-------|---------|------|
| TimeBlenderGameManager | TimeBlenderGameManager.cs | ゲーム状態管理・スコア・InstructionPanel/StageManager統合 |
| PuzzleManager | PuzzleManager.cs | タイルグリッド管理・時代切替・キャラ移動・パラドックス判定 |
| TimeBlenderUI | TimeBlenderUI.cs | HUD・パネル表示・ボタンイベント管理 |

**namespace**: `Game091v2_TimeBlender`

## タイルグリッド設計

```
エラ (Era) 列挙体:
  Past = 0, Present = 1, Future = 2

タイルタイプ:
  Empty = 空き(通過可)
  Wall = 壁(通過不可)
  Bridge = 橋(過去でWall、未来でEmpty)
  Tree = 木(過去でEmpty、未来でWall/Object)
  Goal = ゴール
  Player = プレイヤー開始位置
  Paradox = パラドックスゾーン(特定組み合わせで矛盾発生)
  Start = スタート位置
```

## ステージデータ設計

各ステージはハードコードされたタイルマップを使用（5ステージ×過去/未来の2状態）。

Stage 1 (4x4, 2時代):
- 過去: 通路があるが右側が壁
- 未来: 左側が壁になるが右側に通路が開く
- 解法: 過去→右へ→未来切替→下へ→ゴール

Stage 2 (5x5, 2時代):
- 橋ギミック: 過去で「木」タイルをある条件で通過すると未来で「橋」になる
- 複数切替が必要

Stage 3 (5x5, 2時代):
- 因果連鎖: 3つのオブジェクトが連動（Aを動かすとBが変わる）

Stage 4 (6x6, 2時代):
- パラドックスゾーン: 特定タイル組み合わせで残り許容数-1

Stage 5 (6x6, 3時代):
- 過去・現在・未来の3ボタン、3状態のマップを使い分け

## 入力処理フロー

```
TimeBlenderUI → PastButton/FutureButton/PresentButton タップ
  → TimeBlenderGameManager.OnEraChangeRequested(era)
    → PuzzleManager.SwitchEra(era)
      → タイルマップ切替 + キャラ位置検証
      → パラドックスチェック
      → ビジュアルフィードバック

Canvas タップ → TimeBlenderUI.OnTileClicked(gridPos)
  → PuzzleManager.TryMovePlayer(gridPos)
    → 隣接チェック + 通過可能チェック
    → 成功: キャラ移動 + ゴール判定
    → 失敗: 赤フラッシュ
```

## SceneSetup構成方針

- MenuItem: `Assets/Setup/091v2 TimeBlender`
- クラス名: `Setup091v2_TimeBlender`
- ファイル: `Assets/Editor/SceneSetup/Setup091v2_TimeBlender.cs`

生成順序:
1. Camera (orthographicSize=6, 背景色=深い紺色)
2. Background sprite
3. GameManager + StageManager (子)
4. PuzzleManager (GameManager子)
5. Canvas + CanvasScaler + GraphicRaycaster
6. HUD テキスト群 (Stage, Score, Moves, Paradox残り, Combo)
7. 時代インジケーター (現在の時代テキスト)
8. 時代切替ボタン (過去/未来/現在)
9. StageClearPanel, AllClearPanel, GameOverPanel
10. InstructionPanel (フルスクリーンオーバーレイ)
11. HelpButton ("?")
12. BackToMenuButton
13. EventSystem + InputSystemUIInputModule
14. 全フィールド配線
15. シーン保存 + BuildSettings追加

## StageManager統合

```csharp
// GameManager.Start()
_instructionPanel.Show("091", "TimeBlender", "過去と未来を切り替えて謎を解こう", 
    "ボタンで時代切替、タップで移動", "時代の変化を利用してゴールに到達しよう");
_instructionPanel.OnDismissed += StartGame;

// StartGame()
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

// OnStageChanged(int stageIndex)
var config = _stageManager.GetCurrentStageConfig();
_puzzleManager.SetupStage(config, stageIndex);
```

## StageConfigパラメータ表

| Stage | speedMultiplier | countMultiplier | complexityFactor | 意味 |
|-------|----------------|-----------------|-----------------|-----|
| 1 | 1.0 | 1 | 0.0 | 4x4グリッド、手数制限なし |
| 2 | 1.0 | 1 | 0.3 | 5x5グリッド、手数制限20 |
| 3 | 1.0 | 1 | 0.5 | 5x5グリッド、手数制限15 |
| 4 | 1.0 | 1 | 0.8 | 6x6グリッド、手数制限12 |
| 5 | 1.0 | 1 | 1.0 | 6x6グリッド、3時代、手数制限10 |

- `complexityFactor`: 0.0=4x4/制限なし, 0.3=5x5/制限20, 0.5=5x5/制限15, 0.8=6x6/制限12, 1.0=6x6/3時代/制限10
- countMultiplierは使用しないが、gridSizeはcomplexityFactorから計算

## InstructionPanel内容

```
title: "TimeBlender"
description: "過去と未来を切り替えて謎を解こう"
controls: "ボタンで時代切替、タップで移動"
goal: "時代の変化を利用してゴールに到達しよう"
```

## ビジュアルフィードバック設計

1. **時代切替成功**: 全タイルが0.15秒でスケールパルス(1.0→1.1→1.0) + 時代ごとの色フラッシュ
   - 過去: 暖色(橙/茶)フラッシュ
   - 未来: 寒色(青/シアン)フラッシュ
   - 現在(Stage5): 緑フラッシュ

2. **移動成功**: プレイヤースプライトがポップアニメーション(1.0→1.3→1.0, 0.2秒)

3. **移動失敗**: タイルの赤フラッシュ(0.3秒)

4. **パラドックス発生**: 画面全体の紫フラッシュ + パラドックスカウンター更新

5. **ゴール到達**: スケール拡大 + 金色フラッシュ

6. **コンボ**: ComboTextのスケールバウンス

## スコアシステム

- 移動: なし（パラドックス発生なしでコンボ継続）
- コンボ x1.0 (0-2連続), x1.5 (3-4), x2.0 (5+)
- ステージクリア: 残り手数 × 10pt
- 時代切替なしでゴール: +50pt
- ステージ基本スコア: 100pt × ステージ番号

## レスポンシブ配置設計

```
camSize = 6.0
camWidth = camSize * aspect

topMargin = 1.5f     // HUD領域
bottomMargin = 3.0f  // UIボタン領域
availableHeight = (camSize * 2) - topMargin - bottomMargin  // = 7.5

グリッドサイズ (gridN):
  Stage 1-2: 4-5 → cellSize = availableHeight / gridN
  Stage 3-5: 5-6 → cellSize = availableHeight / gridN
  maxCellSize = 1.5f
```

## 配線漏れチェック (SceneSetup)

- [ ] gm._stageManager = sm
- [ ] gm._instructionPanel = instructionPanel
- [ ] gm._puzzleManager = puzzleManager
- [ ] gm._ui = ui
- [ ] ui._stageText
- [ ] ui._scoreText
- [ ] ui._moveText
- [ ] ui._paradoxText
- [ ] ui._comboText
- [ ] ui._eraText
- [ ] ui._pastButton
- [ ] ui._futureButton
- [ ] ui._presentButton (Stage5用、初期非表示)
- [ ] ui._stageClearPanel
- [ ] ui._stageClearScoreText
- [ ] ui._nextStageButton → gm.NextStage()
- [ ] ui._allClearPanel
- [ ] ui._allClearScoreText
- [ ] ui._gameOverPanel
- [ ] ui._gameOverScoreText
- [ ] ui._retryButton → gm.RestartGame()
