# Design: Game018v2_TimeRewind

## namespace

`Game018v2_TimeRewind`

## スクリプト構成

| ファイル | 担当 |
|---------|------|
| `TimeRewindGameManager.cs` | 状態管理・スコア・StageManager/InstructionPanel統合 |
| `BoardManager.cs` | 盤面・コマ・特殊マス・移動履歴・巻き戻し処理 |
| `TimeRewindUI.cs` | HUD・パネル・タイムライン表示 |

## 盤面データ設計

```
enum CellType { Empty, Wall, Goal, Switch, Ice, Bomb, Ghost }
```

- セル状態は `CellType[,]` の2次元配列
- コマ位置: `Vector2Int _playerPos`
- 移動履歴: `List<BoardSnapshot> _history`（各スナップショット: playerPos + wallState + bombTimer）
- 分身（Stage5）: `List<Vector2Int> _ghostTrail`（巻き戻し地点以降の旧軌跡）

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;         // 5.0
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.3f;    // HUD用
float bottomMargin = 2.8f; // Canvas UIボタン用
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 0.9f);
```

## ステージ別パラメータ

| Stage | gridSize | rewindsAllowed | features |
|-------|----------|---------------|---------|
| 1 | 5 | 3 | なし |
| 2 | 6 | 3 | スイッチ |
| 3 | 7 | 3 | 氷床 |
| 4 | 8 | 3 | タイムボム(N=5) |
| 5 | 8 | 2 | 分身 |

→ `StageManager.StageConfig.speedMultiplier` を gridSizeFactor に転用（1.0〜1.6）、`countMultiplier` をrewindsMultiplier（0.9〜0.6）に転用。

## 入力処理フロー（BoardManager）

1. `Mouse.current.leftButton.wasPressedThisFrame` でドラッグ開始位置を記録
2. `leftButton.wasReleasedThisFrame` でリリース位置を取得
3. `delta = end - start`、最大成分の方向を移動方向として決定（±X or ±Y）
4. `delta.magnitude > 30` のしきい値でスワイプ判定
5. 移動先セルを検証（壁判定、氷床のスライド、スイッチ処理等）
6. `_history.Add(snapshot)` で記録 → UI更新

## 巻き戻し処理

- 「⏪」ボタン → `TimelinePanel` 表示（履歴インデックスのミニサムネイル）
- ユーザーが履歴エントリをタップ → `RewindTo(index)` 呼び出し
- `_history.RemoveRange(index, _history.Count - index)` でトリミング
- `rewindsUsed++`、スナップショットを復元

## 特殊マス処理

- **Switch**: 踏んだ瞬間に `_toggleWalls` セットのCellTypeをWall/Emptyでトグル
- **Ice**: 移動方向に壁/障害物に当たるまで連続移動（ループ）
- **Bomb**: 踏んだ手番から `_bombCountdown = N` を設定、毎手減算、0でGameOver
- **Ghost（Stage5）**: 巻き戻し時、`_ghostPos` = 巻き戻し前のコマ位置として残す。両方がGoalに達したらクリア

## InstructionPanel内容

```
gameId: "018v2"
title: "TimeRewind"
description: "行き詰まったら時間を巻き戻して別ルートを試そう"
controls: "スワイプで移動、⏪ボタンで時間を戻す"
goal: "巻き戻し回数を節約しつつゴールに到達しよう"
```

## ビジュアルフィードバック

1. **ゴール到達時**: コマのスケールパルス（1.0→1.4→1.0、0.3秒）+ 黄色フラッシュ
2. **巻き戻し実行時**: 盤面全体が一瞬青白くフラッシュ（`SpriteRenderer.color` を0.5秒で白→元色）
3. **ミス/ゲームオーバー時**: 赤フラッシュ + カメラシェイク（0.2秒）
4. **氷スライド中**: コマのtrailColor変化（白→シアン）

## SceneSetup 構成方針

- `[MenuItem("Assets/Setup/018v2 TimeRewind")]`
- カメラ背景: `new Color(0.06f, 0.04f, 0.12f)` (深夜紺)
- GameManager > StageManager, BoardManager を子として配置
- Canvas (sortOrder=10) に HUD・ボタン・タイムラインパネル・各種パネルを配置
- スプライトは `Assets/Resources/Sprites/Game018v2_TimeRewind/` から読み込み

## SceneSetup 配線フィールド（全量）

**TimeRewindGameManager:**
- `_stageManager` → StageManager
- `_instructionPanel` → InstructionPanel
- `_boardManager` → BoardManager
- `_ui` → TimeRewindUI

**BoardManager:**
- `_playerSprite` → Sprite
- `_goalSprite` → Sprite
- `_wallSprite` → Sprite
- `_floorSprite` → Sprite
- `_switchSprite` → Sprite
- `_iceSprite` → Sprite
- `_bombSprite` → Sprite
- `_ghostSprite` → Sprite

**TimeRewindUI:**
- `_stageText` → TMP_Text
- `_scoreText` → TMP_Text
- `_rewindCountText` → TMP_Text (残り巻き戻し回数)
- `_moveCountText` → TMP_Text
- `_bombCountdownText` → TMP_Text
- `_timelinePanel` → GameObject (巻き戻しUIパネル)
- `_stageClearPanel` → GameObject
- `_clearPanel` → GameObject
- `_gameOverPanel` → GameObject
- `_clearScoreText` → TMP_Text
- `_gameOverText` → TMP_Text
- `_starsText` → TMP_Text

## スコアシステム

```
baseScore = 1000 * stageNumber
noRewindBonus = (rewindsUsed == 0) ? baseScore * 3.0f : 0
rewindRemainingBonus = (rewindsAllowed - rewindsUsed) * 200
shortestPathBonus = (moveCount <= optimalMoves) ? baseScore * 0.5f : 0
comboMultiplier = (combo >= 3) ? 1.5f : (combo >= 2) ? 1.2f : 1.0f
total = (baseScore + noRewindBonus + rewindRemainingBonus + shortestPathBonus) * comboMultiplier
```
