# Design: Game096v2 DualControl

## namespace

`Game096v2_DualControl`

## スクリプト構成

### DualControlGameManager.cs
- ゲーム全体の状態管理（Playing / StageClear / AllClear / GameOver）
- StageManager・InstructionPanelを統合
- スコア・シンクロボーナス・タイマー管理
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] ControlManager _controlManager`
- `[SerializeField] DualControlUI _ui`

**Start()の流れ:**
1. `_instructionPanel.Show("096", "DualControl", ...)`
2. `_instructionPanel.OnDismissed += StartGame`

**StartGame():**
1. `_stageManager.OnStageChanged += OnStageChanged`
2. `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
3. `_stageManager.StartFromBeginning()`

**OnStageChanged(int stageIndex):**
- `_controlManager.SetupStage(config, stageIndex)`
- UIを更新

**スコア計算:**
- ステージクリア時: `100 × (stageIndex+1) × config.speedMultiplier`
- シンクロボーナス: 両キャラが1秒以内にゴール → ×2.0倍
- ノーダメージ: +50pt

### ControlManager.cs
- 2キャラ（LeftChar / RightChar）の生成・管理
- 入力処理（タッチ/マウス左右分割）
- 障害物・スイッチ・ゴールの生成
- 衝突判定（Physics2D.OverlapPoint）
- **5ステージ難易度対応**: `SetupStage(StageConfig, stageIndex)`

**入力処理:**
```csharp
// マウス（PCエディタ向け）
if (Mouse.current.leftButton.isPressed) {
    Vector2 screenPos = Mouse.current.position.ReadValue();
    float halfWidth = Screen.width * 0.5f;
    if (screenPos.x < halfWidth) { /* 左キャラ操作 */ }
    else { /* 右キャラ操作 */ }
}
// タッチ（モバイル向け）
foreach (var touch in Touchscreen.current.touches) {
    if (touch.isInProgress.ReadValue() > 0) {
        Vector2 tp = touch.position.ReadValue();
        if (tp.x < Screen.width * 0.5f) { /* 左 */ }
        else { /* 右 */ }
    }
}
```

**レスポンシブ配置（必須）:**
```csharp
float camSize = Camera.main.orthographicSize; // 6.0f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;    // HUD領域
float bottomMargin = 2.8f; // Canvas UIボタン領域
float availableHeight = camSize * 2f - topMargin - bottomMargin;
// 左半分 x: -camWidth/2 ～ 0, 右半分 x: 0 ～ camWidth/2
float halfW = camWidth * 0.5f;
```

**障害物レイアウト設計:**
- Stage 1: 各サイド3行×1列の静止障害（左右同じ配置）
- Stage 2: 各サイド独立配置（左右で行数・位置が異なる）
- Stage 3: 横移動する障害物（`speedMultiplier`で速度調整）
- Stage 4: スイッチ＋ドア（片方スイッチ踏む → もう片方のドア消える）
- Stage 5: 全要素複合

**衝突判定:**
- 障害物・スイッチはレイヤー "Obstacle"
- ゴールはレイヤー "Goal" (or タグ使用)
- `Physics2D.OverlapPoint(pos)` でキャラの位置をチェック
- タグではなく `gameObject.name`で判別（Physics2Dタグ比較バグ防止）

**ビジュアルフィードバック:**
1. **ゴール到達時**: `transform.localScale` ポップ（1.0→1.3→1.0、0.2秒）
2. **トラップ接触時**: `SpriteRenderer.color` 赤フラッシュ（0.15秒）＋カメラシェイク（0.3秒、0.15振幅）
3. **シンクロゴール時**: 両キャラ同時に金色フラッシュ演出

**シンクロ判定:**
```csharp
float _leftGoalTime = -1f;
float _rightGoalTime = -1f;
bool IsSynced => Mathf.Abs(_leftGoalTime - _rightGoalTime) <= 1.0f;
```

**_isActive ガード**: GameManager から `_isActive` フラグをチェックし、ゲームオーバー/クリア後は入力処理を止める

**動的リソースクリーンアップ:**
```csharp
void OnDestroy() {
    // 生成した障害物GameObject等をClean up
    ClearStage();
}
```

### DualControlUI.cs
- スコア・タイマー・ステージ表示
- コンボ/シンクロボーナス表示
- StageClearPanel・AllClearPanel・GameOverPanel の表示切替
- ボタンのUnityEvent登録

**必須表示要素:**
- `_stageText`: "Stage X / 5"
- `_scoreText`: スコア
- `_timerText`: 経過タイマー（秒）
- `_comboText`: シンクロ/コンボ状態
- `_stageClearPanel` + `_nextStageButton`
- `_allClearPanel` + AllClearスコア
- `_gameOverPanel` + `_retryButton`
- `_menuButton`: BackToMenuButton

