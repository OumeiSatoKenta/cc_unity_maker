# Design: Game034v2 DropZone

## スクリプト構成

| クラス | ファイル | 役割 |
|--------|---------|------|
| DropZoneGameManager | DropZoneGameManager.cs | 状態管理・StageManager統合・スコア管理 |
| DropZoneMechanic | DropZoneMechanic.cs | 落下アイテム生成・ドラッグ処理・ゾーン判定 |
| FallingItem | FallingItem.cs | 各アイテムの落下・ドラッグ追従・コライダー |
| DropZoneUI | DropZoneUI.cs | スコア・ミス・コンボ・ステージ表示 |

namespace: `Game034v2_DropZone`

## 盤面・ステージデータ設計

### StageConfig マッピング
- speedMultiplier: 落下速度倍率 (基準速度 × speedMultiplier)
- countMultiplier: アイテム数 (基準数 × countMultiplier)
- complexityFactor: 紛らわしさ度 (0.0〜1.0)
- stageName: "Stage X"

### Stage パラメータ表
| Stage | speedMultiplier | countMultiplier | complexityFactor | zoneCount | dualDrop |
|-------|----------------|----------------|-----------------|-----------|----------|
| 1 | 1.0 | 1 | 0.0 | 2 | false |
| 2 | 1.5 | 1 | 0.0 | 3 | false |
| 3 | 2.0 | 2 | 0.5 | 3 | false |
| 4 | 2.5 | 2 | 0.3 | 4 | false |
| 5 | 3.5 | 2 | 0.3 | 4 | true |

### アイテム種類
- ItemType.Fruit (🍎フルーツ系): Zone 0 (FruitBox)
- ItemType.Trash (🗑ゴミ系): Zone 1 (TrashBox)
- ItemType.Recycle (♻️リサイクル): Zone 2 (RecycleBox) - Stage2以降
- ItemType.Special (⭐ボーナス): Zone 3 (BonusBox) - Stage4以降

### 紛らわしいアイテム (Stage3, complexityFactor > 0)
- 色やシルエットが似ているが分類が異なる組合せ
- complexityFactor が高いほど紛らわしいアイテムの出現率UP

## 入力処理フロー

```
DropZoneMechanic.Update()
  → Pointer Down: Camera.ScreenToWorldPoint → Physics2D.OverlapPoint → FallingItem検出
  → Pointer Drag: 検出したFallingItemを _dragItem としてワールド座標追従
  → Pointer Up: _dragItem の位置から最近傍ゾーンに判定 → OnDrop(zone)
```

新Input System使用:
- `Mouse.current.leftButton.wasPressedThisFrame` / `isPressed` / `wasReleasedThisFrame`
- `Mouse.current.position.ReadValue()`
- using: `using UnityEngine.InputSystem;`

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize; // 5.0
float camW = camSize * Camera.main.aspect;
float topMargin = 1.2f;    // HUD用
float bottomMargin = 3.0f; // Canvas UI用
float gameAreaTop = camSize - topMargin;       // アイテム生成Y
float gameAreaBottom = -camSize + bottomMargin; // ゾーン配置Y

// ゾーン配置（横に均等分割）
for (int i = 0; i < zoneCount; i++)
    zoneX = -camW + (camW * 2f / zoneCount) * (i + 0.5f);
```

## SceneSetup 構成方針

`Setup034v2_DropZone.cs` in `Assets/Editor/SceneSetup/`
- MenuItem: `"Assets/Setup/034v2 DropZone"`
- 背景・カメラ設定
- ゾーンオブジェクトは SceneSetup で生成（動的ではなく固定として配置）
  → SetupStage() でゾーン表示/非表示を切替
- InstructionPanel 生成・配線
- StageClearPanel・FinalClearPanel・GameOverPanel 生成

## StageManager 統合

```csharp
// GameManager.Start()
_instructionPanel.Show("034v2", "DropZone", description, controls, goal);
_instructionPanel.OnDismissed += StartGame;

// StartGame()
_stageManager.SetConfigs(configs);
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

// OnStageChanged(int stageIndex)
var config = _stageManager.GetCurrentStageConfig();
_mechanic.SetupStage(config, stageIndex);
_ui.UpdateStage(stageIndex + 1, totalStages);
_ui.HideStageClear();
```

## InstructionPanel 内容
- title: "DropZone"
- description: "落ちてくるアイテムを正しい箱に仕分けしよう"
- controls: "アイテムをドラッグして正しいゾーンにドロップ"
- goal: "ミス3回以内で全アイテムを仕分けしよう"

## ビジュアルフィードバック設計

1. **正解ドロップ時 (正解アニメ)**:
   - アイテムが対応ゾーンへ縮小しながら消える（Scale: 1.0 → 0, 0.3秒）
   - ゾーンがポップ（Scale: 1.0 → 1.2 → 1.0, 0.2秒）

2. **ミス時 (赤フラッシュ)**:
   - アイテムの SpriteRenderer.color を赤にフラッシュ (0.2秒)
   - カメラシェイク (intensity=0.2, duration=0.3秒)
   - ミスカウンター表示更新

## スコアシステム
- 基本スコア: 正解1個=10pt
- コンボ: 連続正解でコンボ倍率アップ (1→2→3→4→5)
- ミスでコンボリセット
- ステージクリアボーナス: 残りライフ×100pt
- コンボ時実スコア: 10 × comboMultiplier

## ステージ別新ルール表
- Stage 1: フルーツとゴミの2分類。2ゾーン
- Stage 2: リサイクル品（緑色系）が追加。3ゾーン
- Stage 3: 見た目が似た紛らわしいアイテム登場（例: バナナとゴムほか）
- Stage 4: 4ゾーン+ボーナス箱登場。ボーナスアイテムを正しく入れるとライフ+1
- Stage 5: 2個同時落下でマルチタスク要求

## 判断ポイント実装設計
- ドラッグ開始: アイテムの形/色から瞬時にカテゴリ判断
- ボーナス判断: Stage4以降、ゴールド輝き付きアイテムをボーナスゾーンへ
- 2個同時: Stage5、落下速度が遅い方を先に処理するか早い方を処理するか

## Buggy Code 防止
- タグ比較は `gameObject.CompareTag()` ではなくアイテム種別はenumで管理
- `_isActive` ガードで非プレイ時の入力を無効化
- `OnDestroy()` で生成した Texture2D・Sprite をクリーンアップ
- 動的生成アイテムは `_spawnedItems` リストで追跡・クリーンアップ
