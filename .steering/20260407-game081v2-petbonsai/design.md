# Design: Game081v2 PetBonsai

## namespace
`Game081v2_PetBonsai`

## スクリプト構成

### PetBonsaiGameManager.cs
- namespace: Game081v2_PetBonsai
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] PetBonsaiManager _bonsaiManager
- [SerializeField] PetBonsaiUI _ui
- Start(): InstructionPanel表示 → OnDismissed += StartGame
- StartGame(): StageManager購読、StartFromBeginning()
- OnStageChanged(int): GetCurrentStageConfig() → _bonsaiManager.SetupStage()
- OnAllStagesCleared(): 全クリア表示
- OnStageClear(): ステージクリアパネル表示
- NextStage(): HideStageClear() → CompleteCurrentStage()
- OnGameOver(): ゲームオーバー表示

### PetBonsaiManager.cs
- 盆栽の状態管理（成長ゲージ、美しさスコア、水残量、肥料残量）
- 枝オブジェクトのリスト管理
- 水やり・肥料・剪定のロジック
- 害虫イベント（Stage4+）
- コンボカウンター
- SetupStage(StageManager.StageConfig config, int stageIndex)
  - stageIndex 0〜4 で branchCount, waterNeeded, hasPest を設定
- 入力処理: Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint
- using UnityEngine.InputSystem;
- レスポンシブ配置: Camera.main.orthographicSize から動的計算

#### ステージ別パラメータ表
| Stage | speedMultiplier | countMultiplier | complexityFactor | 枝数 | 水必要回数 | 害虫 |
|-------|----------------|-----------------|-----------------|------|----------|------|
| 1     | 1.0            | 1               | 0.0             | 3    | 3        | なし |
| 2     | 1.0            | 1               | 0.25            | 5    | 4        | なし |
| 3     | 1.0            | 1               | 0.5             | 5    | 4        | なし |
| 4     | 1.2            | 1               | 0.75            | 7    | 5        | あり |
| 5     | 1.2            | 2               | 1.0             | 9    | 5        | あり |

#### 枝の状態
- Normal（通常）: 切ると美しさ+5〜+15（バランス依存）
- Overgrown（伸びすぎ）: 切ると美しさ+20 / 放置でスコア-2/sec
- Pest（害虫付き、Stage4+）: タップで駆除+10、放置でスコア-5/sec

#### 成長ゲージシステム
- 0〜100の成長ゲージ
- 水やりで+15、肥料でその後15秒間効果×1.5
- 成長ゲージが50以上で枝が成長（Overgrownになる可能性）
- 成長ゲージ100達成でステージ目標カウント+1

#### クリア判定
- 水やり必要回数を達成 AND 美しさスコア85以上で品評会ボタン押下でクリア

### PetBonsaiUI.cs
- UpdateBeautyScore(int score): 美しさスコアテキスト更新
- UpdateGrowth(float ratio): 成長ゲージスライダー更新
- UpdateWater(int current, int max): 水残量テキスト更新
- UpdateSeason(string season): 季節テキスト更新
- UpdateCombo(int combo): コンボ表示
- UpdateStage(int stage, int total): ステージ表示
- ShowStageClear(int stage): ステージクリアパネル表示
- HideStageClear(): ステージクリアパネル非表示
- ShowAllClear(int score): 全クリアパネル表示
- ShowGameOver(int score): ゲームオーバーパネル表示
- ShowRivalScore(int rivalScore): ライバルスコア表示（Stage5）
- ShowFeedback(string text, Color color): フィードバックテキスト

## SceneSetup: Setup081v2_PetBonsai.cs
- [MenuItem("Assets/Setup/081v2 PetBonsai")]
- Camera: backgroundColor茶系(和風), orthographicSize=6f
- Background: 和風背景スプライト
- GameManager → StageManager（子）→ PetBonsaiManager（子）
- Canvas + PetBonsaiUI
- StageConfig 5段階設定
- InstructionPanel生成・配線
- ステージクリアパネル生成

## ビジュアルフィードバック設計
1. **剪定成功**: 枝オブジェクトのスケールパルス（1.0→1.4→0.0、0.3秒でフェードアウト）+ 花びら風パーティクル（`SpriteRenderer.color`をピンクに変化）
2. **水やり**: じょうろSpriteの揺れアニメーション（`transform.rotation` ±15度、0.2秒）+ 水滴エフェクト（青いドットが盆栽に向かって落下）
3. **害虫駆除**: 赤フラッシュ後に黒く縮小（`transform.localScale`を0に）
4. **コンボ**: スコアテキストのスケールパルス + 色が金色に変化

## スコアシステム
- 基本: 枝剪定1回+5〜+20（枝の状態による）
- コンボ: 連続剪定成功×3以上で×1.2倍
- ステージクリアボーナス: 美しさスコア × 10
- 害虫駆除: +10pt

## InstructionPanel内容
- title: "PetBonsai"
- description: "盆栽を育てて品評会で優勝しよう"
- controls: "タップで水やり、枝をタップして剪定"
- goal: "美しい盆栽を育てて品評会で★3評価を目指そう"

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
// 盆栽メインオブジェクト: 中央
// 枝オブジェクト: 盆栽を中心に扇型配置（半径1.5〜2.5f）
float treeBaseY = -0.5f; // 中央少し下
// 下部UI: -camSize+2.5f以内
```

## カラーパレット（simulation カテゴリ）
- メイン: #795548（茶）
- サブ: #FF8F00（琥珀）
- アクセント: #EFEBE9（淡茶）
- 和風アクセント: #4CAF50（緑/葉）, #FF6B6B（赤/実）, #FFB6C1（ピンク/花）

## Pythonスプライト生成
出力先: `MiniGameCollection/Assets/Resources/Sprites/Game081v2_PetBonsai/`

必要なスプライト:
1. background.png（640x1280、和風・茶系グラデーション）
2. tree_trunk.png（128x192、木の幹・茶系）
3. branch_normal.png（96x32、枝・緑系）
4. branch_overgrown.png（96x32、伸びすぎ枝・濃緑）
5. branch_pest.png（96x32、害虫付き枝・黄緑+赤点）
6. leaf_cluster.png（64x64、葉の塊・緑系グラデーション）
7. pot.png（128x96、鉢・茶系陶器風）
8. watering_can.png（80x80、じょうろ・青系）
9. fertilizer.png（64x64、肥料袋・琥珀色）
10. pest_icon.png（48x48、害虗・赤系丸型）
11. flower_pink.png（32x32、花びら・ピンク）
12. water_drop.png（24x32、水滴・青透明）
