# Design: Game029v2_MeteorShield

## namespace

`Game029v2_MeteorShield`

## スクリプト構成

| クラス | ファイル | 責務 |
|-------|--------|------|
| MeteorShieldGameManager | MeteorShieldGameManager.cs | ゲーム状態管理・スコア・StageManager/InstructionPanel統合 |
| ShieldController | ShieldController.cs | シールドの入力処理・移動・当たり判定・跳ね返しロジック |
| MeteorSpawner | MeteorSpawner.cs | 隕石の生成・ステージ別難易度制御 |
| MeteorObject | MeteorObject.cs | 個別隕石の移動・HP・タイプ(小/大/分裂) |
| MeteorShieldUI | MeteorShieldUI.cs | HUD表示（星HP・スコア・コンボ・時間） |

## 盤面・ステージデータ設計

### ステージ別パラメータ表

| Stage | speedMultiplier | spawnInterval | largeRatio | splitRatio | angleRatio | groupSpawn |
|-------|-----------------|---------------|------------|------------|------------|-----------|
| 1     | 1.0             | 2.0s          | 0          | 0          | 0          | false |
| 2     | 1.25            | 1.5s          | 0          | 0          | 0.3        | false |
| 3     | 1.4             | 1.2s          | 0.15       | 0          | 0.3        | false |
| 4     | 1.6             | 0.9s          | 0.15       | 0.2        | 0.3        | false |
| 5     | 1.8             | 0.5s(群れ)    | 0.15       | 0.2        | 0.3        | true |

### StageManager StageConfig マッピング
- speedMultiplier → 落下速度倍率
- countMultiplier → スポーン密度（1=2s間隔ベース）
- complexityFactor → 0=小隕石のみ、0.3=斜め追加、0.5=大隕石追加、0.7=分裂追加、1.0=群れ

## 入力処理フロー

ShieldController.Update():
1. Mouse.current.leftButton.isPressed を確認
2. Mouse.current.position.ReadValue() でX座標取得
3. Camera.main.ScreenToWorldPoint() でワールド座標変換
4. ShieldのX座標を目標X座標に向けてLerp移動（シャープ係数15）
5. ShieldのX座標を画面端でクランプ

## 当たり判定フロー

MeteorObject がShieldのCollider2Dに触れたとき:
1. OnTriggerEnter2D で検出
2. ShieldControllerがMeteorを跳ね返す（Y速度反転 + ランダム角度補正）
3. GameManagerにOnMeteorDeflected(meteorType)通知
4. 跳ね返した隕石が別の隕石に当たるとOnChainKill()通知

星にMeteorが到達したとき:
1. Star がOnTriggerEnter2Dで検出
2. GameManagerにOnStarHit(damage)通知
3. HP減少、HPがゼロでGameOver

## SceneSetup の構成方針

Setup029v2_MeteorShield.cs を Assets/Editor/SceneSetup/ に作成

### 生成するオブジェクト構成
```
Main Camera (orthographicSize=5, 黒宇宙背景)
Background (SpriteRenderer, sortOrder=-10)
Star (SpriteRenderer + CircleCollider2D, 画面下部中央 y=-3.5)
Shield (SpriteRenderer + BoxCollider2D, y=-2.5)
GameManager
  ├── StageManager
  ├── ShieldController (ShieldSR, ShieldCollider参照)
  ├── MeteorSpawner (GameManager参照)
  └── MeteorShieldUI
Canvas (ScreenSpaceOverlay, 1080x1920)
  ├── HPBar (Slider, 上部)
  ├── ScoreText (右上)
  ├── ComboText (中央上)
  ├── TimeText (左上)
  ├── InstructionPanel (フルスクリーンオーバーレイ)
  ├── StageClearPanel (非表示)
  ├── FinalClearPanel (非表示)
  ├── GameOverPanel (非表示)
  ├── MenuButton (左下 Y=30)
  └── QuestionButton (右下 Y=30)
EventSystem (InputSystemUIInputModule)
```

