# Design: Game026v2_SliceNinja

## スクリプト構成

### namespace: `Game026v2_SliceNinja`

| クラス | ファイル | 役割 |
|--------|---------|------|
| SliceNinjaGameManager | SliceNinjaGameManager.cs | ゲーム状態管理、StageManager/InstructionPanel統合 |
| SliceManager | SliceManager.cs | スワイプ軌跡描画、切断判定、物体スポーン管理 |
| FlyingObject | FlyingObject.cs | 飛来物体（フルーツ/爆弾）の挙動・タイプ管理 |
| SliceNinjaUI | SliceNinjaUI.cs | スコア・コンボ・ミス・ステージ表示 |

## クラス詳細

### SliceNinjaGameManager
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] SliceManager _sliceManager`
- `[SerializeField] SliceNinjaUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager購読 → StartFromBeginning()
- OnStageChanged(int stage): SliceManager.SetupStage(config, stage+1) 呼び出し
- OnAllStagesCleared(): State = Clear、UI最終クリアパネル表示
- AddScore(int pts): スコア加算、UI更新
- OnMiss(): ミスカウント、3回でGameOver
- OnBombCut(): 即GameOver

### SliceManager
- スワイプ入力（Mouse.current）でライントレース軌跡を保持
- ドラッグ中: LineRenderer で軌跡描画
- ドラッグ終了: Physics2D.OverlapCircleAll で軌跡点周辺のFlyingObjectを検出→切断
- 物体生成: `Coroutine SpawnLoop()` でspawnIntervalごとにFlyingObject生成
- SetupStage(StageConfig config, int stageNumber): 難易度パラメータ適用
  - spawnInterval, flySpeed, bombRate, frozenRate, miniRate, stealthBombRate
- 物体生成位置: 画面下(-camSize+0.5)、X座標ランダム(-camWidth*0.8 〜 camWidth*0.8)
- 飛来方向: 斜め上方向（物体ごとにランダムangle）

### FlyingObject
- `enum ObjectType { Fruit, Bomb, FrozenFruit, MiniFruit, StealthBomb }`
- Rigidbody2D で飛来（AddForce）
- `int hitCount` (FrozenFruitは2回でSlice完了)
- `bool IsBomb` property
- `void OnSliced(Vector2 sliceDir)`: 切断エフェクト、GameObject破棄
- `void OnFallOff()`: 画面外判定でGameManager.OnMiss()

### SliceNinjaUI
- `void Initialize(SliceNinjaGameManager gm)` 
- `void UpdateScore(int score)`
- `void UpdateCombo(float multiplier)`
- `void UpdateMiss(int count, int max)`
- `void UpdateStage(int stage, int total)`
- `void ShowStageClear(int stage)`
- `void ShowFinalClear(int score)`
- `void ShowGameOver(int score, int maxCombo)`

## 盤面・ステージデータ設計

StageManager.StageConfig 活用:
- speedMultiplier → flySpeed = baseSpeed * speedMultiplier
- countMultiplier → spawnInterval = baseInterval / countMultiplier
- complexityFactor → 特殊オブジェクト率の調整

ステージ別パラメータ:
| Stage | baseSpeed | spawnInterval | bombRate | frozenRate | miniRate | stealthRate |
|-------|-----------|--------------|----------|------------|---------|-------------|
| 1 | 3.0 | 2.0 | 0% | 0% | 0% | 0% |
| 2 | 3.8 | 1.6 | 10% | 0% | 0% | 0% |
| 3 | 4.5 | 1.3 | 10% | 15% | 0% | 0% |
| 4 | 5.5 | 1.1 | 20% | 10% | 20% | 0% |
| 5 | 6.5 | 0.9 | 20% | 5% | 10% | 10% |

## 入力処理フロー
```
Mouse.current.leftButton.wasPressedThisFrame → StartSwipe
Mouse.current.leftButton.isPressed → UpdateSwipe (軌跡追加)
Mouse.current.leftButton.wasReleasedThisFrame → EndSwipe (切断判定)
```

## InstructionPanel内容
- gameId: "026v2"
- title: "SliceNinja"
- description: "飛んでくる物体をスワイプで切れ！爆弾だけは絶対に切るな！"
- controls: "マウスでドラッグしてスワイプ軌跡を描く。軌跡に触れた物体が切断される"
- goal: "ミス3回以内・爆弾を避けながら全5ステージをサバイバルしよう"

## レスポンシブ配置
- camSize = 5, aspect = Camera.main.aspect
- ゲーム領域: 全画面（飛来物はどこでも飛ぶ）
- HUD上部: Y = camSize - 0.6 (ステージ・スコア表示)
- ボタン: Canvas下部 anchored

## ビジュアルフィードバック設計
1. **切断成功**: FlyingObjectが2つに分かれるエフェクト（スケール変化 + フェードアウト）
2. **爆弾切断**: 赤フラッシュ + カメラシェイク（GameOver）
3. **コンボ上昇**: スコアUIのスケールパルス（1.0→1.3→1.0、0.2秒）
4. **ミス発生**: ミスアイコンの赤フラッシュ

## SceneSetup 構成方針
- `[MenuItem("Assets/Setup/026v2 SliceNinja")]`
- スプライト: Background, Fruit, Bomb, FrozenFruit, SliceTrail, MissIcon
- GameManager → StageManager(子), SliceManager(子), SliceNinjaUI(子)
- Canvas → InstructionPanel, HUD(Score/Combo/Miss/Stage), StageClearPanel, FinalClearPanel, GameOverPanel

## StageManager統合
- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
- OnStageChanged: SliceManager.SetupStage()でパラメータ更新、ステージタイマー開始
- ステージクリア: 制限時間経過後 → _stageManager.CompleteCurrentStage()

## スコアシステム
- 基本: 10pt/個
- 1スワイプ複数: 2個=x2, 3個=x3, 4個以上=x5
- 連続コンボ(5秒以内): 連続1=x1.0, 2=x1.5, 3=x2.0, 4以上=x3.0
- ミスでコンボリセット
