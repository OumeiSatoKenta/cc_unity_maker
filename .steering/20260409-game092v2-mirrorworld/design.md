# Design: Game092v2_MirrorWorld

## namespace
`Game092v2_MirrorWorld`

## スクリプト構成

### MirrorWorldGameManager.cs
- ゲーム状態管理（Playing / StageClear / GameOver / AllClear）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] MirrorPuzzleManager _puzzleManager`
- `[SerializeField] MirrorWorldUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager.StartFromBeginning()
- OnStageChanged(int stageIndex): PuzzleManager.SetupStage(config, stageIndex)
- OnAllStagesCleared(): AllClear表示
- OnBothReachedGoal(int movesUsed): スコア計算、StageClearパネル表示
- OnTrapHit(): GameOver処理
- NextStage(): _stageManager.CompleteCurrentStage()
- RestartGame(): スコアリセット、StartFromBeginning()

### MirrorPuzzleManager.cs
- コアメカニクス: グリッドベース鏡像移動パズル
- `[SerializeField] MirrorWorldGameManager _gameManager`
- `[SerializeField] MirrorWorldUI _ui`
- Spriteフィールド: _sprBackground, _sprWall, _sprTrap, _sprGoal, _sprSwitch, _sprDoor, _sprPlayerTop, _sprPlayerBot
- SetupStage(StageConfig config, int stageIndex): ステージデータ初期化、グリッド生成
- bool _isActive ガード
- 入力処理: Mouse.current.leftButton.wasPressedThisFrame + ドラッグ方向検出
- 鏡像ロジック: topキャラdr→botキャラ(-dr)、topキャラdc→botキャラ(+dc)
- スイッチ踏んだらドアを開ける
- Stage5: ターンごとに障害物が移動
- OnDestroy(): Texture2D等のリソースクリーンアップ

### MirrorWorldUI.cs
- UpdateStage(int stage, int total)
- UpdateScore(int score)
- UpdateMoves(int used, int limit)
- UpdateCombo(int combo, float multiplier)
- ShowStageClear(int stageNum, int score)
- HideStageClear()
- ShowGameOver(int score)
- HideGameOver()
- ShowAllClear(int score)

## 盤面・ステージデータ設計

静的マップデータをクラス内に埋め込み:
```csharp
enum TileType { Empty, Wall, Goal, Trap, Switch, Door, Start }
```

Stage 1 (5x5, noLimit):
```
Top: S....
     .....
     ..W..
     .....
     ....G
Bot: (鏡像対称) start=(0,4), goal=(4,0)
```

Stage 2 (5x5, limit=15):
- 非対称壁配置、片方だけ壁にぶつかる

Stage 3 (6x6, limit=12):
- トラップタイル追加

Stage 4 (6x6, limit=10):
- スイッチ＆ドア追加

Stage 5 (7x7, limit=8):
- 移動障害物追加（ターンごとに移動）

## レスポンシブ配置（必須）

```csharp
float camSize = Camera.main.orthographicSize;  // 6f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD
float bottomMargin = 2.8f; // UIボタン
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// 上下2ゾーンに分割: mirrorLine=0
float zoneHeight = availableHeight / 2f;
float cellSize = Mathf.Min(zoneHeight / gridSize, camWidth * 2f / gridSize, 0.9f);
float topZoneCenter = camSize - topMargin - zoneHeight/2f;
float botZoneCenter = -(camSize - bottomMargin - zoneHeight/2f);
```

## InstructionPanel内容
- gameId: "092"
- title: "MirrorWorld"
- description: "鏡の世界で2人同時にゴールさせよう"
- controls: "スワイプで移動（上下キャラが鏡像で連動）"
- goal: "両方のキャラをゴールに導こう"

## StageManager統合

StageConfig パラメータの使い方:
- speedMultiplier: 未使用（ターン制なので）
- countMultiplier: 使用しない
- complexityFactor: ステージ識別に stageIndex を直接使用

OnStageChanged(int stageIndex) → MirrorPuzzleManager.SetupStage(config, stageIndex)

Stage configs:
```
Stage 0: speedMultiplier=1.0, countMultiplier=1, complexityFactor=0.0, stageName="Stage 1"
Stage 1: speedMultiplier=1.0, countMultiplier=1, complexityFactor=0.3, stageName="Stage 2"
Stage 2: speedMultiplier=1.0, countMultiplier=1, complexityFactor=0.5, stageName="Stage 3"
Stage 3: speedMultiplier=1.0, countMultiplier=1, complexityFactor=0.7, stageName="Stage 4"
Stage 4: speedMultiplier=1.0, countMultiplier=1, complexityFactor=1.0, stageName="Stage 5"
```

## ビジュアルフィードバック設計

1. **成功時（両ゴール到達）**: 両プレイヤーのスケールパルス (1.0 → 1.4 → 1.0, 0.3秒) + 黄色フラッシュ
2. **ミス時（トラップ接触）**: 赤フラッシュ (SpriteRenderer.color → 赤 → 白) + カメラシェイク (0.2秒)
3. **壁バウンド時**: 軽いスケールバウンス (0.9 → 1.1 → 1.0, 0.15秒)
4. **スイッチ踏んだ時**: ドアオブジェクトのフェードアウト

## スコアシステム
- ベーススコア: 100 × (stageIndex+1)
- 手数ボーナス: (制限手数 - 使用手数) × 10 (制限なし時は+20)
- ノーミスボーナス: +50
- 壁バウンド技術ボーナス: バウンド回数 × 20
- コンボ倍率: 連続ステージクリアで×1.2

## SceneSetup構成方針

Setup092v2_MirrorWorld.cs:
- MenuItem: "Assets/Setup/092v2 MirrorWorld"
- クラス名: Setup092v2_MirrorWorld (static)
- スプライト保存先: Assets/Resources/Sprites/Game092v2_MirrorWorld/
- Spriteファイル: background.png, wall.png, trap.png, goal_top.png, goal_bot.png, switch.png, door.png, player_top.png, player_bot.png
- GameManager → StageManager(子), PuzzleManager(子)
- Canvas: InstructionPanel, HUD(ステージ/スコア/手数/コンボ), StageClearPanel, GameOverPanel, AllClearPanel
- BottomButtons: BackToMenuButton, NextStageButton(StageClearPanel内)
- MirrorLineオブジェクト（画面中央の境界線）

## 判断ポイントの実装設計
- ユーザーがスワイプするたびに両キャラの新位置を計算し表示
- トラップ隣接時は予告として赤い枠を表示（Stage3以降）
- スイッチの踏み忘れ防止：スイッチタイルをハイライト表示
