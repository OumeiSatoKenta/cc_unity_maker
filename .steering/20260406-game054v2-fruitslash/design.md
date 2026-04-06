# Design: Game054v2_FruitSlash

## Namespace
`Game054v2_FruitSlash`

## スクリプト構成

### FruitSlashGameManager.cs
- ゲーム状態管理 (Idle/Playing/StageClear/AllClear/GameOver)
- スコア・コンボ・ライフ管理
- StageManager・InstructionPanel統合
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] FruitManager _fruitManager`
- `[SerializeField] FruitSlashUI _ui`
- Start(): Show InstructionPanel → OnDismissed += StartGame
- StartGame(): SetConfigs → OnStageChanged/OnAllStagesCleared購読 → StartFromBeginning()
- OnStageChanged(int): FruitManager.SetupStage(config) + UI更新
- AddScore(int points, bool isMultiSlash): コンボ倍率計算してスコア加算
- OnFruitMissed(): 見逃しカウント、3回でライフ-1
- OnBombCut(): ライフ-1、コンボリセット
- OnIceFruitCut(): コンボリセット（ライフ減少なし）

### FruitManager.cs
- フルーツ・爆弾のスポーン・物理管理
- `[SerializeField] Sprite[] _fruitSprites` (apple, watermelon, gold, ice, bomb, bigBomb)
- スポーンはコルーチンで定期的に実行
- `SetupStage(StageManager.StageConfig config)`: スポーン間隔・速度・爆弾比率設定
- フルーツはRigidbody2Dで放物線（初速ベクトル設定）
- スワイプ切断判定: FruitSlashGameManagerからOnSwipe(Vector2 start, Vector2 end)で受信
- 切断済みフルーツはアニメーション（2つに分割エフェクト）後Destroy
- ステージ移行時に画面上のフルーツを全クリア

### FruitObject.cs（フルーツの個別データ）
- FruitType enum: Apple, Watermelon, Gold, Ice, Bomb, BigBomb
- `FruitType Type`
- `int Score`（Apple=10, Watermelon=30, Gold=100, Ice=15, Bomb/BigBomb=0）
- `bool IsSliced`

### FruitSlashUI.cs
- スコア・コンボ・ライフ・タイマー・ステージ表示
- StageClearPanel・GameOverPanel・AllClearPanel表示
- `[SerializeField] TextMeshProUGUI _scoreText`
- `[SerializeField] TextMeshProUGUI _comboText`
- `[SerializeField] GameObject[] _heartObjects` (3個)
- `[SerializeField] TextMeshProUGUI _timerText`
- `[SerializeField] TextMeshProUGUI _stageText`
- `[SerializeField] TextMeshProUGUI _targetScoreText`
- `[SerializeField] Slider _progressSlider`

## 入力処理フロー
FruitSlashGameManager.Update()で:
1. Mouse.current.leftButton.isPressed で押下中かチェック
2. Mouse.current.delta.ReadValue() でドラッグベクトル取得
3. 一定距離以上移動時にスワイプ軌跡を更新
4. Mouse.current.leftButton.wasReleasedThisFrame でリリース時に切断判定実行
5. Camera.main.ScreenToWorldPoint() でワールド座標変換
6. FruitManager.CheckSlash(worldStart, worldEnd) を呼び出し

## SceneSetup 構成方針
- `Setup054v2_FruitSlash.cs`
- Menu: `Assets/Setup/054v2 FruitSlash`
- カメラ: orthographic, size=5.5, background=casual緑系
- Sprite生成: Assets/Resources/Sprites/Game054v2_FruitSlash/ に保存

## StageManager統合

### OnStageChanged(int stage)
```csharp
void OnStageChanged(int stageIndex)
{
    State = GameState.Playing;
    _missCount = 0;
    _comboCount = 0;
    var config = _stageManager.GetCurrentStageConfig();
    _fruitManager.SetupStage(config);
    _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages, _targetScores[stageIndex]);
}
```

### ステージ別パラメータ表
| Stage | speedMultiplier | countMultiplier | complexityFactor | 意味 |
|-------|----------------|----------------|-----------------|------|
| 1 | 1.0 | 1 | 0.0 | 低速・1個ずつ・爆弾なし |
| 2 | 1.3 | 2 | 0.1 | 普通・2個同時・爆弾10% |
| 3 | 1.6 | 3 | 0.4 | やや速・3個同時・金フルーツ |
| 4 | 2.0 | 4 | 0.7 | 速い・4個同時・氷フルーツ |
| 5 | 2.5 | 5 | 1.0 | 高速・5個同時・巨大爆弾 |

complexityFactor: 0.0=爆弾なし, 0.1=爆弾10%, 0.4=金フルーツあり, 0.7=氷フルーツあり, 1.0=巨大爆弾あり

## InstructionPanel内容
- title: "FruitSlash"
- description: "飛んでくるフルーツをスワイプで切り爆弾は回避しよう！"
- controls: "画面をドラッグしてフルーツを切断。コンボで得点倍率アップ！"
- goal: "目標スコアに到達してステージクリア！爆弾は切らないで！"

## ビジュアルフィードバック設計
1. **スライスエフェクト**: 切断時にフルーツを左右2つに分割→スケールアニメーション(1.0→0.0, 0.3秒)
2. **コンボテキストポップ**: コンボ達成時にComboTextが1.0→1.5→1.0スケールパルス（0.2秒）+ 黄色フラッシュ
3. **爆弾ヒット**: SpriteRendererの赤フラッシュ + カメラシェイク(0.3秒)
4. **ライフ減少**: ハートアイコンがグレーアウト + スケール縮小アニメーション

## スコアシステム
- 基本スコア: Apple=10, Watermelon=30, Gold=100, Ice=15
- コンボ倍率: 5連=1.5x, 10連=2.0x, 20連=3.0x
- マルチスラッシュ(1スワイプで3個以上): 追加×2.0
- パーフェクトボーナス(ステージ内ミス0): 最終×1.5

## ステージ別新ルール表
- Stage 1: 基本（りんご・スイカのみ、爆弾なし）
- Stage 2: 爆弾登場（切ると-1ライフ、コンボリセット）
- Stage 3: 金フルーツ登場（小さく高得点100pt、速い）
- Stage 4: 氷フルーツ登場（切るとコンボリセット、ライフは減らない）
- Stage 5: 巨大爆弾登場（サイズ2倍、爆発エフェクト広範囲）

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5.5
float camWidth = camSize * Camera.main.aspect;
// スポーン範囲: X=[-camWidth*0.8, camWidth*0.8], Y=-camSize-1
// 飛行ピーク: Y=camSize*0.5 (上方)
// ゲーム領域: Y=[-camSize+2.5, camSize-1.2]
// UI下部マージン: 下端から2.8u確保
```

## Buggy Code防止チェック
- スワイプ判定は FruitManager の `CheckSlash` に一元化
- `_isActive` ガード: Update() でのスポーンは `_isSpawning` フラグで制御
- Texture2D/Sprite のクリーンアップは SceneSetup で生成しないため不要
- 物理はRigidbody2D使用（gravity使用、放物線は自然物理）
