# Design: Game032v2_SpinCutter

## namespace
`Game032v2_SpinCutter`

## スクリプト構成

### SpinCutterGameManager.cs
- ゲーム状態管理（WaitingInstruction / WaitingLaunch / Playing / StageClear / Clear / GameOver）
- StageManager・InstructionPanel統合
- スコア・コンボ管理
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] SpinCutterMechanic _mechanic`
- `[SerializeField] SpinCutterUI _ui`

### SpinCutterMechanic.cs
- 刃のスポーン・軌道計算（中心点を回転）
- 敵の生成・配置・当たり判定
- 障害物管理
- UIスライダーから半径・速度を受け取る
- `SetupStage(StageManager.StageConfig config, int stageIndex)` で敵数・障害物生成
- 発射管理（残り発射数カウント）
- `[SerializeField] SpinCutterGameManager _gameManager`
- 入力: UI Buttonのclickイベント（OnLaunchButtonClicked）
- **レスポンシブ配置**: Camera.main.orthographicSizeから動的計算

### BladeController.cs
- 刃の回転・移動制御
- 初期化時に半径・速度を受け取る
- 敵・障害物との当たり判定（OnTriggerEnter2D）
- 一定時間後または障害物接触後に消滅
- `BladeController.Initialize(float radius, float speed, float duration)`

### SpinCutterUI.cs
- スコア・ステージ・残弾数・残敵数表示
- 半径スライダー・速度スライダー値の表示
- 軌道プレビュー（LineRenderer）
- ステージクリアパネル・最終クリアパネル・ゲームオーバーパネル
- 発射ボタン

## 盤面・ステージデータ設計

### StageConfig利用
```csharp
// speedMultiplier: 刃の回転速度倍率
// countMultiplier: 敵数（直接利用、int変換）
// complexityFactor: 障害物有無(0=なし、0.25以上=障害物)、移動敵有無(0.5以上)
```

### ステージ別パラメータ
| Stage | speedMultiplier | countMultiplier | complexityFactor | 発射制限 |
|-------|----------------|-----------------|-----------------|---------|
| 1 | 1.0 | 3 | 0.0 | 3発 |
| 2 | 1.2 | 5 | 0.25 | 3発 |
| 3 | 1.3 | 6 | 0.4 | 3発 |
| 4 | 1.5 | 8 | 0.5 | 4発 |
| 5 | 1.7 | 10 | 0.6 | 5発 |

発射制限 = `Mathf.RoundToInt(2 + complexityFactor * 5)` で計算

## 入力処理フロー
1. UIスライダー（OnValueChanged）→ _mechanic.SetRadius(v) / _mechanic.SetSpeed(v)
2. 発射ボタン → _gameManager.OnLaunchPressed()
3. GameManager → _mechanic.LaunchBlade()

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5f
float camW = camSize * Camera.main.aspect;
float topMargin = 1.2f;
float bottomMargin = 3.5f; // スライダー+ボタン用に広めに確保
float gameAreaTop = camSize - topMargin;
float gameAreaBottom = -camSize + bottomMargin;
// 敵配置エリア: 上部70%をゲームエリア、下部30%はUI
```

## InstructionPanel設定
- title: "SpinCutter"
- description: "回転する刃の軌道を調整して敵を一掃しよう"
- controls: "スライダーで半径・速度を調整、ボタンで発射"
- goal: "できるだけ少ない発射回数で全敵を倒そう"

## SceneSetup構成方針

### `Setup032v2_SpinCutter.cs`
- MenuItem: `"Assets/Setup/032v2 SpinCutter"`
- カメラ: backgroundColor = (0.05, 0.05, 0.12) ダーク
- GameManager + StageManager(子) + SpinCutterMechanic(子)
- Canvas: 参照解像度1080x1920
- EventSystem: InputSystemUIInputModule

### UIレイアウト（Canvas内）
- **上部HUD**: Stage Text（左上）、Score Text（右上）、残弾Text（中央上）
- **中央**: 残敵Text
- **下部（Y=10〜20）**: 戻るボタン
- **下段（Y=65〜120）**: 発射ボタン（中央）
- **下段（Y=140〜200）**: 半径スライダー + ラベル
- **下段（Y=220〜280）**: 速度スライダー + ラベル
- スライダーは横幅80%で配置

### InstructionPanel + ステージクリアパネル
- InstructionPanel: Canvas上のフルスクリーンオーバーレイ（sortingOrder最前面）
- StageClearPanel: 「ステージクリア！」 + 「次のステージへ」ボタン
- GameOverPanel / FinalClearPanel も生成

## ビジュアルフィードバック設計
1. **敵撃破時**: SpriteRendererの色フラッシュ（赤→白→消滅）0.3秒
2. **コンボ達成時**: 刃のスケールパルス（1.0→1.4→1.0、0.2秒）
3. **刃の障害物接触時**: カメラシェイク（intensity=0.2、duration=0.3s）
4. **ゲームオーバー時**: カメラシェイク（intensity=0.5、duration=0.5s）

## スコアシステム
- 基本: 敵1体 = 100pt
- コンボ: 1発で3体=×1.5、5体=×2.0、全滅=×3.0
- ボーナス: 残り発射数×100pt
- 総合スコアはUIに表示

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stageIndex) {
    var config = _stageManager.GetCurrentStageConfig();
    _mechanic.SetupStage(config, stageIndex);
    // 残弾数も初期化
}
```

## Buggy Code防止事項
- Physics2D: タグ比較はgameObject.CompareTag()使用
- Update()での入力処理: _isActiveガードを入れる
- BladeController: OnDestroy()でspriteリソース解放不要（prefabではなく動的生成）
- TextureはOnDestroy()でDestroy(texture)
