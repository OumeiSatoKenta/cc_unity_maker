# Design: Game064v2_AquaCity

## namespace
`Game064v2_AquaCity`

## スクリプト構成

### AquaCityGameManager.cs
- SerializeField: StageManager, InstructionPanel, CityManager, AquaCityUI
- Start(): InstructionPanel.Show → OnDismissed += StartGame
- StartGame(): stageManager.StartFromBeginning()
- OnStageChanged(int): CityManager.SetupStage(config, stage)
- OnAllStagesCleared(): UI.ShowAllClear()
- OnStageClear(): UI.ShowStageClear()
- NextStage(): stageManager.CompleteCurrentStage()

### CityManager.cs
- 3x3グリッド（最大9マス）
- 建物の購入・配置・タップ回収
- 自動収入コルーチン（Stage2以降）
- 隣接ボーナス計算（Stage3以降）
- サメ襲来イベント（Stage4以降）
- 深海エリア（Stage5）
- 魚タップコンボ
- SetupStage(StageConfig, int stage)
- 入力: Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint
- レスポンシブ: Camera.main.orthographicSize から動的計算

### AquaCityUI.cs
- UpdatePopulation, UpdateCoins, UpdateStage
- ShowStageClear, ShowAllClear, ShowGameOver
- ShowSharkWarning(bool)
- ShowDeepSeaUnlocked()
- UpdateCombo(int)
- UpdateAutoRate(float)
- ショップパネル（建物購入ボタン）

## 盤面設計
- 3x3グリッド（9マス）
- カメラ orthographicSize=6
- camWidth = 6 * aspect
- ゲーム領域（中央）: Y range [-1.5, 3.5]
- グリッド中心: (0, 1.0)
- セルサイズ: 約1.6u
- 下部マージン: 3.0u（Canvas UIボタン用）

## 建物種別
| 建物 | コスト | 収入/秒 | 人口 | 解放ステージ |
|------|--------|---------|-----|------------|
| 住宅 (House) | 0(初期) | 2 | 5 | 1 |
| 広場 (Plaza) | 10 | 5 | 10 | 1 |
| 珊瑚礁 (Coral) | 20 | 8 | 15 | 1 |
| デコ (Deco) | 30 | 3 | 5, 魚出現+20% | 2 |
| 水族館 (Aquarium) | 50 | 15 | 25 | 3 |
| 深海基地 (DeepBase) | 200 | 50 | 100 | 5 |

## InstructionPanel内容
- title: "AquaCity"
- description: "海底に都市を作って魚を集めよう"
- controls: "建物をタップしてコイン回収、ボタンで建物を購入"
- goal: "人口目標を達成して次のステージへ進もう"

## ビジュアルフィードバック
1. **建物タップ**: スケールパルス 1.0→1.25→1.0（0.2秒）+ コイン浮き上がりテキスト
2. **魚タップ**: SpriteRenderer色フラッシュ（黄色、0.15秒）+ コンボカウント表示
3. **サメ出現**: カメラシェイク + 赤フラッシュ（警告）
4. **隣接ボーナス発動**: 緑フラッシュ + テキスト "+Bonus!"

## スコアシステム
- 人口 = Σ(建物.人口) + 隣接ボーナス
- コンボ: 魚を連続タップ（1.5秒以内）でx1〜x5
- コインコンボ: コンボ倍率 * 基本コイン

## ステージ別新ルール
- Stage 1: 住宅・広場・珊瑚礁の3種のみ、手動コイン回収
- Stage 2: デコレーション解放 + 自動コイン回収（1秒ごと）開始
- Stage 3: 隣接ボーナス解放（同種建物が隣接すると収入1.5倍）
- Stage 4: サメ襲来（20秒ごと）: 5秒以内にタップで撃退しないと魚コインが減少
- Stage 5: 深海エリア解放（DeepBase建物が使えるようになる）+ 水族館解放

## SceneSetup構成
- Assets/Setup/064v2 AquaCity (MenuItem)
- カメラ背景色: 深海青 (0.02, 0.05, 0.15)
- Background sprite (海底)
- 3x3グリッド GameObject（CityManager に配置データ）
- GameManager > StageManager, CityManager
- Canvas > UI要素すべて
- InstructionPanel: フルスクリーンオーバーレイ
- EventSystem + InputSystemUIInputModule

## 配線フィールド一覧
GameManager: stageManager, instructionPanel, cityManager, ui
CityManager: gameManager, ui, buildingPrefabs(6種のSprites)
AquaCityUI: populationText, coinsText, stageText, autoRateText, comboText,
            sharkWarning, stageClearPanel, allClearPanel, shopPanel,
            houseButton, plazaButton, coralButton, decoButton, aquariumButton, deepBaseButton,
            nextStageButton, menuButton

## 判断ポイント実装
- 「建物を買う」判断: 10〜30秒ごとにコインが貯まる → プレイヤーが購入選択
- 隣接ボーナス: どのマスに置くかで1.5倍収入の差 → 戦略的配置
- サメ: 5秒のタイムリミット、撃退失敗でコイン-50

## Buggy Code 防止
- _isActive ガードを CityManager.Update() に適用
- Texture2D は OnDestroy でクリーンアップ
- 固定座標ハードコーディング禁止 → orthographicSize から動的計算