## 盤面・ステージデータ設計

```
【画面レイアウト】（縦1080×1920想定、orthographicSize=6）
 ┌────────────────────────────────┐  y= 5.8
 │  StageText │  ScoreText       │  HUD
 ├────────────┼───────────────────┤  y= 4.6
 │            │ RIGHT CHAR        │
 │ LEFT CHAR  │  障害物群(右)     │  ゲーム領域
 │  障害物群  │                   │  (availableHeight≈8.4→÷7行)
 │   (左)     │  GOAL RIGHT ▼    │
 │  GOAL LEFT▼│                   │
 ├────────────┴───────────────────┤  y=-3.2
 │   [次のステージ] [メニュー]   │  Canvas UI
 └────────────────────────────────┘  y=-5.8
```

各キャラの開始位置: y = camSize - topMargin - 0.5 ≈ 4.3（上端付近）
ゴール位置: y = -(camSize - bottomMargin) + 0.5 ≈ -2.8（下端付近）

スイッチ配置（Stage4〜5）:
- LeftSwitch: 左エリア中段
- RightDoor: 右エリア中段（スイッチ踏むと消える）
- RightSwitch: 右エリア中段
- LeftDoor: 左エリア中段

## SceneSetup 構成方針

`Setup096v2_DualControl.cs` を `Assets/Editor/SceneSetup/` に作成

**メニュー**: `[MenuItem("Assets/Setup/096v2 DualControl")]`

**生成順序:**
1. Camera設定（orthographicSize=6, 背景色ダーク）
2. Background スプライト配置
3. スプライト読み込み
4. DualControlGameManager 階層作成
   - StageManager（子）
   - ControlManager（子）
5. Canvas（HUD）作成
   - StageText, ScoreText, TimerText, ComboText（上部HUD）
   - BackToMenuButton（下部固定）
   - StageClearPanel（中央オーバーレイ）
   - AllClearPanel（中央オーバーレイ）
   - GameOverPanel（中央オーバーレイ）
6. InstructionCanvas（sortOrder=100）
   - InstructionPanel（フルスクリーン）
   - TitleText, DescText, ControlsText, GoalText, StartButton
   - HelpButton（メインCanvasの右下）
7. DualControlUI（GameManager子オブジェクト）
8. フィールド配線（SetField）
9. EventSystem（InputSystemUIInputModule）
10. シーン保存・BuildSettings追加

**InstructionPanel フィールド配線:**
```
_panelRoot, _titleText, _descriptionText, _controlsText, _goalText, _startButton, _helpButton
```

**StageManager StageConfigs:**
| Stage | speedMultiplier | countMultiplier | complexityFactor |
|-------|----------------|----------------|----------------|
| 1     | 1.0            | 1              | 0.0            |
| 2     | 1.2            | 2              | 0.3            |
| 3     | 1.4            | 2              | 0.5            |
| 4     | 1.6            | 3              | 0.7            |
| 5     | 2.0            | 3              | 1.0            |

## InstructionPanel 内容

```
title: "DualControl"
description: "左右の親指で2キャラ同時操作！"
controls: "左ドラッグで左キャラ、右ドラッグで右キャラを操作"
goal: "2人同時にゴールさせよう"
```

## ビジュアルフィードバック設計

1. **ゴール到達**: キャラのスケールポップ（1.0→1.3→1.0、Coroutine 0.2秒）
2. **トラップ接触**: 赤フラッシュ（SpriteRenderer.color→赤→元色、0.15秒）＋カメラシェイク（振幅0.15、0.3秒）
3. **シンクロゴール**: 両キャラ金色フラッシュ + "SYNCHRO BONUS!" テキストポップ

## スコアシステム

- ステージクリア基本: `100 × (stageIndex+1) × config.speedMultiplier`（intに丸め）
- シンクロボーナス: ×2.0倍（両キャラが1秒以内にゴール）
- ノーダメージ: +50pt/ステージ
- 総スコアはDualControlGameManagerが管理

## 判断ポイントの実装設計

- **注意配分トリガー**: 障害物が各キャラに0.5ユニット以内に接近したとき
- **速度調整**: キャラはドラッグ位置に追随（速度=`moveSpeed × speedMultiplier`、最大6 units/sec）
- **スイッチ同期**: Stage4〜5でスイッチ踏んだ瞬間にドアGameObjectを非表示化

## Buggy Code防止チェック

- [x] Physics2Dタグ比較 → `gameObject.name` 使用
- [x] `_isActive` ガード → GameOver/Clear後は入力停止
- [x] `OnDestroy()` でClearStage()（動的生成GameObjectのDestroy）
- [x] ワールド座標は `Camera.main.orthographicSize` から動的計算
- [x] 下部2.8uマージン確保（Canvas UIボタン重複防止）
