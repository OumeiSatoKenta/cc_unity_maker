# Design: Game024v2_BubblePop

## namespace
`Game024v2_BubblePop`

## スクリプト構成

### BubblePopGameManager.cs
- ゲーム状態管理: WaitingInstruction / Playing / StageClear / Clear / GameOver
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] BubbleController _controller`
- `[SerializeField] BubblePopUI _ui`
- ライフ管理（初期3）
- コンボ管理（同色連続タップカウント）
- フィーバー状態管理（20連続破裂で10秒間）
- スコア集計

### BubbleController.cs
- バブルの生成・移動・削除を管理
- 入力処理: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint`
- バブルプールまたは動的生成（上限20個）
- `SetupStage(StageManager.StageConfig config, int stageNumber)` でステージパラメータ適用
- バブル種類: Normal(3色), Iron(鉄), Split(分裂), Ghost(透明)
- 上端到達検出でGameManagerに通知
- **レスポンシブ配置**: カメラのorthographicSizeから計算
  - バブル出現X範囲: `[-camWidth*0.8, camWidth*0.8]`
  - 出現Y: 下端 + 0.5u
  - 上端判定Y: `camSize - 1.0f` (HUD領域考慮)

### BubblePopUI.cs
- スコア・ライフ・コンボ・フィーバーゲージ表示
- ステージ表示「Stage X / 5」
- StageClearPanel, ClearPanel, GameOverPanel制御
- フローティングボーナステキスト

## 盤面・ステージデータ設計

### StageManager統合
- `OnStageChanged` → `BubbleController.SetupStage(config, stageNumber)`
- `OnAllStagesCleared` → ゲームクリア処理

### ステージ別パラメータ表
| Stage | spawnInterval | riseSpeed | stageDuration | colorCount | ironRatio | splitRatio | ghostRatio |
|-------|--------------|-----------|--------------|-----------|----------|-----------|-----------|
| 1 | 1.8s | 1.2 | 30s | 1 | 0% | 0% | 0% |
| 2 | 1.4s | 1.5 | 30s | 3 | 0% | 0% | 0% |
| 3 | 1.2s | 1.8 | 60s | 3 | 15% | 0% | 0% |
| 4 | 1.0s | 2.2 | 60s | 3 | 15% | 20% | 0% |
| 5 | 0.8s | 2.7 | 60s | 3 | 15% | 20% | 10% |

StageManagerの`speedMultiplier`をriseSpeedとして利用、`countMultiplier`をspawnInterval短縮に使用。

## InstructionPanel内容
- title: "BubblePop"
- description: "浮かんでくるバブルをタップして割ろう！"
- controls: "バブルをタップして破裂させよう。同じ色を連続タップで連鎖ボーナス！"
- goal: "ライフを守りながら全5ステージをクリアしよう"

## ビジュアルフィードバック設計

### 1. バブル破裂ポップアニメーション
- `transform.localScale` 1.0 → 1.4 → 0（0.25秒）Coroutine
- 通常バブル: Scaleup + 透明化して消滅

### 2. 鉄バブルのヒットフラッシュ
- `SpriteRenderer.color` → 白フラッシュ（0.1秒）
- 2回目でポップアニメーション

### 3. ライフ消失エフェクト
- カメラシェイク（0.3秒、強度0.3）
- HUDのライフアイコンが赤フラッシュ

### 4. フィーバーモード演出
- 背景色変化 + 「FEVER!!」テキスト点滅
- スコアテキストが金色に変化

### 5. コンボ表示
- コンボ数増加時にスケールパルス（1.0 → 1.3 → 1.0, 0.15秒）

## スコアシステム
- 基本: Normal=10pt, Iron=30pt, Split本体=25pt, Ghost=20pt
- 同色連鎖倍率: 2連=x1.5, 3連=x2.0, 4連+=x3.0
- スピードボーナス: 出現1秒以内=+20pt
- フィーバー中: 全スコア×2
- フィーバー発動: 20連続破裂（色問わず）

## SceneSetup構成方針 (Setup024v2_BubblePop.cs)

### 生成順序
1. Camera（背景色: 水色グラデーション風の暗い青 #0D1B2A）
2. Background.png スプライト
3. GameManager root
   - StageManager（子）
   - BubbleController（子）
4. Canvas
   - HUD: StageText, ScoreText, LifeText, ComboText
   - FeverGauge（Sliderまたはカスタム）
   - BonusText（非アクティブ）
   - StageClearPanel
   - ClearPanel
   - GameOverPanel
   - InstructionPanel（最前面）
   - MenuButton, ReShowInstructionBtn
5. EventSystem (InputSystemUIInputModule)

### MenuItem
`[MenuItem("Assets/Setup/024v2 BubblePop")]`

### UIレスポンシブ配置
- 上部HUD (Y=-30〜-80): Stage, Score
- 左上 (Y=-30): LifeText
- 下部 (Y=15〜55): MenuButton（左下）, ReShowBtn（右下）
- 下段ゲーム操作なし（タップのみ）

### ステージ別新ルール追加
- Stage 1: 基本のみ（チュートリアル的）
- Stage 2: 3色バブル + 同色連鎖システム有効
- Stage 3: 鉄バブル（2タップ）登場
- Stage 4: 分裂バブル（割ると2分裂）登場
- Stage 5: 透明バブル + 全要素複合

### 判断ポイントの実装設計
- **トリガー**: 複数バブルが画面上半分に入った時（毎フレームチェック）
- **選択**: タッパーはどのバブルを次に割るか選択
- **報酬**: 同色連鎖→高スコア / 安全処理→ライフ維持
- **ペナルティ**: 連鎖中断→倍率リセット / ライフ消失→ゲームオーバーに近づく

### Buggy Code防止策
- バブルはGameObject名またはenumで種別管理（タグ不使用）
- `_isActive` ガードでゲームオーバー後の入力を防止
- Texture2D/Spriteは動的生成しない（Resources配下のアセット使用）
- Camera.mainはキャッシュして毎フレーム呼ばない

### レスポンシブ配置計算
```csharp
float camSize = Camera.main.orthographicSize; // 5.0f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 2.8f; // UIボタン領域
float gameAreaTop = camSize - topMargin;
float gameAreaBottom = -camSize + bottomMargin;
// バブル出現: Y = -camSize + 0.5f, X = Random[-camWidth*0.8, camWidth*0.8]
// 上端判定: Y > camSize - topMargin
```
