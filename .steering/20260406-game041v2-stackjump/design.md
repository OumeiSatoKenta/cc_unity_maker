# Design: Game041v2 StackJump

## namespace
`Game041v2_StackJump`

## スクリプト構成

| クラス | ファイル | 責務 |
|--------|---------|------|
| StackJumpGameManager | StackJumpGameManager.cs | ゲーム状態管理、StageManager/InstructionPanel統合、スコア管理 |
| StackJumpMechanic | StackJumpMechanic.cs | ブロックスライド・タップ停止・カット処理・Perfect判定・コンボ |
| StackJumpUI | StackJumpUI.cs | 段数/スコア/コンボ表示、ステージクリア/ゲームオーバーパネル |

## ゲーム空間設計
- カメラ: orthographic, size=5, 固定（カメラシェイクはコルーチンで一時的に）
- ゲーム領域: X軸 ±2.5, Y軸は下から積み上げ
- ブロックは2D SpriteRenderer
- 底盤（ベースブロック）をY=-3.0あたりに固定配置
- 積み上がるほどカメラが追従（`camera.transform.position.y` をスムーズに上昇）

## ブロック配置計算
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float blockHeight = 0.4f;
float maxBlockWidth = camWidth * 1.6f; // 画面幅の80%
// ブロックY位置: baseY + stackCount * blockHeight
// スライド範囲: ±(camWidth - blockWidth/2)
```

## StackJumpMechanic 設計

### フィールド
```csharp
[SerializeField] StackJumpGameManager _gameManager
[SerializeField] Sprite _spriteBlock, _spritePerfect, _spriteBackground, _spriteBase
float _blockWidth (現在のブロック幅)
float _slideSpeed (ステージ設定から)
bool _isSliding (スライド中か)
bool _isAxisX (X軸スライドか)
int _stackCount (積み上げ段数)
int _comboCount (Perfectコンボ数)
int _targetCount (ステージ目標段数)
bool _useAcceleration (Stage3以降)
bool _useCameraShake (Stage4以降)
float _initialWidthMultiplier (Stage5用)
bool _isActive
List<GameObject> _stackedBlocks
GameObject _slidingBlock
```

### GetCurrentStageConfig() 対応パラメータ
- speedMultiplier → slideSpeed
- countMultiplier → targetCount
- complexityFactor → acceleration/shake/shrink制御

### SetupStage(StageManager.StageConfig config)
```
stage 0 (Stage1): speed=2.0, target=10, axisX only, no extras
stage 1 (Stage2): speed=2.5, target=15, X/Y交互
stage 2 (Stage3): speed=3.0, target=20, 加速ギミック
stage 3 (Stage4): speed=3.5, target=25, カメラシェイク
stage 4 (Stage5): speed=4.0, target=30, 縮小スタート(70%)+全複合
```

### 入力処理
```csharp
Mouse.current.leftButton.wasPressedThisFrame // 新Input System
```

### Perfect判定
```csharp
float overlap = ...;
float perfectThreshold = 0.1f; // ±0.1ユニット以内
bool isPerfect = Mathf.Abs(offset) < perfectThreshold;
if (isPerfect) { _blockWidth = Mathf.Min(_blockWidth + 0.2f, maxWidth); }
else { _blockWidth = overlap; }
```

### ビジュアルフィードバック
1. **Perfect時**: ブロックのスケールパルス（1.0→1.3→1.0、0.2秒）+ 白フラッシュ + "PERFECT!" テキスト演出
2. **ミス/カット時**: 赤フラッシュ（SpriteRenderer.color変更0.15秒）
3. **カメラシェイク（Stage4+）**: 積み上げ時にカメラを±0.05ユニット0.2秒間シェイク
4. **コンボ時**: コンボカウント表示のスケール演出

## InstructionPanel 内容
```
gameId: "041v2"
title: "StackJump"
description: "タイミングよくタップしてブロックを積み上げよう"
controls: "画面タップでブロックを止める"
goal: "目標段数まで積み上げてステージクリア！"
```

## StageManager 統合
- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
- `OnStageChanged(int stage)` → `_mechanic.SetupStage(stage)`
- StageManagerのデフォルト5ステージ設定を使用（speedMultiplier/countMultiplier）

## ステージ別新ルール表
| Stage | index | 新要素 |
|-------|-------|--------|
| 1 | 0 | 基本X軸スライドのみ |
| 2 | 1 | Y軸スライドをX軸と交互に追加 |
| 3 | 2 | 5段ごとに速度+0.5の加速ギミック |
| 4 | 3 | 積み上げ時カメラシェイク追加 |
| 5 | 4 | 初期ブロック幅70%スタート＋全要素複合 |

## スコアシステム
- 1段積み上げ: `baseScore = 100`
- Perfect: `perfectScore = 300 + comboCount * 150`
- ステージクリアボーナス: `(currentWidth / maxWidth) * 1000`

## SceneSetup (Setup041v2_StackJump.cs)

### MenuItem
`[MenuItem("Assets/Setup/041v2 StackJump")]`

### シーン構成
```
Main Camera (orthographic, size=5, bg=#1a1a2e)
  Directional Light
Background (SpriteRenderer, sorting=-10)
GameManager (StackJumpGameManager)
  StageManager
  StackJumpMechanic
Canvas (ScreenSpaceOverlay)
  InstructionPanel (fullscreen overlay)
    Title/Desc/Controls/Goal TextMeshPro
    "はじめる"ボタン (sizeDelta 200x55)
  "?"ボタン (右下、再表示、sizeDelta 55x55)
  HUD
    StageText (上部左、"Stage 1/5")
    ScoreText (上部右)
    ProgressSlider (上部中央)
  ComboText (中央、Perfectコンボ時表示)
  PerfectText (中央、"PERFECT!"表示)
  StageClearPanel
    "ステージクリア！"Text
    "次のステージへ"ボタン (sizeDelta 200x55)
  FinalClearPanel
    "全ステージクリア！"Text
    ScoreText
    "メニューへ戻る"ボタン
  GameOverPanel
    "ゲームオーバー"Text
    ScoreText
    "リトライ"ボタン (sizeDelta 160x55)
    "メニューへ戻る"ボタン (sizeDelta 160x55)
EventSystem (InputSystemUIInputModule)
```

### 配線
- gm._stageManager = stageMgr
- gm._instructionPanel = instructionPanel
- gm._mechanic = mech
- gm._ui = ui
- mech._gameManager = gm (SerializedObject)
- ui._gameManager = gm (SerializedObject)
- ボタンのOnClickをSerializedObjectで登録

## レスポンシブ配置
```csharp
float camSize = 5f;
float bottomMargin = 2.8f; // Canvas UIボタン領域
float topMargin = 1.5f;    // HUD領域
// ゲーム領域: Y = (-camSize + bottomMargin) ～ (camSize - topMargin)
// ブロックスライド範囲: X = ±(camWidth * 0.8 - blockWidth/2)
```
