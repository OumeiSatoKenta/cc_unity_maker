# Design: Game061v2_CookieFactory

## スクリプト構成

| クラス | 担当 |
|-------|------|
| CookieFactoryGameManager | ゲーム状態、StageManager/InstructionPanel統合 |
| CookieManager | コアメカニクス（生産・設備・コンボ） |
| CookieFactoryUI | UI更新・パネル表示 |

namespace: `Game061v2_CookieFactory`

## クラス詳細

### CookieFactoryGameManager
- Fields: `[SerializeField] StageManager _stageManager`, `[SerializeField] InstructionPanel _instructionPanel`, `[SerializeField] CookieManager _cookieManager`, `[SerializeField] CookieFactoryUI _ui`
- Start(): InstructionPanel.Show → StartGame
- StartGame(): `_stageManager.StartFromBeginning()`
- OnStageChanged(int stage): CookieManager.SetupStage(config)
- OnAllStagesCleared(): UI.ShowGameClear

### CookieManager
- 生産システム: `_cookies`(long), `_autoRate`(float/秒), `_tapPower`(int)
- コンボシステム: `_combo`, `_comboTimer`, ComboThreshold=1秒
  - combo5→×1.5、combo10→×2
- 設備（ShopItem[]）:
  - OvenUpgrade: tapPower+1, コスト100→200→400
  - ConveyorBelt: autoRate+0.5/秒, コスト500→1500→5000（Stage2解放）
  - PackagingMachine: autoRate+3/秒, コスト3000→10000→30000（Stage3解放）
- 特注クッキー（Stage3+）: 受諾→15秒間ライン占有→高単価+500枚
- 設備故障（Stage4）: 確率10%/分で発生、修理タップ5回で復旧、故障中はautoRate×0.3
- VIP注文（Stage5）: 30秒以内に500枚生産→×10ボーナス（失敗ペナルティなし）
- SetupStage(config): ステージ別speedMultiplier、解放設備設定
- 目標到達でGameManager.OnStageClear()呼び出し

### CookieFactoryUI
- フィールド: StageText, CookieText, AutoRateText, ProgressBar, ShopPanel, StageClearPanel, GameClearPanel
- 設備購入ボタン（3種類）を動的に有効/無効化
- コンボテキスト表示（コンボ中のみ）
- VIPOrderパネル（カウントダウン）

## 入力処理フロー
- TapCookieButton.onClick → CookieManager.Tap()
- 各ShopItem購入ボタン → CookieManager.BuyUpgrade(index)
- SpecialOrderButton.onClick → CookieManager.StartSpecialOrder()
- RepairButton.onClick → CookieManager.RepairTap()
- VIPOrderButton.onClick → CookieManager.AcceptVIPOrder()

## StageManager統合
- OnStageChanged購読: CookieManager.SetupStage(config), UI更新
- ステージ別パラメータ（StageManager.StageConfig利用）:
  | Stage | speedMultiplier | 解放機能 |
  |-------|----------------|---------|
  | 1 | 1.0 | なし |
  | 2 | 1.2 | ConveyorBelt |
  | 3 | 1.5 | PackagingMachine, SpecialOrder |
  | 4 | 1.8 | BreakdownEvent |
  | 5 | 2.0 | VIPOrder |
- OnAllStagesCleared購読: GameClear表示

## InstructionPanel内容
- title: "CookieFactory"
- description: "タップでクッキーを焼いて工場を大きくしよう"
- controls: "クッキーをタップして焼く・設備を買って自動化"
- goal: "売上目標を達成して次のステージへ進もう"

## ビジュアルフィードバック設計
1. タップ時: クッキーボタンのScaleパルス（1.0→1.15→1.0、0.15秒）+ 「+N」フローティングテキスト
2. コンボ時: コンボテキストの色変化（橙→赤）+ スケール拡大
3. 設備故障時: 赤フラッシュ + 震えアニメーション
4. VIP注文: カウントダウンバーの色変化（緑→黄→赤）

## スコアシステム
- 基本: クッキー1枚=1点
- コンボ乗算: combo5→×1.5倍、combo10→×2倍
- VIPボーナス: 達成時に現在クッキー×0.1を追加付与（×10の表現）

## SceneSetup構成方針（Setup061v2_CookieFactory.cs）
- MenuItem: "Assets/Setup/061v2 CookieFactory"
- カメラ背景: 暖色系ベージュ (#FFF8E1)
- Canvas構成:
  - 上部: StageText（Stage 1/5）、売上テキスト
  - 中央: 大きなクッキーボタン（背景円形）
  - 右側: AutoRateText（/秒）
  - 下部左: ShopPanel（縦リスト）
  - 最下部: MenuButton
- スプライト生成（Python Pillow）:
  - Cookie.png (128x128): 茶色グラデーション円
  - Oven.png (96x96): 赤みのあるグラデーション四角
  - ConveyorBelt.png (96x96): グレーグラデーション
  - PackagingMachine.png (96x96): 紫グラデーション
  - Background.png (256x256): 淡い黄色グラデーション

## Buggy Code防止チェック
- `_isActive` ガードを Update() に実装
- Texture2D → OnDestroy でクリーンアップ（Destroy(tex)）
- ワールド座標不要（全てCanvas UI）
- 目標到達チェックは Update() 内で毎フレーム実施
