# Design: Game068v2 CloudFarm

## namespace
`Game068v2_CloudFarm`

## スクリプト構成

| ファイル | 担当 |
|---------|------|
| `CloudFarmGameManager.cs` | ゲーム状態管理、StageManager/InstructionPanel統合 |
| `FarmManager.cs` | コアメカニクス（畑・作物・天候・市場・害虫） |
| `CloudFarmUI.cs` | HUD・パネル表示管理 |

## 盤面・ステージデータ設計
- 畑マス: `PlotCell[]` (最大12マス、2列 × 6行のグリッド)
- ステージごとに使用可能マス数: 6→8→10→12→12
- 作物定義 `CropData`: name, growthTime, basePrice
  - にんじん: 30s, 50G
  - キャベツ: 50s, 80G
  - トマト: 80s, 120G
  - スターメロン: 120s, 1000G (Stage5のみ)

## クラス別詳細

### CloudFarmGameManager
```
フィールド:
  [SerializeField] StageManager _stageManager
  [SerializeField] InstructionPanel _instructionPanel
  [SerializeField] FarmManager _farmManager
  [SerializeField] CloudFarmUI _ui

Start():
  _instructionPanel.Show("068", "CloudFarm", ...)
  _instructionPanel.OnDismissed += StartGame

StartGame():
  _stageManager.OnStageChanged += OnStageChanged
  _stageManager.OnAllStagesCleared += OnAllStagesCleared
  _stageManager.StartFromBeginning()

OnStageChanged(int stage):
  _farmManager.SetupStage(config, stage)
  _ui.UpdateStage(stage+1, 5)

OnAllStagesCleared(): _ui.ShowAllClear(totalCoins)
```

### FarmManager
```
GetComponentInParent<CloudFarmGameManager>()

フィールド:
  [SerializeField] CloudFarmGameManager _gameManager
  [SerializeField] Camera _camera
  PlotCell[] _plots
  int _stageIndex
  float _speedMul, _countMul, _complexityFactor

状態:
  bool _isActive
  long _coins
  long _totalEarned  // 累計出荷額
  long _stageTarget

  // 天候 (Stage2+)
  enum Weather { Sunny, Rainy, Stormy }
  Weather _weather
  float _weatherTimer
  float _weatherDuration (ランダム10〜30s)

  // 市場価格 (Stage3+)
  float _marketMultiplier (0.5〜2.0)
  float _marketTimer

  // 害虫 (Stage4+)
  float _pestTimer
  bool[] _hasPest  // 各マスに害虫いるか

  // コンボ
  int _harvestCombo
  float _comboTimer

  // インベントリ
  int _inventory (収穫済み作物の合計価値)

  // 自動収穫 (Stage2+)
  bool _autoHarvestUnlocked
  float _autoHarvestTimer
  float _autoHarvestInterval (3.0s / speedMul)

  // アップグレードレベル
  int _autoHarvestLevel (0〜3)
  int _growthBoostLevel (0〜2)

Input:
  Mouse.current.leftButton.wasPressedThisFrame
  → OverlapPoint でクリックされたマスを判定 (各PlotCell に Collider2D)
  → 空きマス: 選択中の種を植える
  → 成熟マス: 収穫
  → 害虫マス: 駆除

Update():
  各マスの成長進行
  天候タイマー
  市場価格タイマー
  害虫タイマー
  コンボタイマー
  自動収穫タイマー
  累計チェック→ステージクリア判定
```

### CloudFarmUI
```
UpdateStage(int stage, int total)
UpdateCoins(long coins)
UpdateInventory(int value)
UpdateWeather(Weather w)
UpdateMarketPrice(float mul)
UpdatePestAlert(bool active)
UpdateStageProgress(long earned, long target)
UpdateAutoRate(float perSec)
ShowStageClear(int stage)
ShowAllClear(long totalCoins)
UpdateHarvestCombo(int combo)
UpdateSelectedCrop(string name)
```

## StageManager統合
- `OnStageChanged` で `FarmManager.SetupStage(config, stageIndex)` を呼ぶ
- SetupStage で畑マス数・天候有効化・市場有効化・害虫有効化・プレミアム作物解放を切り替える
- ステージ別パラメータ:
  | Stage | speedMul | countMul | complexityFactor |
  |-------|---------|---------|-----------------|
  | 0 | 1.0 | 1.0 | 0.2 |
  | 1 | 1.3 | 1.5 | 0.4 |
  | 2 | 1.6 | 2.0 | 0.6 |
  | 3 | 2.0 | 3.0 | 0.8 |
  | 4 | 2.5 | 4.0 | 1.0 |

## InstructionPanel内容
- title: "CloudFarm"
- description: "雲の上の農場で作物を育てて出荷しよう"
- controls: "タップで種まき・収穫、ボタンで出荷"
- goal: "出荷目標を達成してステージクリア"

## ビジュアルフィードバック設計
1. **収穫パルス**: 作物収穫時に `transform.localScale` が 1.0→1.4→1.0 (0.2秒コルーチン)
2. **害虫アラート**: 害虫出現マスに赤フラッシュ (`SpriteRenderer.color` が赤→白)
3. **出荷ボーナス演出**: 高値出荷時にスコアテキストが黄色でフラッシュ

## スコアシステム
- 基本スコア = 作物の基本価格 × 市場倍率
- 高値ボーナス: 市場倍率 >= 1.5 のとき × 1.5
- 収穫コンボ: 連続収穫のたびにコンボ+1、次の収穫で +combo*5コインボーナス
- コンボリセット: 1秒間収穫しないとリセット

## ステージ別新ルール表
| Stage | 新要素 |
|-------|--------|
| 1 | 基本のみ（手動収穫、3作物、6マス） |
| 2 | 自動収穫＋天候システム（雨/晴/嵐） |
| 3 | 市場価格変動＋コンパニオンプランティング |
| 4 | 害虫イベント（タップ駆除） |
| 5 | プレミアム作物（スターメロン）解放 |

## 判断ポイントの実装設計
- **出荷タイミング**: 市場倍率が1.5以上のとき出荷ボタンが金色に光る → 視覚的に高値を通知
- **コンパニオン**: 隣接チェックは4方向（上下左右）で同種CropTypeなら収穫時に+30%

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6.0
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f; // HUD用
float bottomMargin = 3.0f; // Canvas UI用
float availableH = camSize * 2f - topMargin - bottomMargin;
// 畑グリッド: 2列 × maxRows行
// cellSize = availableH / rows を基本に、横方向にはみ出ないよう min取得
```

## SceneSetup 構成方針
- `[MenuItem("Assets/Setup/068v2 CloudFarm")]`
- カメラ: orthographicSize=6, 空色(0.5, 0.8, 1.0)
- 畑グリッド: 2列×6行のSpriteRenderer (PlotCell×12を生成し、使用可能数はFarmManagerで管理)
- 各PlotCellに BoxCollider2D を付ける
- GameManager → FarmManager, StageManager を子に配置
- Canvas: InstructionPanel, StageClearPanel, AllClearPanel, HUD, MarketPanel, ShopPanel
- EventSystem: InputSystemUIInputModule
