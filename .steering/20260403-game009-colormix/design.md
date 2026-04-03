# Design: Game009v2 ColorMix

## namespace
`Game009v2_ColorMix`

## スクリプト構成

### ColorMixGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- StageManager・InstructionPanel統合
- スコア・コンボ管理
- ColorMixManager に現在ステージのパラメータを渡す

### ColorMixManager.cs
- コアメカニクス: スライダー値管理、色差計算（ΔE）、判定ロジック
- ステージ別ターゲットカラー設定
- 動的目標（ステージ5: コルーチンで色を変化）
- ビジュアルフィードバック演出

### ColorMixUI.cs
- スコア・ステージ・コンボ表示
- スライダーUI管理（RGBスライダー + ステージ3〜のVスライダー）
- 残り判定回数表示（ステージ4〜）
- パネル表示/非表示（ステージクリア・ゲームクリア・ゲームオーバー）

## ステージパラメータ設計

```
Stage 0 (index): allowedDeltaE=20, maxJudgments=-1(無制限), dynamicTarget=false, showBrightnessSlider=false
Stage 1: allowedDeltaE=15, maxJudgments=-1, dynamicTarget=false, showBrightnessSlider=false
Stage 2: allowedDeltaE=12, maxJudgments=-1, dynamicTarget=false, showBrightnessSlider=true
Stage 3: allowedDeltaE=10, maxJudgments=3, dynamicTarget=false, showBrightnessSlider=true
Stage 4: allowedDeltaE=8, maxJudgments=3, dynamicTarget=true, showBrightnessSlider=true
```

## ターゲットカラー定義

```
Stage 0: 赤(255,50,50)・青(50,100,255)・黄(255,220,0) からランダム1色
Stage 1: 緑(50,200,80)・紫(160,50,200)・橙(255,140,0) からランダム1色
Stage 2: パステルピンク(255,180,200)・ミントグリーン(150,230,200)・ラベンダー(200,180,240) からランダム
Stage 3: 段階的に変化する難しめの色（ティール、マゼンタ、ゴールド等）
Stage 4: 複合色（シアン、コーラル、ライム）＋ 周期変化
```

## 色差計算（ΔE）
簡易的なユークリッド距離:
```csharp
float dr = (targetR - currentR) / 255f;
float dg = (targetG - currentG) / 255f;
float db = (targetB - currentB) / 255f;
float deltaE = Mathf.Sqrt(dr*dr + dg*dg + db*db) * 100f; // 0〜173.2
```
許容値と比較してクリア判定。

## InstructionPanel
- title: "ColorMix"
- description: "スライダーで色を混ぜて目標の色を再現するパズル"
- controls: "R/G/Bスライダーをドラッグして色を調整"
- goal: "目標色にできるだけ近い色を作ろう"

## StageManager統合
- `_stageManager.OnStageChanged += OnStageChanged`
- `OnStageChanged(int stage)` → ColorMixManager.SetupStage(stage) 呼び出し
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`

## ビジュアルフィードバック設計
1. **成功時（クリア）**: 混色プレビューのスケールパルス (1.0 → 1.3 → 1.0、0.3秒) + 緑フラッシュ
2. **失敗時（ΔE超過）**: 混色プレビューの赤フラッシュ + カメラシェイク（amplitude=0.15, duration=0.25秒）
3. **スライダー変更時**: 混色プレビューのスムーズカラー更新（Lerp 0.1秒）

## スコアシステム
- 基本スコア = (100 - deltaE) × 10 × (stageIndex + 1)
- 1回目判定でΔE≤5: ×3.0ボーナス
- 連続ステージクリアコンボ: ×1.2累積
- ★3: ΔE≤5, ★2: ΔE≤15, ★1: クリア

## SceneSetup構成方針 (Setup009v2_ColorMix.cs)
- Camera: 背景色 (0.05, 0.05, 0.12) 暗めの青紫
- 上部HUD: ステージ・スコア・コンボ表示
- 中央: 目標色パネル（左）・混色プレビューパネル（右）・ΔE表示
- 下部: RGBスライダー3本（+ ステージ3〜Vスライダー）・判定ボタン・リセットボタン
- 残り判定回数テキスト（ステージ4〜）
- ボタン最低サイズ: (150, 55)
- StageManager, InstructionPanel配線
- EventSystem: InputSystemUIInputModule

## レスポンシブ配置
- ゲーム要素はCanvas UI内で完結（ワールド座標のゲームオブジェクトなし）
- Canvas: ScreenSpaceOverlay, referenceResolution: (1080, 1920)
- 目標色・混色プレビュー: 各200×200px の Image
