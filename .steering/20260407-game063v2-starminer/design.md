# Design: Game063v2_StarMiner

## namespace
`Game063v2_StarMiner`

## スクリプト構成

### StarMinerGameManager.cs
- ゲーム状態管理（Playing / StageClear / AllClear）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] MiningManager _miningManager`
- `[SerializeField] StarMinerUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame → StartGame() で _stageManager.StartFromBeginning()
- OnStageChanged(int stage): _miningManager.SetupStage(config)
- OnAllStagesCleared(): AllClear処理

### MiningManager.cs
- コアメカニクス管理
- 鉱石カウント、資金、ドリルレベル、ドローン数を管理
- `SetupStage(StageManager.StageConfig config)`: ステージパラメータ適用
- 入力処理: `Mouse.current.leftButton.wasPressedThisFrame` で星をタップ検出
- コンボシステム: 1秒以内の連続タップでcombo++、1秒経過でリセット
- タップ採掘量 = baseTapAmount × drillLevel × comboMultiplier
- 自動採掘: coroutineで_droneCountに応じて定期採掘（ステージ2以降）
- 小惑星嵐イベント（ステージ4以降）: 定期的に発生、シールドなしで採掘量半減
- 目標達成チェック: 累計採掘量 >= target → _gameManager.OnStageClear()
- クリーンアップ: OnDestroy()でTexture2D等の動的生成リソースをDestroy

### StarMinerUI.cs
- スコア・資金・ドリルレベル・ドローン数表示
- Stage X/5 表示
- コンボ表示
- アップグレードボタン（ドリル強化・ドローン購入・星系開拓）
- ステージクリアパネル（次へボタン）
- 全クリアパネル
- 嵐警告表示

## 盤面設計
- 星オブジェクト（スプライト）を画面中央に配置
- タップで採掘アニメーション（スケールパルス）
- ステージごとに星の外観が変わる

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6
float topMargin = 1.2f; // HUD用
float bottomMargin = 3.0f; // ボタン用
// 星の中心位置
float starY = (camSize - topMargin - (camSize - topMargin - bottomMargin) * 0.5f) - topMargin;
```
星オブジェクトは画面中央〜上部に配置（下部3uはUIボタン用）

## InstructionPanel内容
- title: "StarMiner"
- description: "宇宙で鉱石を掘って宇宙船を強化しよう"
- controls: "タップで採掘、ボタンでアップグレード"
- goal: "採掘目標を達成して新しい星系を開拓しよう"

## ビジュアルフィードバック
1. タップ成功時: 星のスケールパルス（1.0 → 1.2 → 1.0、0.15秒）
2. コンボ時: 星の色フラッシュ（黄→白→元の色）
3. レア鉱石出現時: パーティクル風のきらめき（スケール1.5倍で短時間表示）
4. 嵐発生時: カメラシェイク + 赤色フラッシュ

## スコアシステム
- タップ採掘量 = 1 × drillLevel × combo倍率
- combo倍率: 1連=×1, 2連=×1.5, 3連=×2, 4連=×3, 5連以上=×5
- スコア = 累計採掘量（ステージクリア進捗として使用）
- 売却レート: 通常鉱石=1G/個、レア鉱石=10G/個

## ステージ別パラメータ（StageManager.StageConfig使用）
| Stage | target | speedMultiplier | countMultiplier | 新要素 |
|-------|--------|----------------|----------------|--------|
| 1 | 100 | 1.0 | 1.0 | 基本タップ |
| 2 | 500 | 1.2 | 1.5 | ドローン解放（speed=drone rate）|
| 3 | 2000 | 1.5 | 2.0 | レア鉱石（complexityFactor=rare chance）|
| 4 | 8000 | 2.0 | 3.0 | 小惑星嵐 |
| 5 | 30000 | 3.0 | 5.0 | 伝説の星系 |

## SceneSetup構成 (Setup063v2_StarMiner.cs)
- MenuItem: "Assets/Setup/063v2 StarMiner"
- 背景: 宇宙背景スプライト（SpriteRenderer）
- 星オブジェクト: 画面中央やや上（y≈1.5）
- GameManager root GameObject
  - StageManager (child)
  - MiningManager (child)
- Canvas (ScreenSpaceOverlay, 1080x1920)
  - HUD: StageText (top-center), OreText (top-left), FundText (top-right)
  - ComboText (center)
  - UpgradePanel (bottom): DrillBtn, DroneBtn, StarBtn横並び
  - BackToMenuButton (bottom-left)
  - StageClearPanel (center overlay, hidden)
  - AllClearPanel (center overlay, hidden)
  - StormWarning (center, hidden)
- InstructionPanel: フルスクリーンオーバーレイ
- EventSystem: InputSystemUIInputModule

## 配線（SerializeField）
GameManager: _stageManager, _instructionPanel, _miningManager, _ui
MiningManager: _gameManager, _ui, _starTransform（星オブジェクト）
StarMinerUI: 各TextMeshPro、各パネル、各ボタン

## アセット一覧
- Background.png（宇宙背景 512x512）
- Star1.png（鉄の星 128x128）
- Star2.png（銀の星 128x128）
- Star3.png（ダイヤの星 128x128）
- Star4.png（嵐の星 128x128）
- Star5.png（伝説の星 128x128）
- DrillIcon.png（ドリルアイコン 64x64）
- DroneIcon.png（ドローンアイコン 64x64）
- OreIcon.png（鉱石アイコン 64x64）
