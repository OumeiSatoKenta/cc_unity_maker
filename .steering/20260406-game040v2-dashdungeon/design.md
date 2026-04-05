# Design: Game040v2_DashDungeon

## namespace

`Game040v2_DashDungeon`

## スクリプト構成

| クラス | ファイル | 責務 |
|--------|---------|------|
| `DashDungeonGameManager` | DashDungeonGameManager.cs | ゲーム状態管理・StageManager/InstructionPanel統合・スコア管理 |
| `DashDungeonMechanic` | DashDungeonMechanic.cs | グリッド生成・プレイヤー移動・敵配置・タイル判定 |
| `DashDungeonUI` | DashDungeonUI.cs | HUD表示・パネル表示・ボタン処理 |

## ゲーム状態

```csharp
enum DashDungeonState { WaitingInstruction, Playing, StageClear, Clear, GameOver }
```

## DashDungeonGameManager

- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] DashDungeonMechanic _mechanic`
- `[SerializeField] DashDungeonUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager.OnStageChanged/OnAllStagesCleared 購読 → StartFromBeginning()
- OnStageChanged(int stage): HideAllPanels → mechanic.SetupStage() → ui 更新
- OnAllStagesCleared(): Clear 状態 → FinalClearPanel 表示
- OnStageClear(int bonus): StageClear 状態 → StageClearPanel
- OnGameOver(): GameOver 状態 → GameOverPanel
- AdvanceToNextStage(), RetryGame(), ReturnToMenu()

## DashDungeonMechanic

### グリッド設計

- `SetupStage(int stage)` でステージパラメータ適用
- グリッドはカメラ座標から動的計算（固定ハードコード禁止）
  ```csharp
  float camSize = Camera.main.orthographicSize; // 5f
  float topMargin = 1.2f; // HUD用
  float bottomMargin = 3.0f; // Canvas ボタン用（4方向ボタンあり）
  float availableHeight = camSize * 2f - topMargin - bottomMargin;
  float camWidth = camSize * Camera.main.aspect;
  float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.0f);
  ```
- グリッド原点: 画面中央（やや上）

### タイルタイプ

```csharp
enum TileType { Floor, Wall, Spike, Exit, Ice, WarpA, WarpB }
```

### 移動ロジック

- プレイヤーが方向を入力すると `SlidePlayer(Vector2Int dir)` を呼ぶ
- 壁か画面端に当たるまで1マスずつ進む（氷床は+2マス追加）
- 着地したマスのタイルタイプを判定:
  - Spike: HP-1、ビジュアルフィードバック（赤フラッシュ）
  - 敵: HP-1、敵消滅（スケールポップアニメ）
  - Exit: StageClear コールバック
  - WarpA/WarpB: 対応するワープ先に瞬間移動
  - Ice: 通常より2マス余分に滑ってから着地

### 入力処理

- 4方向ボタンからコールバックで受け取る（GameManager経由）
- `_isActive` フラグ（移動中・演出中は入力を無視）

### ステージ別パラメータ

| ステージ | グリッドサイズ | HP | 棘 | 敵 | 氷床 | ワープ |
|---------|------------|-----|-----|-----|------|-------|
| 0(Stage1) | 5 | 3 | 0 | 0 | 0 | false |
| 1(Stage2) | 7 | 3 | 2 | 0 | 0 | false |
| 2(Stage3) | 7 | 3 | 2 | 1 | 0 | false |
| 3(Stage4) | 9 | 3 | 3 | 1 | 2 | false |
| 4(Stage5) | 9 | 3 | 3 | 2 | 2 | true |

### ビジュアルフィードバック

1. **プレイヤーダメージ時**: SpriteRenderer.color を赤フラッシュ（0.3秒）+ カメラシェイク（0.15秒、強度0.15f）
2. **出口到達時**: プレイヤーオブジェクトのスケールポップ（1.0→1.4→1.0、0.3秒）+ 出口マス黄色フラッシュ
3. **敵撃破時**: 敵のスケールポップアニメ → 消滅

### 最短手数カウント

- BFS でゲーム開始時に最短手数を事前計算
- `_minMoves` として保持、クリア時のボーナス計算に使用

## DashDungeonUI

### 表示要素

- `_stageText`: "Stage 1 / 5"
- `_hpText`: "HP: ♥♥♥"
- `_movesText`: "手数: 0 (最短: ?)"
- `_scoreText`: "Score: 0"
- StageClearPanel（ステージクリア！ / Score: X / 次のステージへ / メニュー）
- FinalClearPanel（全ステージクリア！ / Score: X / もう一度 / メニュー）
- GameOverPanel（ゲームオーバー / Score: X / もう一度 / メニュー）

## SceneSetup（Setup040v2_DashDungeon.cs）

- `[MenuItem("Assets/Setup/040v2 DashDungeon")]`
- 生成内容:
  - Camera（背景色: ダーク青、orthographic size: 5）
  - 4方向ボタン（上下左右、Canvas 下部に横並び）
  - GameManager（StageManager子 + Mechanic子 + UI子）
  - Canvas（InstructionPanel / StageClearPanel / FinalClearPanel / GameOverPanel）
  - EventSystem（InputSystemUIInputModule）

### 方向ボタン配置（Canvas 下部）

- 上ボタン: anchoredPos (0, 230)、size (150, 55)
- 下ボタン: anchoredPos (0, 120)、size (150, 55)
- 左ボタン: anchoredPos (-160, 175)、size (150, 55)
- 右ボタン: anchoredPos (160, 175)、size (150, 55)
- メニューボタン: anchoredPos (0, 20)、size (200, 55)

### StageManager 統合

- `OnStageChanged`: Mechanic.SetupStage(stage) でグリッド再生成
- `OnAllStagesCleared`: FinalClearPanel 表示

### InstructionPanel 内容

- title: "DashDungeon"
- description: "ダッシュしてダンジョンを攻略しよう"
- controls: "上下左右ボタンで壁まで直進"
- goal: "トラップを避けて出口にたどり着こう"

## スコアシステム

```
ステージクリアボーナス = 100 + 残りHP × 50 - (実際の手数 - 最短手数) × 5
最終スコア = 各ステージのスコアの合計
```

## スプライト一覧（Pillow生成）

| ファイル名 | 内容 | サイズ |
|-----------|------|--------|
| background.png | ダークダンジョン背景 | 128×128 |
| floor.png | 床タイル | 64×64 |
| wall.png | 壁タイル | 64×64 |
| player.png | プレイヤーキャラ | 64×64 |
| enemy.png | 敵キャラ | 64×64 |
| spike.png | 棘トラップ | 64×64 |
| exit.png | 出口タイル | 64×64 |
| ice.png | 氷床タイル | 64×64 |
| warp_a.png | ワープ床A | 64×64 |
| warp_b.png | ワープ床B | 64×64 |

カテゴリ: action → メインカラー #F44336（赤）/ サブ #FF9800（オレンジ）
