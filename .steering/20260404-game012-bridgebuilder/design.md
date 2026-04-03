# Design: Game012v2_BridgeBuilder

## スクリプト構成

```
Scripts/Game012v2_BridgeBuilder/
├── BridgeBuilderGameManager.cs   // 状態管理・StageManager・InstructionPanel統合
├── BridgeManager.cs              // 橋の設計・物理シミュレーション・入力処理
└── BridgeBuilderUI.cs            // UI表示・ボタンイベント
```

- namespace: `Game012v2_BridgeBuilder`

## クラス責務

### BridgeBuilderGameManager.cs
- GameState: WaitingInstruction / Building / Testing / StageClear / Clear / GameOver
- StageManager・InstructionPanel の SerializeField
- Start() → InstructionPanel.Show() → OnDismissed += StartGame
- StartGame() → StageManager.StartFromBeginning()
- OnStageChanged(stage) → BridgeManager.SetupStage(config, stage)
- OnAllStagesCleared() → GameClear表示
- OnTestResult(bool passed, float budgetRatio) → スコア計算
- スコア計算: 基本点(1000×stage) × 予算残ボーナス × コンボ倍率

### BridgeManager.cs
- **建設フェーズ**: 支点ノード管理、パーツ配置・削除
- **テストフェーズ**: 車の移動、橋の崩壊判定（簡易物理）
- 入力処理を一元管理（Mouse.current 使用）
- SetupStage(StageConfig config, int stageIndex) で難易度適用
- スパン数・予算・利用可能パーツ・車重量・風強度を設定
- **橋の物理**: Rigidbody2Dを使わず、シンプルな重み付き崩壊判定
  - パーツに強度値を持たせ、荷重超過でisDestroyed=true
  - 車の進行に合わせてその下のスパンに荷重計算

### BridgeBuilderUI.cs
- 表示: ステージ・スコア・残予算・パーツ一覧ボタン・テストボタン
- ShowStageClearPanel / ShowGameClearPanel / ShowGameOverPanel
- パーツ選択ボタンのハイライト管理

## 盤面・ステージデータ設計

```csharp
// StageManager.StageConfig の利用
// speedMultiplier → 車の速度
// countMultiplier → スパン数
// complexityFactor → 風強度(Stage5)
```

支点配置:
- 左岸、右岸は固定（画面端）
- 中間支点: ステージのスパン数に応じて動的配置
- Y座標: カメラorthographicSizeから計算

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;   // 5.0
float camWidth = camSize * Camera.main.aspect;   // ~2.8
// 渓谷エリア: Y=-1.0 〜 -3.0 (水面)
// 橋の支点Y: Y=0.0 〜 1.5
// 上部マージン(HUD): camSize - 1.0 = 4.0
// 下部マージン(UI): -2.5 以下（Canvas UI領域）
float bridgeY = 0.5f;
float anchorSpacing = (camWidth * 2f) / (spanCount + 1);
```

## InstructionPanel 内容

- title: "BridgeBuilder"
- description: "パーツを組み合わせて車が渡れる橋を作ろう"
- controls: "パーツ選択 → 支点タップ2回で配置 → テストで走行確認"
- goal: "予算内で橋を作り、車を安全に渡らせよう"

## ビジュアルフィードバック設計

1. **パーツ配置成功**: 配置パーツが1.0→1.3→1.0にスケールポップ（0.15秒）
2. **橋の崩壊**: 崩壊パーツが赤フラッシュ → 落下アニメーション
3. **車の通過成功**: ゴール到達時に緑フラッシュ + テキストアニメーション
4. **応力表示**: テスト中、パーツの色を緑（安全）→黄→赤（危険）でグラデーション

## スコアシステム

```
baseScore = 1000 * stageIndex
budgetBonus = budgetRatio >= 0.5 ? 2.0f : budgetRatio >= 0.3 ? 1.5f : 1.0f
comboMul = Mathf.Min(1.0f + (combo - 1) * 0.3f, 3.0f)
stageScore = Mathf.RoundToInt(baseScore * budgetBonus * comboMul)
stars = budgetRatio >= 0.5 ? 3 : budgetRatio >= 0.2 ? 2 : 1
```

## StageManager統合

- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
- OnStageChanged で BridgeManager.SetupStage() を呼び出し
- StageConfig.speedMultiplier → 車速度倍率
- StageConfig.countMultiplier → スパン数倍率
- StageConfig.complexityFactor → 環境負荷（風・重量車両）

## ステージ別新ルール表

| Stage | spanCount | budget | availableParts | 特殊ルール |
|-------|-----------|--------|----------------|-----------|
| 1 | 3 | $500 | 木材のみ | なし（チュートリアル） |
| 2 | 4 | $400 | 木材・鉄骨 | 鉄骨解放 |
| 3 | 5 | $350 | 木材・鉄骨・ロープ | ロープ解放 |
| 4 | 5 | $300 | 全パーツ | 重量車両（荷重2倍） |
| 5 | 6 | $280 | 全パーツ | 風（横方向の揺れ） |

## SceneSetup 構成方針

- Menu: `Assets/Setup/012v2 BridgeBuilder`
- 橋の支点はゲーム実行時に動的生成（SceneSetupでは不要）
- SpritePath: `Assets/Resources/Sprites/Game012v2_BridgeBuilder/`
- 必要スプライト: Background, Anchor, WoodPlank, SteelBeam, Rope, Car, Water, Goal
- GameManager → StageManager, InstructionPanel, BridgeManager, BridgeBuilderUI を配線
- BridgeManager → スプライト参照を配線
- CanvasボタンのonClickイベントを配線

## 判断ポイントの実装設計

### 荷重計算

各スパンの荷重耐性:
- 木材: strength=100, cost=50
- 鉄骨: strength=250, cost=150
- ロープ: strength=180, cost=80（隣接スパンとの連結で強化）

車の荷重:
- 通常車: load=80
- 重量車両(Stage4+): load=160

崩壊判定:
```csharp
if (span.currentLoad > span.strength) span.Collapse();
```

各フレームで車位置のスパンに荷重を加算し、隣接スパンに50%伝播。

## Buggy Code 防止

- Physics2DタグはgameObject.nameで比較
- _isActiveガード: Building/Testingフェーズ以外は入力を無視
- Texture2D/Spriteは動的生成なし（AssetDatabase経由）
- 動的生成したLineRenderer等はOnDestroy()で削除
