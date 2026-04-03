# 設計書: Game002_MirrorMaze

## namespace

`Game002_MirrorMaze`

## スクリプト構成

### MirrorMazeGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] GridManager _gridManager`
- `[SerializeField] MirrorMazeUI _ui`
- Start() の流れ:
  1. `_instructionPanel.Show("002", "MirrorMaze", "鏡を配置してレーザーをゴールに導くパズル", "ドラッグで鏡を配置、タップで回転、発射ボタンで確認", "レーザーを全てのゴールに到達させよう")`
  2. `_instructionPanel.OnDismissed += StartGame`
  3. `StartGame()` 内で `_stageManager.StartFromBeginning()`
- StageManager統合:
  - `_stageManager.OnStageChanged += OnStageChanged`
  - `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
  - `OnStageChanged(int stage)` で `_gridManager.SetupStage(stage)` を呼ぶ
- スコア管理: `AddScore(int reflections, int unusedMirrors)` でスコア計算
- GameManager参照取得: GridManager, UI は `[SerializeField]` で配線

### GridManager.cs
- グリッド盤面の管理
- レーザーシミュレーション
- 鏡の配置・回転・ドラッグ処理
- `[SerializeField] MirrorMazeGameManager _gameManager`
- `bool _isActive` ガードで Update() 制御
- **入力処理を一元管理（このクラスに集約）**:
  - `Mouse.current.leftButton.wasPressedThisFrame` でタップ検出
  - `Mouse.current.leftButton.isPressed` でドラッグ検出
  - `Mouse.current.leftButton.wasReleasedThisFrame` でドロップ検出
  - `Physics2D.OverlapPoint` でグリッドセル・鏡のヒット判定
- ステージデータ:
  - 各ステージのレイアウト（エミッター位置、ゴール位置、壁位置、プリズム位置、移動壁位置）を静的配列で定義
  - `SetupStage(int stageIndex)` でグリッド再構築
- レーザーシミュレーション:
  - `SimulateLaser()`: エミッターからRaycast的にグリッドをトレース
  - 鏡に当たったら反射角度を計算（入射角=反射角）
  - プリズムに当たったら2方向に分岐（再帰的にシミュレーション）
  - 移動壁の開閉状態を考慮
  - 結果を `LineRenderer` で可視化
- 動的リソース管理:
  - `OnDestroy()` で動的生成した Texture2D, Sprite をクリーンアップ

### MirrorMazeUI.cs
- Canvas上のUI管理
- `[SerializeField] MirrorMazeGameManager _gameManager`
- 表示要素:
  - ステージ表示「Stage X / 5」
  - スコア表示
  - 残り鏡数表示
  - 発射ボタン（`OnFireButton()`）
  - リセットボタン（`OnResetButton()`）
  - ステージクリアパネル（「次のステージへ」ボタン）
  - 最終クリアパネル
  - ゲームオーバーパネル（「リトライ」ボタン）

## 盤面・ステージデータ設計

グリッドは正方形セル、カメラ正射影で表示。各セルは以下の状態を持つ:
- Empty: 空セル（鏡を配置可能）
- Wall: 壁ブロック（レーザー遮断）
- Emitter: レーザー発射装置（方向付き）
- Goal: ゴール受光器
- Prism: 分光プリズム（Stage4〜）
- MovingWall: 移動壁（Stage5）
- Mirror: プレイヤーが配置した鏡（0/45/90/135度）

### ステージ別パラメータ表

| パラメータ | Stage1 | Stage2 | Stage3 | Stage4 | Stage5 |
|-----------|--------|--------|--------|--------|--------|
| gridSize | 5 | 6 | 7 | 7 | 8 |
| mirrorCount | 1 | 2 | 3 | 3 | 4 |
| goalCount | 1 | 1 | 1 | 2 | 2 |
| wallCount | 0 | 0 | 3 | 2 | 3 |
| hasPrism | false | false | false | true | true |
| movingWallCount | 0 | 0 | 0 | 0 | 2 |
| movingWallPeriod | - | - | - | - | 2.0s |

## 入力処理フロー

1. Update() で `_isActive && _gameManager.IsPlaying` ガード
2. マウス/タッチ入力検出
3. **ドラッグ開始**: パーツ一覧の鏡をタップ → `_draggingMirror` に設定
4. **ドラッグ中**: マウス位置に鏡スプライトを追従
5. **ドロップ**: グリッドセル上でリリース → Empty セルならスナップ配置
6. **タップ（配置済み鏡）**: 45度回転
7. **発射ボタン**: UI側から `_gridManager.FireLaser()` 呼び出し

## SceneSetup の構成方針

`Setup002_MirrorMaze.cs`:
- `[MenuItem("Assets/Setup/002 MirrorMaze")]`
- `EditorApplication.isPlaying` チェック
- カメラ: 正射影、size=6、背景色ダーク
- Canvas: ScreenSpaceOverlay
- GameManager オブジェクト（子に StageManager）
- GridManager オブジェクト
- MirrorMazeUI（Canvas 子）
- InstructionPanel（Canvas 子、sortOrder 最前面）
- EventSystem（InputSystemUIInputModule）
- **全 SerializeField を SerializedObject で配線**:
  - GameManager._stageManager, _instructionPanel, _gridManager, _ui
  - GridManager._gameManager
  - MirrorMazeUI._gameManager
- ボタンを `UnityEventTools.AddPersistentListener` で配線
- `EditorSceneManager.SaveScene()` → `AddSceneToBuildSettings()`

### SceneSetup で配線が必要なフィールド一覧

| コンポーネント | フィールド | 接続先 |
|---|---|---|
| MirrorMazeGameManager | _stageManager | StageManager |
| MirrorMazeGameManager | _instructionPanel | InstructionPanel |
| MirrorMazeGameManager | _gridManager | GridManager |
| MirrorMazeGameManager | _ui | MirrorMazeUI |
| GridManager | _gameManager | MirrorMazeGameManager |
| MirrorMazeUI | _gameManager | MirrorMazeGameManager |

## StageManager統合

- `OnStageChanged` 購読: `OnStageChanged(int stage)` で `_gridManager.SetupStage(stage)` を呼び、グリッドを再構築
- `OnAllStagesCleared` 購読: 最終クリアパネルを表示
- `CompleteCurrentStage()` はステージクリア時に呼び出す

### ステージ再構築ロジック
1. 既存のグリッドオブジェクト（鏡・壁・エミッター・ゴール・プリズム・移動壁）を全削除
2. ステージデータからグリッドサイズに応じてセルを再生成
3. エミッター、ゴール、壁、プリズム、移動壁を配置
4. 鏡パーツ一覧を初期化（未配置状態）
5. レーザーラインをクリア

## InstructionPanel内容

| 項目 | 値 |
|---|---|
| title | MirrorMaze |
| description | 鏡を配置してレーザーをゴールに導くパズル |
| controls | ドラッグで鏡を配置、タップで回転、発射ボタンで確認 |
| goal | レーザーを全てのゴールに到達させよう |

## ビジュアルフィードバック設計

1. **レーザーヒット演出（スケールパルス）**: ゴールにレーザーが到達した時、ゴール受光器が1.0→1.5→1.0にスケールアニメーション（0.3秒）+ 色が緑にフラッシュ
2. **反射時の色フラッシュ**: レーザーが鏡に当たる瞬間、鏡スプライトが白→元色にフラッシュ（0.15秒）
3. **失敗時のカメラシェイク**: レーザーがゴールに届かなかった時、カメラを微振動（振幅0.1、0.3秒）
4. **コンボ時の複合演出**: 3回以上反射でレーザーの色が青→金に変化、スケール+色の複合演出

## スコアシステム

- **基本スコア**: ステージクリアで 500点
- **反射ボーナス**: 反射回数 × 100点
- **最小パーツボーナス**: 余った（未使用）鏡 1枚 × 200点
- **連続反射コンボ**: 3回以上の反射で ×1.5、5回以上で ×2.0
- **ステージ乗算**: Stage番号が倍率（Stage3なら ×3）

計算式: `totalScore = (500 + reflections*100 + unusedMirrors*200) * comboMultiplier * stageNumber`

## ステージ別新ルール表

| Stage | 基本ルール | 新要素 | 具体的内容 |
|-------|-----------|--------|-----------|
| 1 | 配置・回転・発射 | なし（チュートリアル） | 直線+1回反射、鏡1枚 |
| 2 | 連携反射 | **複数鏡の連携** | L字経路、鏡2枚で2回反射必須 |
| 3 | 迂回経路 | **壁ブロック** | レーザー遮断、壁を避けて鏡3枚で誘導 |
| 4 | 分岐到達 | **分光プリズム** | レーザーを2方向に分岐、2ゴール同時到達 |
| 5 | タイミング | **移動壁（開閉）** | 2秒周期で開閉する壁、発射タイミングの判断が必要 |

## 判断ポイントの実装設計

### トリガー条件と報酬/ペナルティ

1. **鏡の角度選択（毎配置/回転時）**
   - トリガー: 鏡をグリッドに配置 or タップで回転
   - 報酬: 正しい角度で最短経路 → 高スコア
   - ペナルティ: 間違った角度 → レーザーがゴールに届かない

2. **配置位置の選択（鏡ドラッグ時）**
   - トリガー: パーツ一覧から鏡をドラッグ開始
   - 報酬: 最適位置 → 余り鏡ボーナス +200点/枚
   - ペナルティ: 非最適位置 → 鏡を使い切りボーナスなし

3. **発射タイミング（Stage5）**
   - トリガー: 発射ボタン押下
   - 報酬: 移動壁が開いている時に発射 → クリア
   - ペナルティ: 壁が閉じている時に発射 → レーザー遮断、再配置必要
