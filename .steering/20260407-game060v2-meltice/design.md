# Design: Game060v2_MeltIce

## namespace
`Game060v2_MeltIce`

## スクリプト構成

### MeltIceGameManager.cs
- **責務**: ゲーム全体の状態管理・StageManager/InstructionPanel統合
- **SerializeField**: `StageManager _stageManager`, `InstructionPanel _instructionPanel`, `MeltIceUI _ui`, `Transform _boardContainer`
- **GameState**: Idle / Playing / StageClear / GameClear / GameOver
- **Start()**: InstructionPanelを表示してStartGameをサブスクライブ
- **StartGame()**: StageManager.StartFromBeginning()を呼ぶ
- **OnStageChanged(int)**: ボードを再構築し、UIを更新
- **コンボ/スコア**: _comboMultiplier, _totalScore管理

### MirrorController.cs
- **責務**: グリッド上の鏡オブジェクト管理（配置・回転・描画）
- **鏡の角度**: 0, 45, 90, 135度（45度単位）
- **操作**: ドラッグで配置、タップで45度回転、長押しで回収
- **メソッド**: SetAngle(float), GetReflectionDirection(Vector2), Rotate45()

### LightRaySystem.cs
- **責務**: 太陽から光線をレイキャストしてリアルタイム反射経路を計算・描画
- **LineRenderer使用**: 光線の折れ線描画
- **光の追跡**: 再帰的反射（最大10回）
- **判定**: 氷/壁/プリズム/禁止氷との衝突を処理
- **メソッド**: RecalculateLightPath(), UpdateHitTargets()

### IceBlockController.cs
- **責務**: 氷ブロックの状態（通常/禁止/溶けた/移動中）と描画
- **ターゲット**: IsTarget(bool), IsForbidden(bool)
- **移動**: Stage5用にゆっくり往復移動
- **ビジュアル**: 光が当たると溶けるアニメーション

### MeltIceUI.cs
- **責務**: ステージ・スコア・クリアパネル表示
- **表示**: Stage X/5、Score、残り鏡数、ステージクリアパネル、ゲームオーバーパネル

## 盤面・ステージデータ設計

グリッドサイズ: 7×7（ステージによって有効セル変化）

```csharp
// ステージ設定
struct StageData {
    int gridSize;           // 5〜7
    Vector2Int sunPosition; // 太陽の位置（グリッド外）
    Vector2Int[] iceTargets;   // 青い氷の位置
    Vector2Int[] iceForbidden; // 赤い氷の位置（null or empty）
    Vector2Int[] walls;        // 壁ブロックの位置
    Vector2Int[] prisms;       // プリズムの位置
    int mirrorCount;           // 使用可能な鏡の枚数
    int minMirrors;            // 最適解の鏡数（最少）
    bool hasMobileIce;         // Stage5のみtrue
}
```

### ステージ別パラメータ
| Stage | グリッド | 太陽方向 | 青氷 | 赤氷 | 壁 | プリズム | 鏡数 | 最少鏡 |
|-------|---------|---------|-----|-----|---|--------|-----|------|
| 1 | 5×5 | 上→下 | 1 | 0 | 0 | 0 | 2 | 1 |
| 2 | 5×5 | 上→下 | 2 | 0 | 2 | 0 | 3 | 2 |
| 3 | 6×6 | 左→右 | 3 | 1 | 2 | 0 | 3 | 3 |
| 4 | 6×6 | 上→下 | 3 | 2 | 2 | 1 | 3 | 3 |
| 5 | 7×7 | 上→下 | 4 | 2 | 3 | 1 | 4 | 3 |

## 入力処理フロー
1. `Mouse.current.leftButton.wasPressedThisFrame` → タップ判定
2. `Physics2D.OverlapPoint` → 空きセル or 配置済み鏡を判定
3. ドラッグ開始: `Mouse.current.leftButton.isPressed` + ドラッグ距離閾値
4. 長押し: 0.5秒保持で鏡回収

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;
float bottomMargin = 3.0f;
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.0f);
Vector3 boardOrigin = new Vector3(-cellSize * (gridSize-1) / 2f, camSize - topMargin - cellSize * (gridSize-1) / 2f - cellSize/2f, 0);
```

## InstructionPanel内容
- **title**: "MeltIce"
- **description**: "鏡で太陽光を反射させて氷を溶かそう！"
- **controls**: "鏡をドラッグして配置、タップで45度回転、長押しで回収できるよ"
- **goal**: "全ての青い氷ブロックに光を当ててステージクリア！赤い氷には絶対当てないで！"

## ビジュアルフィードバック
1. **氷が溶けるとき**: スケールパルス(1.0→1.3→0.0) + 白フラッシュ → Destroy
2. **鏡配置確定**: 軽いスケールバウンス(1.0→1.15→1.0、0.15秒)
3. **ゲームオーバー（禁止氷接触）**: 赤フラッシュ全体 + カメラシェイク
4. **ステージクリア**: パーティクル風の光の飛散演出

## スコアシステム
- 基本スコア: `(mirrorCount - usedMirrors) × 200`、最低50
- 最適解ボーナス: `usedMirrors <= minMirrors` → ×2.0
- 連続クリアコンボ: `1.0 + (comboMultiplier - 1) * 0.1`（上限1.5）

## ステージ別新ルール表
- Stage 1: 基本反射のみ
- Stage 2: **壁ブロック**追加（光を遮る）
- Stage 3: **禁止氷（赤）**追加（当てるとGameOver）
- Stage 4: **プリズム**追加（光を2方向に分岐）
- Stage 5: **移動する氷**追加（往復移動、タイミング重要）

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stage) {
    BuildBoard(stage);
    _lightRay.RecalculateLightPath();
    _ui.OnStageChanged(stage + 1);
}
```

## SceneSetup構成方針（Setup060v2_MeltIce.cs）
- MenuItem: `"Assets/Setup/060v2 MeltIce"`
- 背景: 空色グラデーション
- GameManager → StageManager（子）、LightRaySystem（子）、BoardContainer（子）
- Canvas: StageText, ScoreText, MirrorCountText
- InstructionPanel: フルスクリーン
- StageClearPanel: 「ステージクリア！」+「次のステージへ」ボタン
- GameOverPanel: 「ゲームオーバー」+「もう一度」ボタン
- EventSystem: InputSystemUIInputModule
- 全SerializeField配線
