# Design: Game035v2_WaveRider

## namespace
`Game035v2_WaveRider`

## スクリプト構成

### WaveRiderGameManager.cs
- ゲーム状態: WaitingInstruction / Playing / StageClear / Clear / GameOver
- フィールド:
  - `[SerializeField] StageManager _stageManager`
  - `[SerializeField] InstructionPanel _instructionPanel`
  - `[SerializeField] WaveMechanic _mechanic`
  - `[SerializeField] WaveRiderUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager.SetConfigs() → StartFromBeginning()
- OnStageChanged(int): mechanic.SetupStage(config, stageIndex)
- OnAllStagesCleared(): mechanic.Deactivate() → ui.ShowFinalClear()
- スコア管理: _score, _combo (コンボ乗算 x1〜x5)
- public void OnTrickSuccess(bool isPerfect): スコア加算
- public void OnHitObstacle(): GameOver遷移
- public void OnBalanceFailed(): GameOver遷移
- public void OnStageGoalReached(): StageClear遷移 → stageManager.CompleteCurrentStage()

### WaveMechanic.cs
- **入力一元管理**: Mouse.current 使用
  - using UnityEngine.InputSystem;
- フィールド:
  - `[SerializeField] WaveRiderGameManager _gameManager`
  - `[SerializeField] Transform _surferTransform`
  - `[SerializeField] GameObject _rockPrefab`
  - `[SerializeField] GameObject _whirlpoolPrefab`
  - `[SerializeField] SpriteRenderer _surferRenderer`
  - `[SerializeField] SpriteRenderer _backgroundRenderer`
- レーン管理: 左/中/右の3レーン (-1.5f, 0f, 1.5f)
- _currentLane: 0〜2 (初期=1=中央)
- _isActive: ゲーム中のみ処理
- Wave管理: sinカーブで波の高さをシミュレート
  - _waveTime: float、毎フレーム加算
  - WaveY = Mathf.Sin(_waveTime * waveFrequency) * waveAmplitude
- ジャンプ: _isJumping bool、_jumpTime float
  - タップ時、WaveY > threshold なら perfect（2倍スコア）
- バランス: _balance (0〜100)、傾き操作で減少・時間で回復
  - →廃止し、シンプルに「岩ヒット=ゲームオーバー」に変更（実装複雑度削減）
- 障害物生成:
  - 画面上方から生成、下方向にスクロール
  - _spawnInterval で間隔管理
- SetupStage(StageConfig config, int stageIndex):
  - speedMultiplier → scrollSpeed
  - countMultiplier → maxObstacles
  - stageIndex >= 2 → whirlpoolEnabled
  - stageIndex >= 3 → shieldEnabled
  - stageIndex >= 4 → stormEnabled
- 距離管理: _distanceTraveled, _goalDistance → OnStageGoalReached呼び出し
- レスポンシブ配置:
  ```csharp
  float camSize = Camera.main.orthographicSize; // 5f
  float camW = camSize * Camera.main.aspect;
  float laneSpacing = camW * 0.5f; // 3レーン間隔
  float laneY = -camSize + 2.5f; // サーファーのY座標（下部マージン確保）
  ```
- ビジュアルフィードバック:
  - トリック成功: _surferTransform のスケールパルス (1.0→1.4→1.0、0.2秒)
  - 岩ヒット: SpriteRenderer 赤フラッシュ + カメラシェイク (0.3秒)
  - パーフェクト: 黄色グロー演出

### WaveRiderUI.cs
- 必須表示:
  - ステージ表示「Stage X / 5」
  - 距離「残り Nm」
  - スコア表示
  - コンボ表示「xN COMBO」
  - ステージクリアパネル + 「次のステージへ」ボタン
  - 最終クリアパネル
  - ゲームオーバーパネル + 「もう一度」ボタン
  - メニューへ戻るボタン
- ボタンはUnityEventで登録

## StageManager統合

StageConfig設定:
```
Stage1: speed=1.0, count=5,  complexity=0.0, name="Stage 1 - 穏やかな海"
Stage2: speed=1.3, count=6,  complexity=0.2, name="Stage 2 - 岩登場"
Stage3: speed=1.6, count=8,  complexity=0.5, name="Stage 3 - 渦巻き"
Stage4: speed=2.0, count=10, complexity=0.7, name="Stage 4 - シールド"
Stage5: speed=2.5, count=13, complexity=1.0, name="Stage 5 - 嵐"
```

OnStageChanged → mechanic.SetupStage(config, stageIndex)
OnAllStagesCleared → 全クリア処理

## SceneSetup: Setup035v2_WaveRider.cs

- `[MenuItem("Assets/Setup/035v2 WaveRider")]`
- カメラ: backgroundColor=深い青 (0.05f, 0.10f, 0.20f), orthographicSize=5f
- スプライト使用:
  - Background.png (海面スクロール背景)
  - Surfer.png (サーフボード+キャラクター)
  - Rock.png (岩)
  - Whirlpool.png (渦巻き)
  - Wave.png (波ライン)
  - Shield.png (シールドエフェクト)
- GameManagerObject: WaveRiderGameManager + StageManager (子)
- Surfer GameObject: サーファーのスプライト + WaveMechanic
- Rock/Whirlpool プレハブ
- InstructionPanel: Canvas上フルスクリーン
- StageClearPanel, FinalClearPanel, GameOverPanel
- ボタン最小サイズ: sizeDelta (150, 55)

## InstructionPanel 4テキスト

- title: "WaveRider"
- description: "波に乗ってトリックを決めながらゴールを目指そう"
- controls: "左右タップでレーン移動、タップでジャンプ・トリック"
- goal: "岩を避けてゴールまで走破しよう"

## ビジュアルフィードバック

1. **トリック成功**: スケールパルス (1.0→1.4→1.0、0.25秒 Coroutine)
2. **パーフェクトジャンプ**: 黄色フラッシュ (SpriteRenderer.color → 黄→白、0.3秒)
3. **岩ヒット**: 赤フラッシュ + カメラシェイク (0.3秒)
4. **コンボ**: コンボ数が増えるたびにUIテキストスケールバウンス

## スコアシステム

- 通常トリック: 50pt × コンボ乗算
- パーフェクトトリック（波頂点±0.2秒以内）: 100pt × コンボ乗算
- コンボ乗算: min(combo, 5) = x1〜x5
- 走破ボーナス: 走破距離 × 1pt（毎フレーム加算）

## レスポンシブ配置

```csharp
float camSize = 5f;
float camW = camSize * aspect;
// サーファーY: -camSize + 2.5f = -2.5f (下部2.5uマージン確保)
// レーン間隔: camW * 0.45f ≒ 1.8f (16:9時)
float laneSpacing = camW * 0.45f;
float[] laneXPositions = { -laneSpacing, 0f, laneSpacing };
```

## ステージ別新ルール

- Stage 1: 岩なし、穏やかな波、ジャンプ練習
- Stage 2: 静止岩（3個）が画面スクロールと共に登場
- Stage 3: 渦巻き追加（近づくと横方向に引き寄せ力が発生）
- Stage 4: トリックコンボ3連続でシールド獲得（岩1回無効化）
- Stage 5: 嵐エフェクト（_stormTimer でカメラ暗転を定期的に実行）
