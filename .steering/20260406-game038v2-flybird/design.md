# Design: Game038v2 FlyBird

## namespace
`Game038v2_FlyBird`

## スクリプト構成

| クラス | ファイル | 責務 |
|--------|---------|------|
| `FlyBirdGameManager` | FlyBirdGameManager.cs | ゲーム状態管理、StageManager/InstructionPanel統合、スコア・コンボ管理 |
| `BirdController` | BirdController.cs | 鳥の物理・タップ入力・アニメーション。GameManagerを `GetComponentInParent<FlyBirdGameManager>()` で取得 |
| `PipeSpawner` | PipeSpawner.cs | パイプ動的生成・移動・再利用（ステージ設定適用） |
| `FlyBirdUI` | FlyBirdUI.cs | UI表示管理（スコア・ステージ・コンボ・各パネル） |

## 盤面・ステージデータ設計

```csharp
// StageManager.StageConfig のカスタムパラメータとして speedMultiplier を利用
// PipeSpawner.SetupStage(StageManager.StageConfig config, int stageIndex) で設定
Stage 1: gapSize=3.5, speed=3.0, targetCount=5, hasCoin=false, hasMovingPipe=false, hasWind=false, hasRotatingPipe=false
Stage 2: gapSize=3.0, speed=3.5, targetCount=7, hasCoin=true, hasMovingPipe=false, hasWind=false, hasRotatingPipe=false
Stage 3: gapSize=2.6, speed=4.0, targetCount=8, hasCoin=true, hasMovingPipe=true, hasWind=false, hasRotatingPipe=false
Stage 4: gapSize=2.4, speed=4.5, targetCount=9, hasCoin=true, hasMovingPipe=true, hasWind=true, hasRotatingPipe=false
Stage 5: gapSize=2.0, speed=5.0, targetCount=10, hasCoin=true, hasMovingPipe=true, hasWind=true, hasRotatingPipe=true
```

## StageManager統合

```csharp
void StartGame() {
    _stageManager.OnStageChanged += OnStageChanged;
    _stageManager.OnAllStagesCleared += OnAllStagesCleared;
    _stageManager.StartFromBeginning();
}

void OnStageChanged(int stage) {
    // ステージ設定を PipeSpawner と BirdController に渡す
    _pipeSpawner.SetupStage(stage);
    _birdController.SetupStage(stage);
    _ui.UpdateStage(stage + 1);
    ResetState();
}
```

## InstructionPanel内容

```
title: "FlyBird"
description: "タップで鳥を飛ばして障害物を避けよう"
controls: "タップで羽ばたき、離すと降下"
goal: "障害物にぶつからずにゴールまで飛ぼう"
```

## 入力処理フロー

BirdController が入力を担当（GameManagerのstate==Playing時のみ動作）:
```csharp
// New Input System
using UnityEngine.InputSystem;
void Update() {
    if (!_isActive) return;
    if (Mouse.current.leftButton.wasPressedThisFrame) {
        _rb.linearVelocity = new Vector2(0, _flapForce); // 上向き力
    }
}
```

## SceneSetup 構成方針

- MenuPath: `Assets/Setup/038v2 FlyBird`
- GameManager root → StageManager(子), PipeSpawner(子), FlyBirdUI(子)
- BirdController は Bird GameObject に attach（GameManagerの子ではない）
- 背景スクロール: 2枚のBackground SpriteRenderer をループで移動

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize; // 5
float camWidth = camSize * Camera.main.aspect; // ~2.8 (9:16)
// 鳥の初期位置: x=-camWidth*0.5, y=0
// パイプ生成X: camWidth + 1.0 (画面右外)
// 地面: y = -camSize + 0.3
// 天井: y = camSize - 0.3
// ゲーム領域: y ∈ [-camSize+0.5, camSize-1.2]
```

## ビジュアルフィードバック設計

1. **パイプ通過時（成功）**: 鳥のスケールパルス (1.0→1.2→1.0, 0.15秒) + スコアテキストの点滅
2. **衝突時（失敗）**: 鳥のSpriteRenderer.colorを赤フラッシュ (白→赤→白, 0.2秒) + カメラシェイク
3. **コインコレクト**: コインがスケールダウンしながらフェードアウト (0.3秒)
4. **コンボ達成**: コンボテキストがスケールアップ (1.0→1.5→1.0, 0.3秒)

## スコアシステム

- パイプ通過: 10pt × コンボ乗算
- コンボ乗算: 5連続→x2、10連続→x3
- コイン取得: 5pt（乗算なし）
- ステージクリアボーナス: 100pt

## ステージ別新ルール表

| Stage | 新要素 | 実装詳細 |
|-------|--------|---------|
| 1 | 基本ルールのみ | 固定パイプ、広い隙間、低速 |
| 2 | コインアイテム | パイプ隙間の上下端付近にランダム配置（取るとリスク） |
| 3 | 移動パイプ | 一部のパイプが上下にSin波で移動（MovingPipe flag） |
| 4 | 突風エリア | 一定間隔で風力（y方向の力）が加わる視覚的エリア |
| 5 | 回転パイプ | 細長いパイプが中心軸で低速回転（RotatingPipe flag） |

## 判断ポイントの実装設計

**タップリズム判断**:
- 鳥のY速度 < -2.0 のとき「タップ推奨」（UI的には示さない、プレイヤーが感覚で学ぶ）
- タップすると瞬時に velocityY = +5.0 にリセット

**コイン取得 vs 安全飛行**:
- コインはパイプ隙間のmidY ± (gapSize/2 + 0.5) の位置に配置（隙間のすぐ外）
- 取るには通常の飛行軌道から ±0.5u ずれる必要がある

## Buggy Code防止チェック

- `_isActive` ガード: BirdController, PipeSpawner の Update() に設ける
- `OnDestroy()` でStageManagerイベント解除、生成したオブジェクト破棄
- 動的生成Texture2D/Sprite は `OnDestroy()` でクリーンアップ
- 固定座標ハードコーディング禁止: カメラサイズから動的計算

## 技術メモ

- Rigidbody2D を使用（gravity scale = 2.0, 制御はvelocity直接操作）
- Physics2D衝突判定は OnTriggerEnter2D で実装
- パイプは オブジェクトプール or Instantiate/Destroy（数が少ないのでDestroyで可）
