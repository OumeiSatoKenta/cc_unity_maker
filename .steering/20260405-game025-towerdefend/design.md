# Design: Game025v2 TowerDefend

## namespace

`Game025v2_TowerDefend`

## スクリプト構成

### TowerDefendGameManager.cs
- ゲーム状態管理（WaitingInstruction / WavePrepare / WaveActive / StageClear / Clear / GameOver）
- StageManager・InstructionPanel統合
- スコア・突破数・インク量管理
- WaveManager・WallManagerとの連携

### WaveManager.cs
- Wave内の敵生成・管理
- 敵の進行パス計算（A*経路探索の簡易版、壁の座標を迂回）
- Wave完了検知 → GameManagerに通知

### WallManager.cs
- ドラッグ入力で壁を生成
- 壁はGrid上の区画として管理（グリッドセル単位）
- インク消費計算
- ダブルタップで壁を消去（インク50%回復）
- 破壊敵による壁の除去処理

### Enemy.cs
- 敵の種類（Normal/Fast/Flying/Breaker）
- 移動処理（Flyingは壁を無視）
- 破壊敵は壁に接触したら壁を壊す
- ゴール到達時に GameManager.OnEnemyReachedGoal() 呼び出し

### TowerDefendUI.cs
- インク残量バー表示
- Wave表示「Wave X / N」
- 突破数表示「突破 X / 5」
- スコア表示
- Waveスタートボタン
- ステージクリアパネル・ゲームオーバーパネル

## 盤面設計

- グリッドサイズ: 動的計算（orthographicSizeから）
- セルサイズ: 0.5ユニット
- ゲーム領域: 画面中央、下部2.8uのCanvas UI用マージン確保
- スタート地点: 左端中央 or 上端（ステージ5では2箇所）
- ゴール地点: 右端中央

```csharp
float camSize = Camera.main.orthographicSize;  // 5.0
float camWidth = camSize * Camera.main.aspect; // ~2.8
float topMargin = 1.2f;
float bottomMargin = 2.8f;
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// グリッド: 縦 ~6 行 × 横 ~10 列 (0.5u/cell)
```

## 入力処理フロー（WallManager）

```
Mouse.current.leftButton.isPressed → ドラッグ検知
→ Camera.main.ScreenToWorldPoint でワールド座標取得
→ グリッドセルに変換
→ セルに壁がない && インクあり → 壁を生成（インク消費）

MouseButton.wasPressedThisFrame → シングルタップ検知
（連続2回クリック検知でダブルタップ判定 0.3s以内）
→ Physics2D.OverlapPoint で壁コライダーを検出
→ 壁を消去、インク50%回復
```

## StageManager統合

```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stage) {
    var config = _stageManager.GetCurrentStageConfig();
    _waveManager.SetupStage(config, stage + 1);
    _wallManager.SetupStage(config, stage + 1);
    State = TowerDefendGameState.WavePrepare;
    // UI更新
}
```

StageManager パラメータ用途:
- `speedMultiplier`: 敵の移動速度倍率
- `countMultiplier`: 敵数の倍率
- `complexityFactor`: 新敵タイプの出現率（飛行/破壊）

## InstructionPanel内容

```csharp
_instructionPanel.Show(
    "025v2",
    "TowerDefend",
    "壁を描いて敵の侵入を阻止しよう！",
    "ドラッグで壁を描く。インクの残量に注意！壁をダブルタップすると消去できる",
    "全Waveの敵をゴールに到達させずに全5ステージをクリアしよう"
);
```

## ビジュアルフィードバック設計

1. **壁生成フラッシュ**: 壁を描くたびにセルが白くフラッシュ（0.1秒でフェード）
2. **敵撃退エフェクト**: 敵が壁に到達できずに消える際にスケールポップ（1.0→1.4→0、0.3秒）
3. **ゴール突破エフェクト**: カメラシェイク0.3秒 + UIの赤フラッシュ
4. **壁破壊エフェクト**: 破壊敵が壁に触れるとセルが爆発的に消去（スケール1.0→1.5→0、0.2秒）

## スコアシステム

- 敵1体撃退: 50pt
- Wave完封: +200pt × Wave番号
- インク残量ボーナス:
  - 残75%以上: x2.0
  - 残50%以上: x1.5
  - それ以外: x1.0
- 迂回ボーナス: 敵の実際の移動距離 ÷ 直線距離 × 10pt

## ステージ別新ルール

| Stage | 新要素 |
|-------|--------|
| 1 | 基本ルール（直線ルート、通常敵のみ、インク100%） |
| 2 | **高速敵が出現**（速度x2、壁迂回がより難しい） |
| 3 | **飛行敵が出現**（壁を無視して直進、別途ゴール前でのタイミング対策が必要） |
| 4 | **破壊敵が出現**（壁に触れると壁を破壊、再配置が必要） |
| 5 | **2方向侵入**（左と上の2箇所からスタート）+ 全種類混在 |

## SceneSetup構成方針

- Menu: `Assets/Setup/025v2 TowerDefend`
- グリッドベースのゲームフィールド（TilemapではなくSpriteRenderer Grid）
- スタート/ゴールマーカーをSpriteRendererで配置
- WaveManagerとWallManagerはGameManagerの子オブジェクト
- Canvas: ScreenSpaceOverlay, 1080x1920
- InstructionPanel、ステージクリアパネル、ゲームオーバーパネルをCanvas上に構成

## コーディング注意点

- `Camera.main` はキャッシュして使用
- `Texture2D` / `Sprite` は OnDestroy でクリーンアップ
- WaveActive中は壁描画を無効化（_isActive ガード）
- Flying敵は物理コライダーなし、直線移動のみ
- 壁グリッドは `HashSet<Vector2Int>` で管理（高速アクセス）
