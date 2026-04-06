# Design: Game071v2 BeatTiles

## namespace
`Game071v2_BeatTiles`

## スクリプト構成
- `BeatTilesGameManager.cs` - ゲーム状態管理・StageManager/InstructionPanel統合
- `NoteManager.cs` - ノーツ生成・落下・判定処理
- `BeatTilesUI.cs` - UI表示（スコア・コンボ・ライフ・判定テキスト）

## クラス設計

### BeatTilesGameManager.cs
- SerializeField: StageManager, InstructionPanel, NoteManager, BeatTilesUI
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager購読 → StartFromBeginning()
- OnStageChanged(int): NoteManager.SetupStage(config, stageIndex)
- OnAllStagesCleared(): 全クリア処理
- OnStageClear(): UI.ShowStageClear()
- NextStage(): StageManager.CompleteCurrentStage()
- GameOver(): UI.ShowGameOver()

### NoteManager.cs
- 4レーンのノーツ管理（Lane 0-3）
- BPM同期タイマーでノーツ生成
- StageConfig活用: speedMultiplier=BPM倍率, countMultiplier=ノーツ密度, complexityFactor=特殊ノーツ割合
- ノーツ種別: Normal / Hold / Rapid（ステージで解禁）
- 入力処理: Mouse.current（各レーンタップ領域）
- 判定: Perfect(±30ms)/Great(±80ms)/Good(±150ms)/Miss
- レスポンシブ配置: Camera.main.orthographicSize基準
  - 上部マージン: HUD用 (camSize - 1.2)
  - ゲーム領域中央: 4レーン等分
  - 下部マージン: 判定ライン + ボタン領域 (2.8u)
  - 判定ライン: 下端から 2.8u の位置
- コンボ・スコア計算
- ライフ管理: 最大100、Miss=-8、Perfect+=1
- ビジュアルフィードバック:
  1. ノーツタップ成功: スケールパルス(1.0→1.3→1.0, 0.2秒) + 色フラッシュ
  2. Miss: カメラシェイク + 赤フラッシュ
- ステージ終了判定: 全ノーツ処理後

### BeatTilesUI.cs
- UpdateScore(int score)
- UpdateCombo(int combo) - コンボアニメーション
- UpdateLife(float life)
- ShowJudgement(string text, Color color) - 0.3秒フェード
- ShowStageClear(int stage)
- ShowAllClear(int finalScore)
- ShowGameOver(int score)
- UpdateStage(int current, int total)

## InstructionPanel内容
- title: "BeatTiles"
- description: "リズムに合わせてタイルをタップしよう"
- controls: "判定ラインにノーツが重なったらタップ！Perfectを狙ってコンボを繋げよう"
- goal: "5ステージを全てクリアしてリズムマスターになろう"

## ビジュアルフィードバック
1. タップ成功: ノーツのスケールパルス (1.0→1.3→1.0、0.2秒)
2. Miss: カメラシェイク（0.15秒） + レーン背景赤フラッシュ

## スコアシステム
- Perfect: 100pt × (1.0 + combo×0.1, max 3.0)
- Great: 70pt × (1.0 + combo×0.05, max 2.0)
- Good: 30pt × 1.0
- Miss: 0pt、コンボリセット

## ステージ別新ルール
| Stage | BPM | 新要素 |
|-------|-----|-------|
| 1 | 90×speed | 2レーン・単押しのみ |
| 2 | 110×speed | 4レーン解放・同時押し登場 |
| 3 | 130×speed | 長押し（ホールド）ノーツ追加 |
| 4 | 150×speed | 連打ノーツ追加 |
| 5 | 170×speed | 全要素複合・裏拍配置 |

## SceneSetup配線
- BeatTilesGameManager
  - _stageManager → StageManager
  - _instructionPanel → InstructionPanel
  - _noteManager → NoteManager
  - _ui → BeatTilesUI
- NoteManager
  - _laneSprites[4] → 各レーン背景スプライト
  - _noteSprite → ノートスプライト
  - _holdSprite → ホールドスプライト
  - _gameManager → BeatTilesGameManager

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
float bottomMargin = 2.8f; // 判定ライン + Canvas UI
float topMargin = 1.2f;    // HUD領域
float gameHeight = camSize * 2f - topMargin - bottomMargin;
// 4レーン横並び
float laneWidth = camWidth * 2f / 4f;
```
