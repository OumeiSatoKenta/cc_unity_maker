# Design: Game082v2_AquaPet

## namespace
`Game082v2_AquaPet`

## スクリプト構成

### AquaPetGameManager.cs
- StageManager・InstructionPanel統合のメイン管理
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] AquariumManager _aquariumManager`
- `[SerializeField] AquaPetUI _ui`
- Start()でInstructionPanel.Show()→StartGame()でStageManager.StartFromBeginning()
- OnStageChanged(int stage): AquariumManagerにSetupStage(config, stage)
- OnStageClear(): UIのステージクリアパネル表示
- NextStage(): StageManager.CompleteCurrentStage()
- OnGameOver(): 全魚死亡時に呼ばれる
- UpdateScoreDisplay(int score): UIスコア更新
- UpdateComboDisplay(int combo): UIコンボ更新

### AquariumManager.cs
- コアメカニクス（魚・環境管理）
- フィールド:
  - `[SerializeField] AquaPetGameManager _gameManager`
  - `[SerializeField] AquaPetUI _ui`
  - `[SerializeField] Sprite[] _fishSprites` (魚スプライト配列)
  - `[SerializeField] Sprite _foodSprite`
  - `float _waterQuality = 100f` (0-100)
  - `float _waterTemperature = 22f` (15-30)
  - `float _pH = 7.0f` (5-9)
  - `int _feedCount = 5` (1ラウンドの餌やり回数制限)
  - `int _combo = 0`, `float _comboMultiplier = 1.0f`
  - `int _totalScore = 0`
  - `bool _isActive = false`
  - `List<FishData> _fishList`

- FishDataクラス（内部クラスorサブファイル）:
  - `GameObject go`
  - `SpriteRenderer sr`
  - `string species` (種名)
  - `float health` (0-100)
  - `float hunger` (0-100, 下がると餌が必要)
  - `bool isSick`
  - `bool isDiscovered` (図鑑登録済み)
  - `float optimalTemp` (適正水温)
  - `float optimalPH` (適正pH)
  - `bool isRare`

- SetupStage(StageManager.StageConfig config, int stageIndex)
  - 魚を生成・配置（countMultipler×3匹）
  - 水質悪化速度、餌回数上限を設定
  - ステージ別新ルールを適用

- 魚の配置: Camera.main.orthographicSizeから動的計算
  - 水槽ゲームエリア: Y範囲 = (-camSize + 2.8f) ~ (camSize - 1.5f)
  - 魚はこの範囲内でランダム初期位置

- Update(): 
  - _isActiveガード
  - 水質悪化（deltaTime × 劣化速度）
  - 魚の満腹度低下（deltaTime × 1/秒）
  - 魚の健康度計算（水質・満腹度・適正環境からの計算）
  - 全魚死亡チェック → _gameManager.OnGameOver()
  - 図鑑達成チェック → _gameManager.OnStageClear()

- OnFeedPressed(): 餌やりボタン処理
  - _feedCountが0なら無視
  - 全魚の満腹度+20、スコア+10×生存魚数
  - 複数魚が低満腹度(30以下)なら コンボ発動
  - 餌粒子のビジュアルフィードバック（スケールパルス）

- OnCleanPressed(): 掃除ボタン処理
  - _waterQuality += 30 (上限100)
  - スコア+20
  - 波紋エフェクト（色フラッシュ）

- OnBreedPressed(): 繁殖ボタン処理（ステージ3+）
  - 健康度80%以上の魚ペアが必要
  - 成功→新種誕生、コンボ+1、スコア+50×_comboMultiplier
  - 失敗→コンボリセット

- ビジュアルフィードバック:
  - 餌やり成功: 魚スケールポップ (1.0→1.3→1.0, 0.2秒) + Coroutine
  - 病気/ミス: SpriteRenderer.colorを赤フラッシュ (0.3秒)

- OnDestroy(): Texture2D/Sprite のクリーンアップ

### AquaPetUI.cs
- スコア・健康度・環境表示
- 必須フィールド:
  - `[SerializeField] TextMeshProUGUI _stageText`
  - `[SerializeField] TextMeshProUGUI _scoreText`
  - `[SerializeField] TextMeshProUGUI _comboText`
  - `[SerializeField] TextMeshProUGUI _waterQualityText`
  - `[SerializeField] TextMeshProUGUI _feedCountText`
  - `[SerializeField] TextMeshProUGUI _collectionText`（図鑑進捗）
  - `[SerializeField] Slider _waterQualitySlider`
  - `[SerializeField] Slider _healthSlider`（平均健康度）
  - `[SerializeField] GameObject _stageClearPanel`
  - `[SerializeField] TextMeshProUGUI _stageClearText`
  - `[SerializeField] Button _nextStageButton`
  - `[SerializeField] GameObject _allClearPanel`
  - `[SerializeField] TextMeshProUGUI _allClearScoreText`
  - `[SerializeField] GameObject _gameOverPanel`
  - `[SerializeField] TextMeshProUGUI _gameOverScoreText`

- メソッド:
  - UpdateStage(int current, int total)
  - UpdateScore(int score)
  - UpdateCombo(int combo)
  - UpdateWaterQuality(float quality)
  - UpdateAverageHealth(float health)
  - UpdateFeedCount(int count)
  - UpdateCollection(int collected, int total)
  - ShowStageClear(int stage)
  - HideStageClear()
  - ShowAllClear(int score)
  - ShowGameOver(int score)

## 盤面・ステージデータ設計

```
Stage1 (complexityFactor=0.0): 淡水魚3種, 目標3種収集, 悪化-2/秒
Stage2 (complexityFactor=0.25): +熱帯魚2種, 目標5種, 悪化-3/秒, 適正環境チェック有効
Stage3 (complexityFactor=0.5): 繁殖解放, 目標7種, 悪化-4/秒
Stage4 (complexityFactor=0.75): +海水魚2種, 目標9種, 悪化-5/秒, 病気イベント有効
Stage5 (complexityFactor=1.0): +深海魚, 目標10種, 悪化-6/秒, 特殊繁殖条件
```

## 入力処理フロー
- ボタン操作: UnityEvent (Inspector配線)
- 新Input System使用: `using UnityEngine.InputSystem;`
- ゲームプレイ中ボタン: 餌やり・掃除・繁殖 (AquariumManagerのPublicメソッド)
- 図鑑ボタン: コレクションパネル表示（AquaPetUI）

## SceneSetup構成方針

### Setup082v2_AquaPet.cs

1. Camera: backgroundColor=深海ブルー (0.02f, 0.05f, 0.15f), orthographicSize=6
2. 背景: background.png を水槽背景としてSpriteRenderer
3. AquaPetGameManager GameObjectの生成・StageManager子配置
4. StageConfigs: 5ステージ分設定
5. AquariumManager: 魚スプライト・食べ物スプライト配線
6. Canvas:
   - HUD上部: Stage・Score・Collection表示
   - 中段: WaterQualitySlider・HealthSlider（ゲーム状態表示）
   - 下部ボタン: 餌やり・掃除・繁殖を横並び (Y=80)
   - 最下部: メニューへ戻る (Y=15)
7. InstructionPanel: 別CanvasでsortingOrder=100
8. ステージクリア・全クリア・ゲームオーバーパネル
9. EventSystem: InputSystemUIInputModule

### UIレスポンシブ配置
- ゲームエリア: Y=(-camSize+2.8)〜(camSize-1.5) のワールド座標
- Canvasボタン下部マージン: Y=80以下
- HUDは上部(Y=-30〜-150)

## InstructionPanel内容
- title: "AquaPet"
- description: "水槽で魚を育ててコレクションしよう"
- controls: "餌やり・掃除・繁殖ボタンをタップして魚を管理しよう"
- goal: "レアな魚を繁殖させて図鑑をコンプリートしよう"

## ビジュアルフィードバック設計
1. **餌やり成功**: 餌をもらった魚のtransform.localScaleポップアニメーション (1.0→1.3→1.0, 0.2秒 Coroutine)
2. **掃除成功**: 水槽全体に青いフラッシュ (Camera.backgroundColor色変化, 0.3秒)
3. **病気発症**: 病気の魚のSpriteRenderer.colorを黄色に変化
4. **繁殖成功**: 新魚出現時にスケール(0→1.2→1.0)アニメーション + Score表示ポップ
5. **コンボ発動**: ComboTextのスケールバウンス演出

## スコアシステム
- 餌やり: +10pt × 同時に満腹度が低い魚数
- 掃除: +20pt
- 繁殖成功: +50pt × _comboMultiplier
- _comboMultiplier: 1.0 → 連続繁殖ごとに×1.3 (上限3.0)
- 全魚健康ボーナス: 全魚平均健康度80%以上で毎秒+1pt

## ステージ別新ルール表
| Stage | 新要素 |
|-------|--------|
| 1 | 餌やり・水質管理の基本。全魚同じ最適環境 |
| 2 | 魚種ごとの適正水温が異なる (適正外で健康度-2/秒) |
| 3 | 繁殖ボタン解放。健康度80%以上ペアが必要 |
| 4 | 病気イベント（ランダム1匹が急速に健康度低下） |
| 5 | 特殊繁殖条件（水温+pH両方適正でないと繁殖不可） |

## Buggy Code防止
- 複数Managerが同時動作しないよう _isActive ガード
- 動的生成Texture2D/Sprite は OnDestroy() でDestroyする
- 魚GameObjectはOnDestroy()でDestroy(go)クリーンアップ
- ワールド座標: Camera.main.orthographicSizeから動的計算（ハードコード禁止）
