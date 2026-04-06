# Design: Game047v2 SpinBalance

## namespace
`Game047v2_SpinBalance`

## スクリプト構成

### SpinBalanceGameManager.cs
- 状態管理: Playing / StageClear / AllClear / GameOver
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] BalanceManager _balanceManager`
- `[SerializeField] SpinBalanceUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed → StartGame()
- StartGame(): _stageManager.StartFromBeginning()
- _stageManager.OnStageChanged += OnStageChanged → BalanceManager.SetupStage(config)
- _stageManager.OnAllStagesCleared += OnAllStagesCleared
- スコア管理: 基本 + コンボ乗算
- ブレーキ未使用フラグ管理

### BalanceManager.cs
- コアメカニクス: 盤面回転制御 + コマ管理
- `[SerializeField] SpinBalanceGameManager _gameManager`
- `[SerializeField] GameObject _platformObj` (盤面)
- `[SerializeField] GameObject _coinPrefab`
- ドラッグ入力: Mouse.current.delta.ReadValue().x で盤面回転
- ダブルクリック: ブレーキ機能（0.3秒以内2回クリック検出）
- SetupStage(StageConfig config): ステージパラメータ適用
- コマ定期追加: config.spawnInterval 秒ごとにコインを生成
- 落下判定: コマのY座標が閾値以下になったらGameOver
- 盤面縮小: Stage5でタイマー駆動

**ステージパラメータ表**:
| Stage | stageDuration | maxCoins | addInterval | coinTypes | bounce | magnet | shrink |
|-------|--------------|----------|-------------|-----------|--------|--------|--------|
| 1 | 20s | 3 | 5s | normal | false | false | false |
| 2 | 30s | 5 | 4s | normal+heavy+light | false | false | false |
| 3 | 40s | 6 | 4s | normal+heavy+light+bounce | true | false | false |
| 4 | 50s | 7 | 3.5s | all+magnet | true | true | false |
| 5 | 60s | 8 | 3s | all | true | true | true |

StageConfig活用: speedMultiplier → 回転感度, countMultiplier → maxCoins

### SpinBalanceUI.cs
- `UpdateTimer(float remaining)`
- `UpdateScore(int score, float multiplier)`
- `UpdateCoinCount(int current, int max)`
- `ShowStageClear()`, `ShowAllClear()`, `ShowGameOver()`
- ブレーキアイコン表示（クールダウン中グレーアウト）
- 危険表示（コマが端から0.5u以内で赤フラッシュ）

## 盤面・ステージデータ設計
- Platform: GameObject with BoxCollider2D (width=6u, height=0.3u)
- Rigidbody2D.bodyType = Kinematic（物理エンジンに影響されず自分で回転）
- 盤面回転: `transform.Rotate(0, 0, rotateAmount)` でZ軸回転
- コマ: CircleCollider2D + Rigidbody2D (gravityScale=1)
- CoinType: Normal(mass=1), Heavy(mass=2), Light(mass=0.5), Bounce(bounciness=0.9), Magnet

## 入力処理フロー
```
Update() {
  if Mouse.leftButton.isPressed:
    delta = Mouse.current.delta.ReadValue()
    platform.Rotate(0, 0, -delta.x * rotSensitivity * Time.deltaTime * 60)
  
  if Mouse.leftButton.wasPressedThisFrame:
    doubleClickTimer += Time.deltaTime
    if timer < 0.3s: BrakeActivated()
    else: reset timer
}
```

## InstructionPanel内容
- title: "SpinBalance"
- description: "コマが落ちないように盤面をドラッグして回転させよう"
- controls: "左右にドラッグで回転 / ダブルクリックで緊急ブレーキ"
- goal: "制限時間が終わるまでコマを盤面上に保持し続けよう"

## ビジュアルフィードバック設計
1. **コマ追加演出**: コマ生成時に scale 0→1.3→1.0 のポップアニメ（0.3秒）
2. **危険フラッシュ**: コマが端から0.5u以内 → SpriteRenderer.color を赤に点滅
3. **ブレーキ発動**: カメラシェイク + 時間スロー効果（Time.timeScale=0.5, 0.5秒）
4. **スコア乗算達成**: コンボUIテキストのスケールパルス

## スコアシステム
- 基本: 1秒ごとに10点 × 倍率
- 倍率: 5個以上で×2、8個以上で×3
- コマ追加ボーナス: コマ数×100点
- ブレーキ未使用ボーナス: ステージクリア時500点
- ステージクリアボーナス: ステージ番号×500点

## SceneSetup構成方針
- Setup047v2_SpinBalance.cs
- MenuItem: "Assets/Setup/047v2 SpinBalance"
- Platform GameObject: BoxCollider2D + Rigidbody2DKinematic
- Coin Prefab: CircleCollider2D + Rigidbody2D を Resources に保存
- InstructionPanel: フルスクリーンオーバーレイ
- StageManager: GameManagerの子オブジェクト
- BalanceManager: GameManagerの子オブジェクト

## レスポンシブ配置
```csharp
float camSize = 5f; // orthographicSize
float camWidth = camSize * aspect;
// Platform: Y=0, Width=6u (盤面中央)
// 落下判定: Y < -camSize - 1f
// UI上部: anchored at top (-30 from top)
// ボタン下部: anchored at bottom (Y=10~80)
```

## ステージ別新ルール表
- Stage 1: 基本（同重量コマ3個、5秒ごと追加）
- Stage 2: 重さ違いコマ（Heavy/Light）追加、重心が偏る
- Stage 3: バウンドコマ追加（弾性0.9で跳ねる、予測不能）
- Stage 4: 磁石コマ追加（隣接コマへの引力/斥力ギミック）
- Stage 5: 盤面縮小（10秒ごとに10%縮小） ＋ 全要素複合

## 判断ポイントの実装設計
- 落下予測: コマの速度ベクトルと盤面端の距離から「あと何秒で落ちるか」を毎フレーム計算
- ブレーキ条件トリガー: 最大速度コマの落下まで1秒以内
- 報酬/ペナルティ: ブレーキ使用で即時安定化 vs クールダウン5秒でその間無防備

## Buggy Code防止チェック
- Physics2D比較はgameObject.nameまたはtagを使用
- _isActive ガードで複数Update競合防止
- Texture2D, Sprite動的生成なし（Pillow生成の静的アセット使用）
- OnDestroy()でイベント購読解除: OnStageChanged, OnAllStagesCleared, OnDismissed
