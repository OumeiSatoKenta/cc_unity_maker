# Design: Game030v2_FingerRacer

## namespace
`Game030v2_FingerRacer`

## スクリプト構成

### FingerRacerGameManager.cs
- ゲーム状態管理: WaitingInstruction / Drawing / Racing / StageClear / Clear / GameOver
- StageManager/InstructionPanel統合
- スコア・コンボ・ブースト残量・コースアウト回数管理
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] CourseDrawer _courseDrawer`
- `[SerializeField] CarController _carController`
- `[SerializeField] FingerRacerUI _ui`
- Start(): instructionPanel.Show() → OnDismissed += StartGame
- StartGame(): stageManager.StartFromBeginning()
- OnStageChanged(int stage): ステージパラメータ適用、コース再セットアップ
- OnAllStagesCleared(): ゲームクリア

### CourseDrawer.cs
- ドラッグ入力でコースポイントを収集（LineRenderer で可視化）
- `SetupStage(config, stageIndex)`: チェックポイント・障害物・砂地配置
- `SetupForDrawing()`: 描画モード開始
- `GetCoursePoints()`: 収集したポイント列を返す
- `ComputeSmoothness()`: カーブの曲率計算（0〜1、1が完全な直線）
- `HasObstacleCollision()`: 軌跡が障害物に接触しているか判定
- `StartRace()`: 描画完了後、CarControllerに渡す

### CarController.cs
- 収集した軌跡ポイントに沿って車を移動（Vector3.MoveTowards + 方向回転）
- ブースト処理: タップで発動、直線判定（前後ポイントの角度が170°以上）でのみ加速
- コースアウト判定: チェックポイントの通過確認 + 経路逸脱チェック
- `SetupStage(StageManager.StageConfig config, int stageIndex)`: 速度・コースアウト上限設定
- `StartRace(Vector3[] coursePoints)`: レース開始
- `TriggerBoost()`: ブースト発動

### FingerRacerUI.cs
- HUD: タイム・ブースト残量・コースアウト回数・スコア表示
- ステージクリアパネル
- 最終クリアパネル
- ゲームオーバーパネル
- 描画フェーズUI（「スタート」ボタン）

## 盤面・ステージデータ設計
```csharp
StageConfig {
  speedMultiplier: 車の基本速度倍率
  countMultiplier: チェックポイント数 (2,3,4,5,6に対応)
  complexityFactor: 0=障害物なし, 0.5=障害物あり, 1.0=ライバル+全要素
}
```

| Stage | speedMultiplier | countMultiplier | complexityFactor |
|-------|----------------|-----------------|-----------------|
| 1 | 1.0 | 2 | 0.0 |
| 2 | 1.25 | 3 | 0.3 |
| 3 | 1.5 | 4 | 0.5 |
| 4 | 1.75 | 5 | 0.7 |
| 5 | 2.0 | 6 | 1.0 |

## 入力処理フロー
- 描画フェーズ: `Mouse.current.leftButton.isPressed` でドラッグポイント収集
- レース中ブースト: `Mouse.current.leftButton.wasPressedThisFrame` でブースト発動
- using `UnityEngine.InputSystem;`

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5f
float camWidth = camSize * Camera.main.aspect;
// ゲーム領域: Y -2.5 ～ +3.5 (上部1.5をHUD、下部2.5をUIボタン確保)
// スタートマーカー: (-camWidth*0.7, -2.0, 0)
// ゴールマーカー: (camWidth*0.7, 2.5, 0)
// チェックポイントは均等配置
```

## InstructionPanel内容
- title: "FingerRacer"
- description: "指でコースを描いて車をゴールへ導こう！"
- controls: "画面をドラッグしてコースを描き、スタートボタンでレース開始！\n直線でタップするとブースト加速！"
- goal: "コースアウト3回以内でゴールに到達しよう"

## StageManager統合
- `OnStageChanged` 購読で `CourseDrawer.SetupStage(config, stageIndex)` を呼び出す
- `OnAllStagesCleared` で最終クリア表示
- ステージクリアは車がゴール到達時に `_stageManager.CompleteCurrentStage()`

## ビジュアルフィードバック設計
1. **チェックポイント通過**: チェックポイントスプライトがスケールパルス (1.0 → 1.5 → 1.0, 0.3秒) + 緑フラッシュ
2. **コースアウト**: 画面が赤くフラッシュ (SpriteRenderer.color赤) + カメラシェイク (0.3秒)
3. **ブースト成功**: 車スプライトに黄色トレイル + スケール微増
4. **ゴール到達**: カメラシェイク(小) + ゴールマーカー虹色点滅

## スコアシステム
- 基本スコア: ゴール到達で `10000 - elapsed_ms`（経過時間が短いほど高得点）
- チェックポイントボーナス: 100pt × 通過チェックポイント数
- 滑らかさボーナス: `smoothness * 500` pt（描いたコースの平均滑らかさ）
- ブーストコンボ: 連続ブースト成功 x1.5 / x2.0 / x3.0
- パーフェクト: コースアウト0回 +1000pt

## SceneSetup構成方針
- Menu: `Assets/Setup/030v2 FingerRacer`
- SceneSetup: `Setup030v2_FingerRacer.cs`
- スプライト一覧:
  - Background.png（道路風の暗い背景）
  - Car.png（赤い車、俯瞰視点）
  - RivalCar.png（青い車）
  - Checkpoint.png（緑のダイヤ型マーカー）
  - StartFlag.png（緑旗）
  - GoalFlag.png（黄色旗）
  - Obstacle.png（赤い三角）
  - BoostIcon.png（稲妻アイコン）

## ステージ別新ルール表
- Stage 1: 基本のみ（描く→走る）。チェックポイント2個
- Stage 2: カーブ曲率計算追加。急カーブ（曲率 > 60°）で車が30%減速
- Stage 3: 障害物3〜5個配置。軌跡が障害物と交差したら描き直し要求
- Stage 4: 砂地エリア2箇所。通過時に車速30%ダウン（視覚的に砂色テクスチャ）
- Stage 5: ライバルAI車が固定経路を走る。ライバルより遅いとゲームオーバー

## 判断ポイントの実装設計
- **トリガー**: 毎フレーム、前後3ポイントの角度を計算し、30°以上なら「カーブ中」フラグ
- **ブースト発動条件チェック**: カーブ中にブースト → スピン（コースアウト判定）
- **報酬**: ブースト直線成功 → +50pt × comboMultiplier + 速度1.5倍(0.5秒)
- **ペナルティ**: コースアウト → コンボリセット、-1残機、2秒停止後復帰

## 配線漏れ対策
- GameManager: _stageManager, _instructionPanel, _courseDrawer, _carController, _ui
- CarController: _gameManager
- CourseDrawer: _gameManager, _carController
- FingerRacerUI: _gameManager, 各パネル参照
