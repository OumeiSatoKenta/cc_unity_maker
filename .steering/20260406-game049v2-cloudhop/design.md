# Design: Game049v2 CloudHop

## namespace
`Game049v2_CloudHop`

## スクリプト構成

| クラス | ファイル | 役割 |
|--------|---------|------|
| `CloudHopGameManager` | `CloudHopGameManager.cs` | ゲーム状態管理・スコア・StageManager/InstructionPanel統合 |
| `CloudHopController` | `CloudHopController.cs` | プレイヤー制御（ジャンプ・左右移動・急降下）・コンボ管理・入力処理 |
| `CloudObject` | `CloudObject.cs` | 雲の種類・消失タイマー・効果（バネ/雷/動く） |
| `CloudSpawner` | `CloudSpawner.cs` | 雲の生成・縦スクロール管理・コイン生成 |
| `CoinObject` | `CoinObject.cs` | コインの当たり判定・収集処理 |
| `CloudHopUI` | `CloudHopUI.cs` | 高度・スコア・コンボ・コイン数・各パネル表示 |

## 盤面・ステージデータ設計

### StageConfig パラメータ活用
- `speedMultiplier`: 雲の消えるまでの時間の逆数（速いほど短命）
- `countMultiplier`: 雲の数（1.0 = 普通）
- `complexityFactor`: 新要素有効フラグ（0.0-1.0）

### ステージ別パラメータ
| Stage | speedMultiplier | countMultiplier | complexityFactor | 特記 |
|-------|--------------|----------------|-----------------|------|
| 1 | 1.0 | 1.3 | 0.0 | 通常雲のみ、4秒で消滅 |
| 2 | 1.2 | 1.2 | 0.2 | バネ雲追加（20%の確率） |
| 3 | 1.4 | 1.0 | 0.5 | 雷雲追加 |
| 4 | 1.6 | 0.9 | 0.7 | 動く雲追加 |
| 5 | 2.0 | 0.8 | 1.0 | ランダム消失 + 全要素 |

## 入力処理フロー

一元管理: `CloudHopController.cs`
- `Mouse.current.leftButton.wasPressedThisFrame` → ジャンプ（接地時のみ）
- `Mouse.current.delta.ReadValue()` → 左右移動（ドラッグ中）
- 下スワイプ検出: startDragY - currentY > しきい値 → 急降下

## SceneSetup 構成方針

`Setup049v2_CloudHop.cs` in `Assets/Editor/SceneSetup/`
- MenuItem: `Assets/Setup/049v2 CloudHop`
- カメラ: orthographic size=5, 背景は空のグラデーション
- Canvas: ScreenSpaceOverlay
- プレイヤーキャラ: 画面下部中央からスタート（y=-2.0付近）
- GameManager に StageManager（子）・CloudSpawner・CloudHopController・UI を配線

## StageManager 統合

```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stageIndex) {
    var config = _stageManager.GetCurrentStageConfig();
    _cloudSpawner.SetupStage(config, stageIndex + 1);
    _controller.SetupStage(config, stageIndex + 1);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

## InstructionPanel 内容

```csharp
_instructionPanel.Show(
    "049v2",
    "CloudHop",
    "消える雲を踏み台にして空高く跳ね上がろう",
    "タップでジャンプ！左右ドラッグで方向調整。下スワイプで急降下できるよ",
    "目標高度に到達してステージクリア！"
);
```

## ビジュアルフィードバック設計

1. **バネ雲ジャンプ**: キャラのスケールパルス（1.0 → 1.5 → 1.0, 0.15秒）+ 黄色フラッシュ
2. **雷雲感電**: カメラシェイク（0.1秒）+ キャラの赤フラッシュ + 「感電！」テキストポップ
3. **コイン取得**: コインのスケールアニメ + スコア加算テキスト（+100）がフロートアップ
4. **コンボ達成**: コンボ数テキストのポップアニメ + 画面縁の色フラッシュ（金色）
5. **雲の消失警告**: 消えかけの雲が点滅（alpha 1.0 ↔ 0.3、0.5秒周期）

## スコアシステム

| 要素 | 点数 |
|------|------|
| 高度ポイント | 1m あたり 10点 |
| コイン取得 | 100点 × コンボ乗算 |
| 雲ジャンプ連続コンボ | 3連続×2 / 5連続×3 / 10連続×5 |
| バネ雲使用 | 300点ボーナス |
| ステージクリアボーナス | 500点 + 余り高度×20 |

## ステージ別新ルール表

| Stage | 新要素 | 具体的な実装 |
|-------|--------|-------------|
| 1 | 基本ルール | 白い通常雲のみ。4秒で消失。コンボシステム導入 |
| 2 | バネ雲（SpringCloud） | 緑色の雲。着地時にジumpVelocity×2。コンボ+300点ボーナス |
| 3 | 雷雲（ThunderCloud） | 黒い雲。着地→1秒間入力無効（感電状態）→落下リスク |
| 4 | 動く雲（MovingCloud） | 一定速度で左右往復。速度は speedMultiplier に比例 |
| 5 | ランダム消失 | 通常の時間経過消失 + 1/3の確率でランダムタイミング消失 |

## 判断ポイントの実装設計

**バネ雲 vs 通常雲の選択**:
- トリガー: 視野内に通常雲とバネ雲の両方がある時
- バネ雲選択: 高得点・大ジャンプだが不安定（位置がランダム寄り）
- 通常雲選択: 安全・低得点

**コイン取得の判断**:
- コインはランダム横オフセット（±2ユニット）に配置
- 取得するとX位置がずれ、次の雲に届かないリスク
- 報酬: コイン100点 × コンボ乗算

**急降下タイミング**:
- 使用条件: 上の雲に届かない時（ジャンプ到達高度 < 次の雲Y）
- コストペナルティ: コンボリセット・高度後退
- ベネフィット: ゲームオーバー回避

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;      // 5.0
float camWidth = camSize * Camera.main.aspect;      // ~2.8
float topMargin = 1.2f;     // HUD用（ステージ・スコア表示）
float bottomMargin = 2.8f;  // CanvasUI用（ボタン）
// ゲーム領域: y = -2.2 ～ +3.8
// キャラ初期位置: y = -2.0
// 雲生成範囲: y = -1.5 ～ +4.5（スクロールで動的生成）
```

## キャラクター移動設計

- 重力: `Physics2D.gravity` または Rigidbody2D `gravityScale = 2.0f`
- ジャンプ力: Stage1=10, 以降 speedMultiplier で調整
- 左右移動速度: ドラッグdelta.x × 0.02（感度調整可）
- カメラ追従: プレイヤーが一定高度を超えたらカメラをスムーズに上昇
- ゲームオーバー判定: プレイヤーY < カメラY - camSize（画面外）

## SceneSetup フィールド配線リスト

GameManager に配線が必要なフィールド:
- `_stageManager` → StageManager コンポーネント
- `_instructionPanel` → InstructionPanel コンポーネント
- `_cloudSpawner` → CloudSpawner コンポーネント
- `_controller` → CloudHopController コンポーネント
- `_ui` → CloudHopUI コンポーネント

CloudHopController に配線:
- `_gameManager` → CloudHopGameManager
- `_playerSprite` → プレイヤー SpriteRenderer

CloudSpawner に配線:
- `_gameManager` → CloudHopGameManager
- `_cloudSprites` → 雲種別スプライト配列
- `_coinSprite` → コインスプライト
