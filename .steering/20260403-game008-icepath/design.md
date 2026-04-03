# Design: Game008v2_IcePath

## スクリプト構成

### namespace: `Game008v2_IcePath`

| クラス | ファイル | 担当 |
|--------|---------|------|
| IcePathGameManager | IcePathGameManager.cs | ゲーム状態管理・StageManager統合 |
| IceBoardManager | IceBoardManager.cs | 盤面ロジック・プレイヤー移動・セル管理 |
| IcePathUI | IcePathUI.cs | スコア・ステージ・パネル表示 |

## 盤面・ステージデータ設計

### セル種別 (enum CellType)
- `Ice` - 通常氷（通過可能）
- `Wall` - 壁（侵入不可、停止）
- `Rock` - 岩（侵入不可、停止）[Stage2+]
- `Crack` - ひび割れ氷（2回通過で穴）[Stage3+]
- `Hole` - 穴（通過不可）[Stage3+の破壊後]
- `Redirect` - 方向変換タイル（強制転向）[Stage4+]
- `Friction` - 摩擦エリア（2マス制限）[Stage5+]

### GridData (各ステージ固定盤面)
各ステージのレイアウトは `IceBoardManager` に配列定数として定義。
プレイヤー初期位置もステージごとに定義。

### StageConfig活用
- `config.countMultiplier` → ステージ番号 → 該当ステージの盤面を選択

## 入力処理フロー

1. `IceBoardManager.Update()` でスワイプ検出
   - タッチ/マウス: `Mouse.current.leftButton` + デルタ検出
   - スワイプ方向判定（8px以上でトリガー）
2. `TryMove(Vector2Int dir)` で移動計算
   - 壁/岩まで滑走（Friction エリアは最大2マス）
   - 経路上のセルを訪問済みに
   - 状態履歴に追加（Undo用）
3. `CheckWinCondition()` → 全Ice/Crack(未破壊)通過でクリア

## SceneSetup 構成方針

### MenuPath
`Assets/Setup/008v2 IcePath`

### SceneSetup クラス名
`Setup008v2_IcePath`

### ファイルパス
`Assets/Editor/SceneSetup/Setup008v2_IcePath.cs`

## StageManager統合

- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
- `OnStageChanged(int stage)`:
  - `_boardManager.SetupStage(stage)` で盤面再構築
  - UI更新

### ステージ別パラメータ

| Stage(0-based) | 盤面 | 新要素 |
|---------------|------|--------|
| 0 | 5×5 | Ice + Wall のみ |
| 1 | 6×6 | + Rock（岩） |
| 2 | 6×6 | + Crack（ひび割れ） |
| 3 | 7×7 | + Redirect（方向変換タイル） |
| 4 | 7×7 | + Friction（摩擦エリア） |

## InstructionPanel内容

- title: "IcePath"
- description: "氷の上を滑って全マスを通過する一筆書きパズル"
- controls: "スワイプで移動方向を指定（上下左右）"
- goal: "全ての氷マスを1回ずつ通過しよう"

## ビジュアルフィードバック設計

1. **移動完了時（成功）**: プレイヤーのスケールパルス (1.0 → 1.3 → 1.0、0.15秒)
2. **ひび割れ氷の破壊時**: 赤フラッシュ（`SpriteRenderer.color` 白→赤→消滅）+ パーティクル風エフェクト（小さな四角が飛び散る）
3. **行き詰まり（詰み検出）**: カメラシェイク + ゲームオーバーパネル表示
4. **ステージクリア**: 全セルが緑に光るウェーブアニメーション

## スコアシステム

- 基本スコア = 1000 × ステージ倍率（Stage 1〜5: ×1〜×3）
- 実際の手数が少ないほど高得点
- 最小手数でクリア → パーフェクトボーナス +500
- ★3 = 最小移動数でクリア、★2 = +2以内、★1 = クリアのみ

## ステージ別新ルール表

| Stage | 新ルール |
|-------|---------|
| 1 | 基本のみ（壁まで滑る、全マス通過） |
| 2 | 岩ブロック追加（任意の位置で止まれる戦術的停止点） |
| 3 | ひび割れ氷（2回通過で穴、通過順序の管理が必要） |
| 4 | 方向変換タイル（踏むと強制的に進路が変わる） |
| 5 | 摩擦エリア（最大2マス滑走制限ゾーン） |

## 判断ポイントの実装設計

### 滑走経路の逆算
- トリガー条件: 複数方向に移動可能な状況
- 報酬: 最短ルート発見で最小手数ボーナス
- ペナルティ: 行き詰まり → リセット

### ひび割れ氷の通過順
- トリガー条件: ひび割れ氷マスに近づく
- 先に通過: 2回目で穴になり後の計画が変わる
- 後に通過: 1回目で安全に通れるが後の計画を縛る

## レスポンシブ配置設計

```csharp
float camSize = Camera.main.orthographicSize; // 6.0f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD用
float bottomMargin = 2.8f; // UIボタン用
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// gridSize = 5 or 6 or 7
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.2f);
```

## 配線が必要なフィールド (SceneSetup)

### IcePathGameManager
- `_stageManager` (StageManager)
- `_instructionPanel` (InstructionPanel)
- `_boardManager` (IceBoardManager)
- `_ui` (IcePathUI)

### IceBoardManager
- `_gameManager` (IcePathGameManager) → GetComponentInParent
- `_playerSprite` (Sprite)
- `_iceSprite` (Sprite)
- `_rockSprite` (Sprite)
- `_crackSprite` (Sprite)
- `_holeSprite` (Sprite)
- `_redirectSprite` (Sprite)
- `_frictionSprite` (Sprite)
- `_visitedSprite` (Sprite)

### IcePathUI
- `_gameManager` (IcePathGameManager) → GetComponentInParent
- `_scoreText`, `_stageText`, `_moveCountText`, `_remainingText`
- `_stageClearPanel`, `_clearPanel`, `_gameOverPanel`
- `_nextStageButton`
