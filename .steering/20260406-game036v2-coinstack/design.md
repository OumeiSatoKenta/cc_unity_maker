# 設計書: Game036v2_CoinStack

## namespace

`Game036v2_CoinStack`

## スクリプト構成

| ファイル | クラス | 責務 |
|---------|-------|------|
| CoinStackGameManager.cs | CoinStackGameManager | ゲーム状態・スコア・ライフサイクル管理 |
| CoinMechanic.cs | CoinMechanic | コアメカニクス（スライド・落下・積み重ね・崩壊判定） |
| CoinStackUI.cs | CoinStackUI | HUD・クリア/ゲームオーバーパネル表示 |

## クラス設計

### CoinStackGameManager

```
状態: WaitingInstruction / Playing / StageClear / Clear / GameOver

[SerializeField] StageManager _stageManager
[SerializeField] InstructionPanel _instructionPanel
[SerializeField] CoinMechanic _mechanic
[SerializeField] CoinStackUI _ui

Start(): InstructionPanel.Show() → OnDismissed += StartGame
StartGame(): StageManager.SetConfigs() → Subscribe OnStageChanged/OnAllStagesCleared → StartFromBeginning()
OnStageChanged(int): mechanic.SetupStage(config, stageIndex) → ui更新
OnAllStagesCleared(): 最終クリア
OnCoinPlaced(float offset): コンボ更新 → スコア計算
OnTowerCollapsed(): GameOver
AdvanceToNextStage(): stageManager.CompleteCurrentStage()
```

### CoinMechanic

```
[SerializeField] SpriteRenderer _sliderCoinRenderer
[SerializeField] Transform _coinStackRoot
[SerializeField] GameObject _targetLinePrefab (or lineRenderer)
[SerializeField] Sprite _normalCoinSprite
[SerializeField] Sprite _heavyCoinSprite
[SerializeField] Sprite _lightCoinSprite

SetupStage(config, stageIndex):
  - 速度・目標段数・新要素フラグをセット
  - 既存スタックをクリア
  - 最初のスライドコインを生成
  - 目標ラインを表示

入力処理:
  - Mouse.current.leftButton.wasPressedThisFrame
  - 入力時: スライド停止 → 落下コルーチン開始
  - 落下完了後: ずれ計算 → コールバック → 次コイン準備

崩壊判定:
  - 各コインの相対X位置が閾値(1.5u)超えたら崩壊
  - または直近3コインの傾き累積が30度超えたら崩壊

ビジュアルフィードバック:
  - PERFECT（ずれ<0.1u）: コインが1.0→1.3→1.0スケールポップ（0.2秒）
  - GOOD（ずれ0.1〜0.3u）: 黄色フラッシュ
  - MISS（ずれ>0.3u）: 赤フラッシュ + カメラシェイク
  - 崩壊時: 全コインが重力落下アニメーション + 大きめカメラシェイク

コンボ更新:
  - 連続PERFECT: 3連続でx2倍、5連続でx3倍
  - GOOD/MISS: コンボリセット

Stage2: CoinType選択 (Random: 70%Normal/15%Heavy/15%Light)
Stage3: 風エフェクト (落下中にコインX位置に±ランダム0.3u加算)
Stage4: 特殊コイン（重い=幅1.4x/安定係数0.7, 軽い=幅0.7x/安定係数1.5）
Stage5: 地震コルーチン (3秒ごとにStack全体をシェイク ±0.1u)
```

### CoinStackUI

```
[SerializeField] TextMeshProUGUI _stageText
[SerializeField] TextMeshProUGUI _scoreText
[SerializeField] TextMeshProUGUI _coinCountText
[SerializeField] TextMeshProUGUI _comboText
[SerializeField] GameObject _stageClearPanel
[SerializeField] GameObject _finalClearPanel
[SerializeField] GameObject _gameOverPanel
[SerializeField] Button _nextStageButton
[SerializeField] Button _retryButton
[SerializeField] Button _returnMenuButton

Initialize(manager): ボタンリスナー登録
UpdateStage(current, total): "Stage X / 5"
UpdateScore(score): スコアテキスト更新
UpdateCoinCount(remaining, total): "残り X 枚"
UpdateCombo(combo): コンボ表示（0の時は非表示）
ShowStageClear(stageNum, totalStages): ステージクリアパネル表示
ShowFinalClear(score): 最終クリアパネル表示
ShowGameOver(score): ゲームオーバーパネル表示
HideStageClear(): パネル非表示
```

## レスポンシブ配置設計

```csharp
float camSize = Camera.main.orthographicSize; // 5.0
float camWidth = camSize * Camera.main.aspect; // ~2.8 (9:16想定)
float topMargin = 1.2f;    // HUD領域確保
float bottomMargin = 2.8f; // Canvasボタン領域確保
// ゲーム領域: y = -camSize+bottomMargin 〜 camSize-topMargin
// コインスライド位置: y = camSize - 0.5f (画面上部から登場)
// スタック底: y = -camSize + bottomMargin
// コインサイズ: 0.9u (基本)
```

