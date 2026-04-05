# Design: Game014v2 MagnetPath

## namespace
`Game014v2_MagnetPath`

## スクリプト構成

### MagnetPathGameManager.cs
- ゲーム状態管理: `WaitingInstruction / Playing / BallMoving / StageClear / Clear / GameOver`
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] MagnetManager _magnetManager
- [SerializeField] MagnetPathUI _ui
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): _stageManager.StartFromBeginning()
- OnStageChanged(int stage): MagnetManager.SetupStage(config)
- OnAllStagesCleared(): FullClear
- スコア/コンボ管理

### MagnetManager.cs
- 磁石グリッドの管理
- 鉄球のSimulation（物理ではなくUpdate内で磁力計算）
- SetupStage(StageManager.StageConfig config)
- 磁石タップ処理: Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint
- LaunchBall(): 鉄球発射
- ResetBall(): 鉄球リセット
- レスポンシブ配置: Camera.main.orthographicSize ベースで動的計算

### MagnetPathUI.cs
- HUD表示: ステージ/スコア/残り切替回数
- ステージクリアパネル/全クリアパネル/ゲームオーバーパネル
- ボタンUnityEvent登録

## 盤面・ステージデータ設計

```
StageConfig (StageManager):
  - speedMultiplier (鉄球移動速度係数)
  - countMultiplier (使用可能切替数係数)
  - complexityFactor (ステージ番号ベース)
```

各ステージの磁石レイアウトはMagnetManagerが静的データとして持つ:
```csharp
static readonly StageLayoutData[] StageLayouts = {
  // Stage 1: 2磁石直線
  // Stage 2: 4磁石L字 + 壁
  // Stage 3: 6磁石 + 強弱磁石
  // Stage 4: 8磁石 + スイッチ磁石
  // Stage 5: 10磁石 + 2球
};
```

## ステージ別パラメータ表
| Stage | speedMultiplier | countMultiplier | complexityFactor |
|-------|----------------|-----------------|-----------------|
| 1 | 0.8 | 1.0 (上限5) | 1.0 |
| 2 | 1.0 | 1.0 (上限6) | 1.5 |
| 3 | 1.2 | 1.0 (上限8) | 2.0 |
| 4 | 1.4 | 1.0 (上限10) | 2.5 |
| 5 | 1.6 | 1.0 (上限12) | 3.0 |

## 磁力シミュレーション設計
- 鉄球はRigidbody2DではなくUpdate内でカスタム移動
- 各磁石から鉄球への力を計算: `F = strength / distance²`
- N極(引力): 鉄球を引き寄せる方向
- S極(斥力): 鉄球を遠ざける方向
- 大きい磁石(ステージ3+): strength × 1.5、影響半径 × 1.5

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;    // HUD
float bottomMargin = 3.0f; // ボタン
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// グリッドサイズ5×5を最大として配置
float cellSize = Mathf.Min(availableHeight / 5f, camWidth * 2f / 5f, 1.2f);
```

## InstructionPanel内容
- title: "MagnetPath"
- description: "磁石の極性を切り替えて鉄球をゴールに導こう"
- controls: "磁石をタップでN/S切替 → スタートで鉄球発射"
- goal: "少ない切替回数で鉄球をゴールに到達させよう"

## ビジュアルフィードバック
1. **磁石切替時**: スケールパルス（1.0→1.3→1.0、0.15秒）+ N極なら赤フラッシュ、S極なら青フラッシュ
2. **鉄球ゴール到達**: パーティクル風の白フラッシュ + カメラシェイク0.2秒
3. **ゲームオーバー(盤面外)**: カメラシェイク + 赤フラッシュ
4. **コンボ演出**: ステージクリア時に連続コンボ数をテキストでポップ表示

## SceneSetup構成方針
- MenuItem: "Assets/Setup/014v2 MagnetPath"
- カメラ: orthographic size=5, 背景色=#1A1A2E
- Canvas: ScreenSpaceOverlay, 1080×1920, ScaleWithScreenSize
- GameManager GameObject → 子に StageManager, MagnetManager
- InstructionPanel: フルスクリーンオーバーレイ
- HUD配置:
  - StageText: 上部左 (anchoredPosition Y=-30)
  - ScoreText: 上部右 (anchoredPosition Y=-30)
  - SwitchCountText: 上部中央 (anchoredPosition Y=-60)
- 操作ボタン:
  - StartButton: 下部左 (Y=80), sizeDelta=(200,60)
  - ResetButton: 下部右 (Y=80), sizeDelta=(200,60)
  - MenuButton: 最下部中央 (Y=20), sizeDelta=(180,55)
- ステージクリアパネル/ゲームオーバーパネル/全クリアパネル

## 判断ポイントの実装
- 切替回数上限到達時: 「これ以上切り替えられません」テキスト表示、スタートボタンのみ有効化
- 発射中は磁石タップ不可（_isMoving フラグ）
- リセット時は切替回数リセット（ただしステージ内でのトータルは保持）

## Buggy Code防止
- using UnityEngine.InputSystem; を明記
- Mouse.current.position.ReadValue() を使用（Input.mousePosition 禁止）
- _isMoving フラグで Update() の入力処理をガード
- Texture2D/Sprite は OnDestroy() でクリーンアップ
- タグ/レイヤー比較は gameObject.name or layer番号を使用