## StageManager統合

GameManagerのStart():
1. _instructionPanel.Show(gameId, title, desc, controls, goal)
2. _instructionPanel.OnDismissed += StartGame

StartGame():
1. _stageManager.SetConfigs([5ステージの設定])
2. _stageManager.OnStageChanged += OnStageChanged
3. _stageManager.OnAllStagesCleared += OnAllStagesCleared
4. _stageManager.StartFromBeginning()

OnStageChanged(int stageIndex):
1. ステージ番号更新
2. _meteorSpawner.SetupStage(config, stageIndex+1)でスポーン設定変更
3. HPを回復しない（ライフ継続型）
4. UIのステージ表示更新

ステージクリア条件:
- 各ステージは時間制（30秒×ステージ番号）
- MeteorSpawnerがタイマーを管理し、時間経過でGameManagerにOnStageTimeUp()通知
- GameManagerが_stageManager.CompleteCurrentStage()を呼ぶ

## InstructionPanel内容

```csharp
_instructionPanel.Show(
    "029v2",
    "MeteorShield",
    "落下する隕石をシールドで弾いて星を守るディフェンスゲーム！",
    "画面をドラッグしてシールドを左右に動かそう",
    "星のHPがゼロになる前にできるだけ長く守り続けよう！"
);
```

## ビジュアルフィードバック設計

1. **隕石弾き返し成功**: ShieldのスケールパルスY方向（1.0→1.2→1.0、0.15秒）+ 白フラッシュ
2. **星へのダメージ**: カメラシェイク（0.3f、0.4秒）+ 星の赤フラッシュ
3. **連鎖撃墜**: 連鎖した隕石位置に黄色フラッシュエフェクト（スプライトスケールアニメ）
4. **コンボアップ**: ComboTextのスケールパルス（1.0→1.4→1.0）

## スコアシステム

- 隕石弾き返し: +20pt
- 連鎖撃墜ボーナス: +100pt × コンボ倍率
- コンボ倍率: 3連=x1.5、5連=x2.0、10連=x3.0
- 生存ボーナス: 30秒ごとに+300pt
- ミス（星がダメージ）でコンボリセット

## ステージ別新ルール表

| Stage | 新要素 |
|-------|--------|
| 1     | 基本のみ（小隕石、真上落下） |
| 2     | 斜め落下隕石（画面端から角度付き） |
| 3     | 大隕石（HP=2、弾き返し2回必要） |
| 4     | 分裂隕石（弾くと3小隕石に分裂） |
| 5     | 隕石群（5-8個同時スポーン）+ 全種類混在 |

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize; // 5f
float camWidth = camSize * Camera.main.aspect; // 約2.8f
float topMargin = 1.5f;    // HUD用
float bottomMargin = 3.0f; // ゲームUI（Star位置）
// シールドX範囲: -(camWidth-0.8f) 〜 +(camWidth-0.8f)
// Star位置: y = -(camSize - 1.5f) = -3.5f
// Shield位置: y = -(camSize - 2.5f) = -2.5f
```

## 判断ポイントの実装設計

### 複数隕石への対応
- MeteorSpawnerが複数隕石を同時管理（List<MeteorObject>）
- プレイヤーは画面全体を見渡して優先順位を決定する
- 各隕石はy座標が小さいほど危険（星に近い）を視覚的に表現（色変化: 遠=白、近=赤）

### 跳ね返し角度
- シールドとの衝突点に応じて反射角度を計算（シールド中心から遠いほど大きな角度）
- 跳ね返した隕石が`Meteor`タグを持つ別オブジェクトに当たるとChainKill発動

### 分裂隕石のタイミング
- 分裂隕石は弾き返した瞬間に分裂処理
- 分裂した小隕石はそれぞれ別の方向へ飛ぶ