## StageManager統合

```
StageConfigs:
  Stage1: speedMultiplier=1.0, countMultiplier=5,  complexityFactor=0.0
  Stage2: speedMultiplier=1.5, countMultiplier=8,  complexityFactor=0.2
  Stage3: speedMultiplier=2.0, countMultiplier=10, complexityFactor=0.5
  Stage4: speedMultiplier=2.5, countMultiplier=12, complexityFactor=0.7
  Stage5: speedMultiplier=3.0, countMultiplier=15, complexityFactor=1.0

OnStageChanged → mechanic.SetupStage(config, stageIndex)
OnAllStagesCleared → FinalClear
```

## InstructionPanel内容

```
gameId: "036v2"
title: "CoinStack"
description: "コインをタイミングよく積み上げてタワーを作ろう"
controls: "タップでコインをドロップ"
goal: "崩さずに目標の高さまで積み上げよう"
```

## ビジュアルフィードバック設計

1. **PERFECT判定時**: コインスケールポップ (1.0 → 1.3 → 1.0、0.2秒コルーチン) + 金色ハイライト
2. **MISS判定時**: SpriteRenderer.color赤フラッシュ + カメラシェイク (0.15秒、0.1u振幅)
3. **コンボ時**: コンボテキストがスケールアップ演出 (1.0 → 1.5 → 1.0)
4. **崩壊時**: 全コインにRigidbody2Dを付与して重力落下 + 大きめカメラシェイク

## スコアシステム

- PERFECT (ずれ < 0.1u): 30点 × コンボ倍率
- GOOD (ずれ 0.1〜0.3u): 10点 × 1 (コンボなし)
- MISS (ずれ > 0.3u): 0点 + コンボリセット
- コンボ倍率: 1〜2連続=1x, 3〜4連続=2x, 5連続以上=3x

## ステージ別新ルール表

| ステージ | 新要素 | 詳細 |
|---------|-------|------|
| Stage 1 | なし（チュートリアル） | 速度2.0、目標5段 |
| Stage 2 | コインサイズランダム | 0.8x〜1.2xでランダム変化、バランス計算が複雑に |
| Stage 3 | 風エフェクト | 落下中にコインX位置がランダムにドリフト(±0.3u) |
| Stage 4 | 特殊コイン | 重い(幅1.4x、安定)/軽い(幅0.7x、不安定)コイン登場 |
| Stage 5 | 地震イベント | 3秒ごとにタワー全体が横揺れ(±0.1u) |

## SceneSetup構成方針

`Setup036v2_CoinStack.cs` → `Assets/Editor/SceneSetup/`
MenuItem: `Assets/Setup/036v2 CoinStack`

必須セットアップ:
1. Camera (orthographic, size=5, 暗めの背景色)
2. DirectionalLight
3. Background (スプライト)
4. SliderCoin GameObject (SpriteRenderer, スライド表示用)
5. CoinStackRoot (空オブジェクト, 積まれたコインの親)
6. TargetLine (LineRenderer or 薄い横線スプライト)
7. Canvas (ScreenSpace-Camera, EventSystem+InputSystemUIInputModule)
   - HUD
   - InstructionPanel
   - StageClearPanel
   - FinalClearPanel
   - GameOverPanel
8. GameManager (CoinStackGameManager + CoinMechanic + CoinStackUI)
   - StageManager (子オブジェクト)

フィールド配線:
- GameManager._stageManager ← StageManager
- GameManager._instructionPanel ← InstructionPanel
- GameManager._mechanic ← CoinMechanic
- GameManager._ui ← CoinStackUI
- CoinMechanic._sliderCoinRenderer ← SliderCoin.SpriteRenderer
- CoinMechanic._coinStackRoot ← CoinStackRoot
- CoinStackUI.各テキスト・パネル・ボタン

## アセット一覧 (Sprites/Game036v2_CoinStack/)

| ファイル | 用途 | サイズ | 色テーマ |
|---------|-----|-------|---------|
| Background.png | 背景 | 256x512 | action系ダーク (#1a1a2e) |
| Coin.png | 通常コイン | 128x64 | 金色グラデーション (#FFD700→#FFA000) |
| HeavyCoin.png | 重いコイン | 192x64 | 銀色グラデーション (#9E9E9E→#616161) |
| LightCoin.png | 軽いコイン | 96x64 | 銅色グラデーション (#FF8F00→#E65100) |
| TargetLine.png | 目標ラインインジケータ | 256x16 | 緑 (#4CAF50) |
| PerfectEffect.png | PERFECT演出 | 128x128 | 金+星 |

## Buggy Code防止チェック

- Physics2D使用なし（タグ比較はgameObject.name）
- 複数Updateは_isActiveガード付き
- Texture2D/Spriteはスクリプト外で管理（SceneSetupで生成、OnDestroyは不要）
- レスポンシブ配置は全てcamSizeから動的計算
- Canvas UIボタンと下部マージン2.8u以上確保
