# Design: Game015v2_TileTurn

## スクリプト構成

### namespace: `Game015v2_TileTurn`

| クラス | ファイル | 役割 |
|--------|---------|------|
| `TileTurnGameManager` | TileTurnGameManager.cs | ゲーム状態管理・StageManager/InstructionPanel統合 |
| `TileManager` | TileManager.cs | コアメカニクス・タイル生成・回転入力処理・ステージ設定 |
| `TileCell` | TileCell.cs | 個別タイルのデータ・挙動（連動/ロック/反転判定） |
| `TileTurnUI` | TileTurnUI.cs | HUD表示・パネル管理 |

## 盤面・ステージデータ設計

### TileCell の状態
```csharp
enum TileType { Normal, Linked, Locked, Flipped }
int correctRotation;    // 正解の回転（0のみ、タイルはrotation=0が正解向き）
int currentRotation;    // 現在の回転 0/1/2/3 (×90度)
TileType tileType;
bool isLocked;
```

### ステージ設定（StageManager.StageConfig利用）
- `speedMultiplier` → 連動タイル割合（0=なし、0.3=30%が連動）
- `countMultiplier` → グリッドサイズ（2/3/4/5）
- `complexityFactor` → ロック/反転タイル割合

| Stage | gridSize | maxRotations | linkedRatio | lockedRatio | flippedRatio |
|-------|---------|-------------|------------|------------|-------------|
| 1 | 2 | 16 | 0.0 | 0.0 | 0.0 |
| 2 | 3 | 30 | 0.25 | 0.0 | 0.0 |
| 3 | 4 | 52 | 0.2 | 0.2 | 0.0 |
| 4 | 4 | 48 | 0.0 | 0.0 | 0.25 |
| 5 | 5 | 80 | 0.2 | 0.15 | 0.15 |

## 入力処理フロー

```
Update() → Mouse.current.leftButton.wasPressedThisFrame
  → Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue())
  → Physics2D.OverlapPoint(worldPos)
  → ヒットしたCollider2DのTileCellを取得
  → TileType判定
    - Locked: 何もしない
    - Flipped: FlipTile()（左右反転）
    - Normal/Linked: RotateTile() → 連動タイルにも伝播
  → 回転カウンター+1
  → 全タイル正解チェック → StageClear通知
  → 回転上限チェック → GameOver通知
```

## SceneSetup構成方針

`Setup015v2_TileTurn.cs` を `Assets/Editor/SceneSetup/` に作成。

MenuItem: `"Assets/Setup/015v2 TileTurn"`

スプライト:
- `Background.png` — 背景
- `TileNormal.png` — 通常タイル（+絵柄グラデーション入り）
- `TileLinked.png` — 連動タイル（★マーク付き）
- `TileLocked.png` — ロックタイル（金色枠）
- `TileFlipped.png` — 反転タイル（赤枠）
- `TileCorrect.png` — 正解時の緑フラッシュ用
- `TileOverlay.png` — プレビュー用オーバーレイ

## StageManager統合

```csharp
void Start() {
    _instructionPanel.OnDismissed += StartGame;
    _stageManager.OnStageChanged += OnStageChanged;
    _stageManager.OnAllStagesCleared += OnAllStagesCleared;
    _instructionPanel.Show("015v2", "TileTurn", ...);
}

void OnStageChanged(int stage) {
    _currentStage = stage;
    _state = GameState.Playing;
    var config = _stageManager.GetCurrentStageConfig();
    _tileManager.SetupStage(config, stage + 1);
    _ui.UpdateStage(stage + 1, 5);
    _ui.HideAllPanels();
}
```

## InstructionPanel内容

- title: "TileTurn"
- description: "タイルをタップして回転させ、1枚の絵を完成させよう"
- controls: "タイルをタップで90度回転"
- goal: "少ない回転数で全タイルを正しい向きにしよう"

## ビジュアルフィードバック設計

1. **正解タイルのポップアニメーション**: 正解向きになった瞬間にスケール 1.0→1.3→1.0（0.2秒）+ 緑色フラッシュ
2. **ゲームオーバー時の赤フラッシュ**: 全タイルがSpriteRenderer.colorを赤(1,0.3,0.3)→白にフェード（0.3秒）
3. **コンボ表示演出**: コンボText要素がスケールアップ表示（0→1.2→1.0、0.15秒）

## スコアシステム

```
baseScore = 1000 * (stage + 1)
remainingRatio = remainingRotations / maxRotations
stageScore = baseScore * (1 + remainingRatio)
if (!previewUsed) stageScore *= 1.5f
combo++
comboMultiplier = combo >= 5 ? 2.0f : combo >= 3 ? 1.5f : combo >= 2 ? 1.2f : 1.0f
stageScore *= comboMultiplier
```

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;   // 5.0
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;   // HUD領域
float bottomMargin = 2.8f;  // ボタン領域
float availableHeight = camSize * 2f - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.8f);
float gridOriginX = -(gridSize - 1) * cellSize * 0.5f;
float gridOriginY = camSize - topMargin - cellSize * 0.5f;
```

## 判断ポイント実装設計

- **連動タイル**: 連動タイルをタップすると `linkedNeighbors` リストの全タイルも同時回転。プレイヤーは事前に連動関係を把握し、操作順序を計画する必要がある
- **プレビューボタン**: `OnPointerDown` → プレビューパネル表示、`OnPointerUp` → 非表示。使用でフラグ立て → スコア乗算なし
- **回転上限チェック**: `_rotationCount >= _maxRotations` でゲームオーバー発動
