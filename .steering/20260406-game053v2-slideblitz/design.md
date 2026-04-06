# Design: Game053v2_SlideBlitz

## Namespace
`Game053v2_SlideBlitz`

## スクリプト構成

### SlideBlitzGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] SlideManager _slideManager`
- `[SerializeField] SlideBlitzUI _ui`
- Start(): InstructionPanel表示 → OnDismissed → StartGame()
- StartGame(): _stageManager.StartFromBeginning()
- OnStageChanged(int stage): _slideManager.SetupStage(config), タイマーリセット
- OnAllStagesCleared(): 全クリア演出
- スコア計算: 残り時間 × 100 × コンボ乗算

### SlideManager.cs（コアメカニクス）
- グリッドの生成・管理
- タイル移動処理
- **入力処理一元管理**: マウス/タッチ入力をここで処理
- 入力: `Mouse.current.leftButton.wasPressedThisFrame` + `Mouse.current.position.ReadValue()` + `Mouse.current.leftButton.wasReleasedThisFrame`
- `using UnityEngine.InputSystem;`
- スワイプ判定: PointerDown位置 → PointerUp位置で方向ベクトル計算
- `SetupStage(StageManager.StageConfig config)`: グリッド再生成
- 固定タイル（Stage4）: `isFrozen` フラグ付きのタイル
- パズル完成判定
- コンボカウンター管理
- **レスポンシブ配置**: Camera.main.orthographicSize から動的計算

### TileObject.cs
- 個別タイルのデータ（番号、isFrozen, isBlank）
- SpriteRenderer管理
- ビジュアルフィードバック（コルーチンでポップアニメーション）

### SlideBlitzUI.cs
- タイマー表示（カウントダウン）
- 手数表示
- ステージ表示「Stage X / 5」
- スコア表示
- ステージクリアパネル
- ゲームオーバーパネル
- 最終クリアパネル

## 盤面・ステージデータ設計

| Stage | gridSize | timeLimit | shuffleCount | fixedTiles | stageName |
|-------|----------|-----------|--------------|------------|-----------|
| 1     | 3        | 60        | 20           | 0          | "Stage 1" |
| 2     | 3        | 45        | 40           | 0          | "Stage 2" |
| 3     | 4        | 90        | 50           | 0          | "Stage 3" |
| 4     | 4        | 90        | 50           | 2          | "Stage 4" |
| 5     | 5        | 120       | 60           | 0          | "Stage 5" |

StageManagerのStageConfig使用:
- speedMultiplier: シャッフル深度 (1.0, 2.0, 1.0, 1.0, 1.0)
- countMultiplier: gridSize (3, 3, 4, 4, 5)
- complexityFactor: 固定タイル割合 (0, 0, 0, 0.13, 0)

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;  // 5.0
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 2.8f; // CanvasUI領域
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
float gridWorldSize = Mathf.Min(availableHeight, camWidth * 1.8f);
float cellSize = gridWorldSize / gridSize;
// グリッド中央位置: Y = topMargin/2 - bottomMargin/2 = -0.8
```

## InstructionPanel内容
- title: "SlideBlitz"
- description: "タイルをスライドさせて数字を順番に並べよう！"
- controls: "タイルをタップして空きマスへスワイプ。数字1から順に並べよう"
- goal: "全タイルを正しい位置に並べてステージクリア！"

## ビジュアルフィードバック

### 1. タイル移動成功時（正しい位置に収まった）
- スケールポップ: 1.0 → 1.3 → 1.0（0.15秒）
- 色フラッシュ: 一瞬黄色 → 元の色（0.2秒）

### 2. 固定タイル（動かせない）を操作しようとした時
- シェイク: 左右に小刻みに振れる（0.3秒）
- 色フラッシュ: 赤フラッシュ

### 3. パズル完成時
- 全タイル順番にスケールポップ（ウェーブエフェクト）
- タイル色を金色に変化

## StageManager統合

```csharp
// GameManager.Start()
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

// OnStageChanged(int stage)
void OnStageChanged(int stage)
{
    var config = _stageManager.GetCurrentStageConfig();
    _slideManager.SetupStage(config);
    _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
    StartTimer();
}
```

## SceneSetup構成方針 (Setup053v2_SlideBlitz.cs)

- MenuItem: `[MenuItem("Assets/Setup/053v2 SlideBlitz")]`
- Camera: orthographicSize=5, backgroundColor=カジュアル系（淡緑）
- Canvas: Screen Space Overlay, InputSystemUIInputModule
- GameManager root: SlideBlitzGameManager + 子にStageManager
- SlideManager: GameManagerの子
- InstructionPanel: Canvasの子、フルスクリーンオーバーレイ
- HUD: ステージ/タイマー/手数テキスト（上部）
- ボタン配置:
  - 右下: Resetボタン (150,55)
  - 左下: メニューへ戻るボタン (150,55)
  - 右下下段: ?ボタン（InstructionPanel再表示）
- ステージクリアパネル（「次のステージへ」ボタン付き）
- ゲームオーバーパネル（「リトライ」「メニューへ戻る」ボタン）
- 全クリアパネル

## スコアシステム
- 基本: remainingTime × 100
- コンボ乗算: 正しい位置にタイルが収まるたびにコンボ増加
  - 1-2: ×1.0、3-4: ×1.1、5+: ×1.5
- 効率ボーナス: 手数が最適の1.5倍以内 → 最終スコア×2.0
- スピードボーナス: 半分以内でクリア → ×1.5

## 判断ポイントの実装設計

### トリガー条件
- タイルクリック時: 移動可能かどうか判定
- 移動可能 → アニメーション付き移動
- 移動不可 → シェイクアニメーション（固定or壁）

### 報酬/ペナルティ
- 正しい位置に収まった: コンボ+1、スコア乗算増加
- 誤移動: コンボリセット
- リセット実行: 手数リセット（タイムはそのまま）

## Buggy Code防止チェック
- Physics2D使用なし（グリッドインデックスで管理）
- 各クラスのUpdate()には`_isActive`ガードを実装
- 動的生成Texture2D/SpriteはOnDestroy()でクリーンアップ
- ワールド座標はCamera.orthographicSizeから動的計算
