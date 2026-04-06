# Design: Game065v2 SpellBrewery

## スクリプト構成

### SpellBreweryGameManager.cs
- namespace: `Game065v2_SpellBrewery`
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] BreweryManager _breweryManager`
- `[SerializeField] SpellBreweryUI _ui`
- Start(): InstructionPanel.Show → OnDismissed += StartGame
- StartGame(): StageManager 購読 + StartFromBeginning()
- OnStageChanged(int): BreweryManager.SetupStage(config, stage)
- OnAllStagesCleared(): UI.ShowAllClear()
- OnStageClear(): StageClear状態 → UI.ShowStageClear()
- NextStage(): stageManager.CompleteCurrentStage()

### BreweryManager.cs
- namespace: `Game065v2_SpellBrewery`
- ゲームのコアロジック全体
- **材料種類**: Fire, Water, Earth, Air, Light（5種）
- **ポーション種類**: 8種（FirePotion, WaterPotion, EarthPotion, AirPotion, LightPotion, StormPotion, NaturePotion, LegendaryPotion）
- **レシピ**: 
  - Fire+Water → FirePotion (30G)
  - Water+Earth → WaterPotion (30G)
  - Earth+Air → EarthPotion (30G)
  - Air+Light → AirPotion (30G)
  - Fire+Air → LightPotion (50G)
  - Fire+Water+Air → StormPotion (120G)
  - Water+Earth+Light → NaturePotion (150G)
  - Fire+Water+Earth+Air+Light → LegendaryPotion (2000G) ← Stage5解放
- **材料在庫**: int[5] (最大20個/種)
- **釜の中身**: List<IngredientType>（最大5スロット）
- **ポーション在庫**: int[8]
- `SetupStage(StageManager.StageConfig, int)`: ステージ設定
- `TapIngredient(IngredientType)`: 材料を釜に追加
- `Brew()`: 醸造実行（コルーチン2秒）
- `SellPotion(PotionType)`: 1個販売
- `SellAll()`: 全ポーション販売
- 注文システム: stage4以降、30秒ごとに注文が来る
- 自動収集: Coroutine（ステージ別速度）
- レスポンシブ配置: Camera.orthographicSizeから動的計算
- ビジュアルフィードバック:
  - 醸造成功: 釜オブジェクトのスケールパルス（1.0→1.4→1.0, 0.3秒）+ 金色フラッシュ
  - 失敗: 赤フラッシュ + カメラシェイク
  - 材料タップ: 小スケールパルス（1.0→1.2→1.0, 0.1秒）
  - コンボ継続: テキスト色変化（白→黄→オレンジ→赤）

### SpellBreweryUI.cs
- namespace: `Game065v2_SpellBrewery`
- ゴールド表示、目標表示
- Stage X/5 表示
- 材料在庫ボタン群（IngredientType別、タップで釜に投入）
- 釜の中身表示（スロット5つ）
- 醸造ボタン、全販売ボタン
- ポーション在庫表示
- コンボ表示
- 注文パネル（stage4以降）
- ステージクリアパネル、全クリアパネル
- FloatingText演出

## InstructionPanel内容
- title: "SpellBrewery"
- description: "材料を集めて魔法のポーションを作ろう"
- controls: "材料をタップして釜に投入、醸造ボタンでポーション完成、販売してゴールド獲得"
- goal: "ポーション販売目標を達成してステージクリア"

## ステージ別パラメータ表

| Stage | speedMultiplier | countMultiplier | complexityFactor |
|-------|----------------|----------------|-----------------|
| 1 | 0.0 (自動なし) | 1 | 0.0 |
| 2 | 1.0 | 1 | 0.1 |
| 3 | 1.4 | 2 | 0.2 |
| 4 | 2.0 | 3 | 0.3 |
| 5 | 3.0 | 5 | 0.5 |

※ complexityFactor が 0 = 自動収集なし

## SceneSetup 構成方針

`Setup065v2_SpellBrewery.cs` を `Assets/Editor/SceneSetup/` に作成

### シーン構造
- Camera (Background: 0.05, 0.02, 0.1 ← 深紫の夜空)
- Background スプライト (sortingOrder: -10)
- Cauldron オブジェクト (ワールド座標、画面中央)
- GameManager
  - StageManager（子）
  - BreweryManager（子）
- Canvas (RenderMode: Overlay, 1080×1920)
  - InstructionPanel
  - HUD (Stage / Gold / Target)
  - IngredientButtons（横並び、下部）
  - CauldronSlots（釜の中身表示）
  - BrewButton / SellAllButton
  - PotionInventory
  - ComboText
  - OrderPanel
  - StageClearPanel
  - AllClearPanel

### UI配置（Canvas 1080×1920基準）
- StageText: 上部中央 (Y=-30)
- GoldText: 上部左 (Y=-80)
- TargetText: 上部右 (Y=-80)
- ComboText: 中央上 (Y=-180)
- CauldronSlotsPanel: 中央 (Y=-500)
- PotionInventoryPanel: 中央下 (Y=-900)
- IngredientButtons: 下部横並び (Y=750)
- BrewButton: 下部 (Y=650, 200×80)
- SellAllButton: 下部 (Y=560, 200×80)
- BackToMenuButton: 最下部 (Y=15, 250×55)

### 配線リスト（全て漏れなく）
- gm._stageManager ← sm
- gm._instructionPanel ← ip
- gm._breweryManager ← bm
- gm._ui ← ui
- bm._gameManager ← gm
- bm._ui ← ui
- bm._cauldronObj ← cauldronObj (BreweryManagerのSerializeField)
- bm._ingredientSprites ← Sprite[5]
- bm._potionSprites ← Sprite[8]
- bm._bgSprite ← bgSprite (背景)
- ui._gameManager ← gm
- ui._breweryManager ← bm
- ui スコアテキスト群 ← 各TextMeshProUGUI
- ui ボタン群 ← 各Button
- ui パネル群 ← 各GameObject

## スコアシステム
- 基本ポーション価格: 30〜2000G
- コンボ倍率: 1x(0連), 1.5x(2連), 2x(3連), 3x(5連), 5x(8連+)
- 注文ボーナス: 指定ポーション作成で2倍
- コンボ切れ条件: 失敗醸造 or 30秒間未醸造

## ビジュアルフィードバック設計
1. **醸造成功**: CauldronオブジェクトのスケールPulse (1.0→1.4→1.0, 0.3秒) + color金色フラッシュ
2. **醸造失敗**: SpriteRendererを赤に0.3秒 + CameraShake（0.15f振幅、0.5秒）
3. **材料タップ**: 材料ボタンの小Pulse (0.1秒)
4. **販売**: FloatingText（+XXG）が画面中央に出て上昇フェード

## 判断ポイントの実装設計
- 釜スロット5個が埋まると自動で醸造ヒントを表示（「レシピのヒント: ○○を試して」）
- Stage4の注文: 30秒タイマー、成功で2倍G、タイムアウトで次の注文
- Stage5: 伝説ポーションは全材料5種×1個必要（材料を貯めるか消費するかの判断）

## Buggy Code防止メモ
- コルーチン(_autoCoroutine等)は SetupStage時に既存を必ずStop
- BrewCoroutineはbrewing中はBrewボタンを無効化
- _isActive ガードを Update/コルーチンに付ける
- OnDestroy でイベント解除と動的オブジェクトDestroy
- Sprite/Texture2D は AssetDatabase 経由でロード（動的生成しない）
