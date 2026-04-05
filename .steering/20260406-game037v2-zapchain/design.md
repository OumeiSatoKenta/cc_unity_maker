# Design: Game037v2_ZapChain

## namespace
`Game037v2_ZapChain`

## スクリプト構成

### ZapChainGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] ZapMechanic _zapMechanic`
- `[SerializeField] ZapChainUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed → StartGame()
- StartGame(): StageManager.StartFromBeginning()
- OnStageChanged(int stage): ZapMechanic.SetupStage(config)
- OnAllStagesCleared(): Final clear
- スコア: comboCount, scoreMultiplier, totalScore
- public: OnNodeConnected(int chainLength), OnChainCompleted(bool isFullClear), OnEnergyEmpty(), AdvanceToNextStage(), RetryGame(), ReturnToMenu()

### ZapMechanic.cs
- ノード生成・管理・入力処理
- `[SerializeField] ZapChainGameManager _gameManager`
- ノードタイプ: Normal, Obstacle, Moving, Timed
- **入力**: Mouse.current (InputSystem)
  - Press: Physics2D.OverlapPoint でノード検出→チェーン開始
  - Hold + Move: Physics2D.OverlapPoint でドラッグ先ノード検出→チェーン延長
  - Release: チェーン確定
- SetupStage(StageManager.StageConfig config): ノード数・タイプ・エネルギー設定
- GenerateNodes(int count, float maxDistance): ランダム配置（重複回避）
- AddToChain(NodeObject node): 隣接判定・追加
- FinalizeChain(): 接続確定・エネルギー消費
- DrawConnections(): LineRendererで接続線描画
- **レスポンシブ配置**:
  ```csharp
  float camSize = Camera.main.orthographicSize;
  float camWidth = camSize * Camera.main.aspect;
  float topMargin = 1.2f;
  float bottomMargin = 2.8f;
  // ノードはこの矩形内にランダム配置
  ```
- **ビジュアルフィードバック**:
  - 接続成功: ノードスケールパルス (1.0→1.3→1.0, 0.2秒) + 黄色フラッシュ
  - エネルギー低下: SpriteRenderer色を赤に変化
  - コンボ高: ノードに電撃エフェクト（スケール + シアン色）

### NodeObject.cs
- SpriteRenderer, CircleCollider2D
- NodeType enum: Normal, Obstacle, Moving, Timed
- 移動ノード: 円軌道を周回 (FixedUpdate)
- 時限ノード: CountdownTimer → 自己消滅
- 接続済み: SpriteRenderer色変更、スケールアニメーション

### ZapChainUI.cs
- スコア・エネルギー・チェーン数・ステージ表示
- UpdateScore(int score)
- UpdateEnergy(float energy, float maxEnergy)
- UpdateChain(int current, int total)
- UpdateStage(int stage)
- ShowStageClearPanel() / ShowFinalClearPanel() / ShowGameOverPanel()

## ステージパラメータ表
| Stage | nodeCount | hasDistanceLimit | maxDistance | obstacleCount | movingCount | timedCount | energyMax |
|-------|-----------|-----------------|-------------|---------------|-------------|------------|-----------|
| 1 | 5 | false | 999 | 0 | 0 | 0 | 100 |
| 2 | 8 | true | 2.5 | 0 | 0 | 0 | 100 |
| 3 | 10 | true | 2.5 | 2 | 0 | 0 | 120 |
| 4 | 12 | true | 2.5 | 1 | 2 | 0 | 120 |
| 5 | 15 | true | 2.5 | 1 | 2 | 3 | 150 |

StageManager.StageConfig の speedMultiplier: 1.0/1.2/1.5/1.8/2.0
countMultiplier（ノード数の係数として使用）
complexityFactor（障害物・特殊ノードの割合）

## InstructionPanel 内容
- title: "ZapChain"
- description: "電撃を連鎖させて全ノードを接続しよう"
- controls: "ノードをタップしてドラッグで隣接ノードへ連鎖"
- goal: "一筆書きで全ノードを接続しよう"

## スコアシステム
- 1接続 = 10pt × comboMultiplier
- comboMultiplier: chain 1-4=1x, 5-9=2x, 10+=3x
- 一筆書き完成ボーナス: +500pt
- エネルギー切れ: スコア変化なし

## ビジュアルフィードバック
1. **接続成功**: ノードスケールパルス (1.0→1.3→1.0, 0.2s) + 黄色フラッシュ
2. **エネルギー低下(<30%)**: SpriteRenderer色が赤に変化、Sliderが赤

## SceneSetup: Setup037v2_ZapChain.cs
- MenuItem: `"Assets/Setup/037v2 ZapChain"`
- Scene: `Assets/Scenes/037v2_ZapChain.unity`
- 構成:
  - Camera (orthographic, size=5)
  - Background (Quad, blue gradient sprite)
  - NodeContainer (empty parent)
  - ConnectionRenderer (LineRenderer コンポーネント付き)
  - GameManager (ZapChainGameManager + StageManager child)
  - Canvas
    - HUD: Stage Text, Score Text, Energy Slider, Chain Text
    - StageClearPanel
    - FinalClearPanel
    - GameOverPanel
    - ReturnButton (常時表示)
  - InstructionPanel
- 全フィールドをSerializedObjectで配線
- EventSystem (InputSystemUIInputModule)

## StageManager統合
- `_stageManager.OnStageChanged += OnStageChanged` → ZapMechanic.SetupStage(config)
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared` → Final clear表示
- OnStageChanged: ノードを全削除→新規生成

## 判断ポイント実装
- 開始ノード選択: どのノードからでも開始可能、ただし行き詰まるとリセット必要
- 接続距離制限: 隣接判定 `Vector2.Distance(a.pos, b.pos) < maxDistance`
- 時限ノード: `timedCountdown` が0になると `Destroy()` → 未接続なら即ゲームオーバー候補

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 2.8f; // Canvas UI領域
float areaTop = camSize - topMargin;
float areaBottom = -camSize + bottomMargin;
float areaLeft = -camWidth + 0.5f;
float areaRight = camWidth - 0.5f;
// ノードをこの矩形内にランダム配置
```
