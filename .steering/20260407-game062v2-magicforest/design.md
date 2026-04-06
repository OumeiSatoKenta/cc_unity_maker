# Design: Game062v2_MagicForest

## namespace
`Game062v2_MagicForest`

## スクリプト構成

| クラス | ファイル | 責務 |
|--------|---------|------|
| MagicForestGameManager | MagicForestGameManager.cs | 状態管理・StageManager統合・InstructionPanel |
| ForestManager | ForestManager.cs | 木・苗管理、タップ入力、成長ロジック、嵐、動物、世界樹 |
| MagicForestUI | MagicForestUI.cs | 魔力・面積・ステージ・コンボ・クリアパネル表示 |

## ゲーム状態
`Idle → Playing → StageClear → (次ステージ) → GameClear`

## ForestManager 設計

### グリッド配置
- 7x10 グリッド（カメラ座標から動的計算）
- `Camera.main.orthographicSize` からセルサイズ計算
- bottomMargin = 3.0f（UI領域確保）

```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;
float bottomMargin = 3.0f;
float availableH = camSize * 2f - topMargin - bottomMargin;
float availableW = camWidth * 2f;
float cellSize = Mathf.Min(availableH / rows, availableW / cols, 1.1f);
```

### 木の状態
- `Empty`: 空き地
- `Sapling`: 苗（小スプライト）
- `TreeOak`: オーク（デフォルト）
- `TreeBirch`: バーチ（ステージ2解放）
- `TreePine`: パイン（ステージ3解放）
- `WorldTree`: 世界樹（ステージ5特殊）
- `Withered`: 枯れた木（嵐後）

### 入力処理
```csharp
// 一元管理: ForestManagerのUpdate()内
if (Mouse.current.leftButton.wasPressedThisFrame)
{
    Vector2 wp = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    // グリッドセルを特定してタップ処理
}
```

### 木の成長フロー
1. 空きセルをタップ → 苗が出現（魔力コスト0、ただし上限まで）
2. 苗をタップ → 木に成長（魔力10消費）または自動成長
3. 木をタップ → 魔力+5 + コンボボーナス
4. 周囲に空きがあれば確率で苗を自動生成（自動成長購入後）

### ステージ設定 (StageConfig活用)
- speedMultiplier → 自動成長速度係数
- countMultiplier → 目標面積
- complexityFactor → 嵐確率 / 世界樹有効フラグ

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stage)
{
    var config = _stageManager.GetCurrentStageConfig();
    _forestManager.SetupStage(stage, config);
    _ui.UpdateStageDisplay(stage + 1);
}
```

## カスタムStageConfig設定
```csharp
var configs = new StageManager.StageConfig[]
{
    new() { speedMultiplier=0f,   countMultiplier=10,  complexityFactor=0f,  stageName="Stage 1" },
    new() { speedMultiplier=0.2f, countMultiplier=25,  complexityFactor=0f,  stageName="Stage 2" },
    new() { speedMultiplier=0.4f, countMultiplier=50,  complexityFactor=0.3f,stageName="Stage 3" },
    new() { speedMultiplier=0.6f, countMultiplier=80,  complexityFactor=0.6f,stageName="Stage 4" },
    new() { speedMultiplier=0.8f, countMultiplier=120, complexityFactor=1.0f,stageName="Stage 5" },
};
_stageManager.SetConfigs(configs);
```

## InstructionPanel内容
- title: "MagicForest"
- description: "木を育てて魔法の森を広げよう"
- controls: "木をタップして育てる、魔力でアップグレードを購入"
- goal: "森の面積を目標まで広げてステージクリア"

## ビジュアルフィードバック
1. **木をタップ時**: スケールパルス（1.0 → 1.4 → 1.0、0.3秒） + 魔力テキストポップ
2. **木が成長時**: 緑フラッシュ（SpriteRenderer.color）
3. **嵐中の枯れ**: 赤フラッシュ → 枯れスプライトに変換
4. **コンボ3以上**: 複合演出（スケール + ゴールド色）

## スコアシステム
- 基本スコア: 木をタップで+5魔力
- コンボ: 2秒以内の連続タップでコンボカウント UP
- コンボ乗算: combo 1-2→x1、3-5→x1.5、6-9→x2、10+→x3
- ステージクリア時にコンボボーナスを加算

## ステージ別新ルール表
- Stage 1: タップのみ。オーク1種。自動成長なし。
- Stage 2: 自動成長購入ボタン解放。バーチ出現（ランダムに生える）。
- Stage 3: 動物訪問システム ON。20秒ごとにシマリスが訪問、苗1本をボーナス苗（即成長済み）に変換。
- Stage 4: 嵐イベント ON。30秒ごとに10秒間嵐発生。嵐中は木が毎秒10%で枯れる。タップして保護。
- Stage 5: 世界樹システム ON。3種全部植えると世界樹マスが出現。世界樹は30秒で完成。完成で全クリア。

## SceneSetup 配線

### SetupすべきSerializedField一覧
MagicForestGameManager:
- _stageManager (StageManager)
- _instructionPanel (InstructionPanel)
- _forestManager (ForestManager)
- _ui (MagicForestUI)

MagicForestUI:
- _stageText (TMP)
- _manaText (TMP)
- _areaText (TMP)
- _comboText (TMP)
- _autoGrowBtn (Button) - ステージ2から表示
- _autoGrowCostText (TMP)
- _stageClearPanel (GameObject)
- _stageClearText (TMP)
- _nextStageBtn (Button)
- _gameClearPanel (GameObject)
- _gameClearText (TMP)
- _retryBtn (Button)
- _gameManager (MagicForestGameManager)

ForestManager:
- _gameManager (MagicForestGameManager)
- _ui (MagicForestUI)

## Buggy Code 防止
- タグ・レイヤー比較は `gameObject.name` or tag string を使用
- `_isActive` フラグで複数Updateの同時実行を防止
- Texture2D/Sprite は OnDestroy でクリーンアップ
- グリッド座標はカメラorthographicSizeから動的計算（固定値禁止）
