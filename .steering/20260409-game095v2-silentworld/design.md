# Design: Game095v2_SilentWorld

## namespace

`Game095v2_SilentWorld`

## スクリプト構成

| クラス | ファイル | 担当 |
|--------|---------|------|
| SilentWorldGameManager | SilentWorldGameManager.cs | ゲーム状態管理・スコア・StageManager/InstructionPanel統合 |
| WorldManager | WorldManager.cs | グリッドマップ生成・キャラ移動・トラップ判定・視覚ヒント |
| SilentWorldUI | SilentWorldUI.cs | HUD表示・パネル管理 |

## 盤面・ステージデータ設計

### グリッドセル種類
```csharp
enum CellType { Floor, Wall, Trap, Exit, Item }
```

### StageConfig マッピング
- `speedMultiplier`: 観察強度係数（値が高いほど手がかりが弱い→逆数で難易度）
- `countMultiplier`: グリッドサイズ係数（1=5×7, 2=6×8, 3=7×9, 5=8×10）
- `complexityFactor`: 偽ヒント率（0.0〜1.0）、暗黒エリア有無（>0.5で有効）

### 5ステージパラメータ表
| Stage | speedMult | countMult | complexFactor | グリッド | トラップ | ヒント強度 | タイムリミット |
|-------|-----------|-----------|---------------|---------|---------|----------|-------------|
| 1 | 1.0 | 1 | 0.0 | 5×7 | 0 | 強（明確） | なし |
| 2 | 1.0 | 2 | 0.2 | 6×8 | 2 | 中（振動） | なし |
| 3 | 1.2 | 3 | 0.4 | 7×9 | 3 | 中 | なし |
| 4 | 1.5 | 3 | 0.6 | 7×9 | 4 | 弱（長押し） | なし |
| 5 | 2.0 | 4 | 1.0 | 8×10 | 5 | 弱+偽 | 60秒 |

## 入力処理フロー

**WorldManager** が入力を一元管理:
```
Mouse.current.leftButton.wasPressedThisFrame
→ Physics2D.OverlapPoint(worldPos)
→ ヒットしたセルのGridPositionを取得
→ 隣接チェック（マンハッタン距離=1）
→ MoveCharacter(targetCell)
```

長押し観察:
```
Mouse.current.leftButton.isPressed（継続時間計測）
→ 0.5秒以上で長押し判定
→ ObserveArea(touchPos, radius=3) 呼び出し
→ 周囲3マスのヒントを強調表示（コルーチン）
```

## SceneSetup の構成方針

- `Setup095v2_SilentWorld.cs`
- MenuItem: `Assets/Setup/095v2 SilentWorld`
- Camera: backgroundColor = 黒に近い深紺 (0.02, 0.02, 0.08)、orthographicSize=6
- グリッドセルは Prefab ではなく Setup 時に動的生成し Save

## StageManager統合

```csharp
// GameManager.StartGame() 内
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

// OnStageChanged(int stageIndex)
var config = _stageManager.GetCurrentStageConfig();
_worldManager.SetupStage(config, stageIndex);
_ui.UpdateStage(stageIndex + 1, 5);
```

## InstructionPanel内容

```csharp
_instructionPanel.Show(
    "095",
    "SilentWorld",
    "視覚だけを頼りに無音の世界を進もう",
    "タップで移動、長押しで周囲の手がかりを強調表示",
    "音符を集めて出口を開き、すべてのステージをクリアしよう"
);
```

## ビジュアルフィードバック設計

1. **音符取得成功**: 音符オブジェクトをスケールパルス (1.0 → 1.4 → 0.0、0.3秒) + 淡い光の放射エフェクト (SpriteRenderer.color alpha フェードアウト)
2. **トラップ接触**: キャラクターの赤フラッシュ (SpriteRenderer.color → 赤 → 白、0.4秒) + カメラシェイク (Camera.main.transform を 0.2秒間微振動)
3. **観察実行**: 周囲セルのハイライト (color → 青白く明滅、0.8秒コルーチン)
4. **出口解放**: 出口セルのスケールバウンス (1.0 → 1.3 → 1.0、0.5秒) + 色変化 (暗い赤 → 輝く黄色)

## スコアシステム

```
基本スコア: 音符1個 = 50pt × (ステージ番号)
ヒント未使用ボーナス: +100pt
ノーダメージボーナス: +100pt
コンボ乗算:
  - 3連続音符取得: ×1.5
  - 5連続音符取得: ×2.0
  - コンボはトラップ接触でリセット
```

## ステージ別新ルール表

| Stage | 新要素 | 実装詳細 |
|-------|--------|---------|
| 1 | 基本のみ | 明確な青白い光でトラップ位置周囲を事前警告 |
| 2 | 振動床 | トラップセルに乗ると1秒後ダメージ（逃げるチャンスあり）。接近1マスで床が振動アニメ |
| 3 | 音符ゲート | 特定の音符を取ると隠し通路セル(Wall→Floor)が開く。音符ゲートと通路はSetupで配線 |
| 4 | 暗黒エリア | 指定エリア内のセルはhint表示がデフォルトOFF。長押し観察時のみ一時的に手がかりが見える |
| 5 | 偽ヒント+タイマー | 一部トラップに偽の安全ヒント付与。60秒タイムリミット表示、超過でゲームオーバー |

## 判断ポイントの実装設計

### 長押し観察 vs 即移動
- 観察: `_hintUsedThisStage++`でカウント。1ステージ3回まで（4回目以降は観察不可）
- 観察コスト: 0.5秒の時間消費（タイムリミットのあるStage5で特に重要）

### ルート選択
- 音符位置はランダム生成（シード固定可）
- 音符を取るルートとスルーするルートで最終スコアが変わる

### 偽ヒント識別（Stage5）
- 本物のヒント: 青白い光が2回明滅
- 偽のヒント: 同じ光だが3回明滅（complexityFactor=1.0時に適用）
- 識別できるかはプレイヤーのパターン記憶力に依存

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize; // 6.0
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.3f;   // HUD用
float bottomMargin = 2.8f; // Canvas UI用
float availableHeight = camSize * 2f - topMargin - bottomMargin;
// cellSize = availableHeight / gridRows（最大値を min でキャップ）
float cellSize = Mathf.Min(availableHeight / gridRows, camWidth * 2f / gridCols, 1.2f);
// グリッド中心 Y: -(bottomMargin/2) + topMargin/2 = 上寄り
```

## SceneSetup 配線が必要なフィールド

`SilentWorldGameManager` SerializeField:
- `_stageManager`: StageManager
- `_instructionPanel`: InstructionPanel
- `_worldManager`: WorldManager
- `_ui`: SilentWorldUI

`WorldManager` SerializeField:
- `_gameManager`: SilentWorldGameManager
- `_cellSprites`: Sprite[] (floor, wall, trap, exit, item)
- `_characterSprite`: Sprite

`SilentWorldUI` SerializeField:
- `_stageText`, `_scoreText`, `_lifeText`, `_hintText`: TMP_Text
- `_timeText`: TMP_Text（タイムリミット表示、Stage5のみ）
- `_comboText`: TMP_Text
- `_stageClearPanel`: GameObject
- `_gameOverPanel`: GameObject
- `_nextStageButton`: Button
- `_retryButton`: Button
- `_menuButton`: Button
