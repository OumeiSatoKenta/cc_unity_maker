# Design: Game031v2_BounceKing

## namespace

`Game031v2_BounceKing`

## スクリプト構成

| クラス | ファイル | 役割 |
|-------|---------|------|
| `BounceKingGameManager` | BounceKingGameManager.cs | ゲーム状態管理、StageManager/InstructionPanel統合、スコア管理 |
| `PaddleController` | PaddleController.cs | パドルのドラッグ操作、移動範囲制限 |
| `BallController` | BallController.cs | ボールの物理移動（Rigidbody2D不使用・手動計算）、壁/パドル/ブロックとの衝突 |
| `BlockManager` | BlockManager.cs | ブロック配置（ステージ別）、アイテムドロップ制御、全破壊通知 |
| `Block` | Block.cs | 個別ブロックの状態（耐久値、色）、ヒット処理 |
| `ItemController` | ItemController.cs | アイテムの落下・取得・効果適用 |
| `BounceKingUI` | BounceKingUI.cs | スコア・ライフ・ステージ・コンボ表示、各種パネル |

## 物理設計（Rigidbody2D不使用）

ボールは `Vector2 velocity` を自前で持ち、`Update()` で `transform.position += velocity * Time.deltaTime` で移動。衝突検出は `Physics2D.OverlapCircle` / `Physics2D.Raycast` で行う。

壁・パドル・ブロックの境界をコライダー（BoxCollider2D）で設定し、Raycastで衝突方向を判定して速度ベクトルを反射。

**ボール速度管理**: ステージ別の速度倍率を `speedMultiplier` で適用。速度が大きく変わっても方向ベクトルの正規化を保持。

## 盤面・ステージデータ設計

```
StageConfig.countMultiplier → ブロック行数
StageConfig.speedMultiplier → ボール速度倍率
StageConfig.complexityFactor → 硬ブロック割合（0.0〜1.0）
```

### ブロックグリッド配置

- グリッド配置: カメラ座標から動的計算
- `camSize = Camera.main.orthographicSize`
- `camWidth = camSize * aspect`
- `topMargin = 1.5f` (HUD用)
- `bottomMargin = 3.0f` (UI/パドル用)
- `availableHeight = camSize * 2 - topMargin - bottomMargin`
- ブロックサイズ: availableHeight をステージの行数で等分

### ブロック種別

| 種別 | 耐久値 | 色 | ドロップ |
|------|--------|---|---------|
| Normal | 1 | 緑グラデーション | 20%確率でアイテム |
| Hard | 2 | 赤グラデーション | 40%確率でアイテム |
| Boss | 4 | 紫グラデーション | 必ずアイテム |

### アイテム種別

| アイテム | 効果 | 持続 |
|---------|------|------|
| PaddleExpand | パドル幅1.5倍 | 15秒 |
| MultiBall | ボールを3個に分裂 | 残りボール数に応じ |
| PaddleShrink | パドル幅0.7倍（トラップ） | 10秒 |

## 入力処理フロー

```
BallController (Update)
  ├── タッチ/クリック開始 → ボール発射（待機中のみ）
  └── ドラッグ → PaddleController へ委譲

PaddleController (Update)
  └── マウス/タッチX座標を読み取り → パドルX位置を更新（範囲クランプ）
```

- 新Input System: `Mouse.current.position.ReadValue()`, `Mouse.current.leftButton`
- `using UnityEngine.InputSystem;`

## SceneSetup 構成方針

`Assets/Editor/SceneSetup/Setup031v2_BounceKing.cs`
- MenuItem: `"Assets/Setup/031v2 BounceKing"`
- カメラ: orthographic, size=5, 背景色=ダーク青
- ゲームオブジェクト構成:
  - Walls (Top/Left/Right) - BoxCollider2D
  - Paddle - BoxCollider2D + PaddleController
  - Ball (初期1個) - CircleCollider2D + BallController
  - BlockManager - BlockManager
  - GameManager - BounceKingGameManager
  - StageManager (GameManagerの子)
  - Canvas / EventSystem (InputSystemUIInputModule)
  - InstructionPanel (Canvas上のオーバーレイパネル)

## StageManager統合

```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stageIndex) {
    // BlockManagerでブロック再配置
    _blockManager.SetupStage(_stageManager.GetCurrentStageConfig());
    // ボールリセット
    _ballController.Reset();
    // UI更新
    _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
}
```

## InstructionPanel内容

```
gameId: "031v2"
title: "BounceKing"
description: "パドルでボールを打ち返してブロックを壊そう"
controls: "パドルをドラッグで左右に動かす。タップでボール発射"
goal: "全てのブロックを壊してステージクリア！"
```

## ビジュアルフィードバック設計

1. **ブロック破壊フラッシュ**: ブロック破壊時に白フラッシュ（0.1秒）してから消滅
   - SpriteRenderer.color を白にしてから `Destroy(gameObject, 0.1f)`
2. **カメラシェイク**: ライフ消失時（ボールを落とした時）0.3秒シェイク
3. **コンボテキストポップ**: コンボ更新時にスケール1.0→1.5→1.0アニメーション（0.3秒）
4. **パドルヒットパルス**: ボールがパドルに当たった時にパドルが0.1秒だけ明るくなる

## スコアシステム

- 通常ブロック破壊: 10pt × コンボ倍率
- 硬ブロック破壊: 30pt × コンボ倍率
- ボスブロック破壊: 50pt × コンボ倍率
- コンボ倍率: 1x (0-4連続), 1.5x (5-9連続), 2.0x (10-19連続), 3.0x (20+連続)
- ステージクリアボーナス: 残りライフ × 200pt
- ボールを落とすとコンボリセット

## ステージ別新ルール表

| ステージ | 行×列 | 速度 | 新要素 |
|---------|-------|------|-------|
| Stage 1 | 3×8=24 | 5.0 | 基本（通常ブロックのみ） |
| Stage 2 | 4×8=32 | 6.0 | 硬ブロック登場（行全体が赤で2ヒット必要） |
| Stage 3 | 5×8=40 | 6.5 | パドル拡大アイテムドロップ |
| Stage 4 | 5×9=45 | 7.5 | マルチボールアイテム登場 |
| Stage 5 | 6×9=54 | 8.5 | 縮小パドルトラップ + ボスブロック（4ヒット） |

## 判断ポイントの実装設計

### アイテム取得判断
- トリガー: ブロック破壊時に `Random.value < dropRate` でアイテム生成
- アイテムは `3.0 units/sec` で下落
- プレイヤーはパドルでキャッチ or 無視
- 無視した場合: 画面下で消滅（ペナルティなし）
- 取得した場合: 即座に効果発動

### 反射角度計算
```csharp
// パドルの中心からの相対位置で反射角を決定
float hitOffset = (ball.x - paddle.center.x) / (paddle.width / 2);
// hitOffset: -1.0(左端) 〜 +1.0(右端)
float angle = hitOffset * 70f; // 最大70度
Vector2 newDir = new Vector2(Mathf.Sin(angle * Deg2Rad), Mathf.Cos(angle * Deg2Rad));
```

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;
float bottomMargin = 3.0f;
float gameAreaTop = camSize - topMargin;
float gameAreaBottom = -camSize + bottomMargin;

// パドル位置
paddleY = gameAreaBottom + 0.5f;

// ブロック配置
float blockAreaHeight = gameAreaTop - (gameAreaBottom + 1.0f);
float blockHeight = blockAreaHeight / rows;
float blockWidth = (camWidth * 2f) / cols;
```
