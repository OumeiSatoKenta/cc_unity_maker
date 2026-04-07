# Design: Game084v2 GardenZen

## Namespace
`Game084v2_GardenZen`

## スクリプト構成

### GardenZenGameManager.cs
- ゲーム状態管理（Playing / StageClear / AllClear / GameOver）
- StageManager・InstructionPanel の統合
- UI更新の中継
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] GardenManager _gardenManager`
- `[SerializeField] GardenZenUI _ui`
- Start(): InstructionPanel.Show() → StartGame()
- OnStageChanged(): GardenManager.SetupStage(config, stageIndex)
- OnAllStagesCleared(): 全クリア処理
- OnStageClear(): ステージクリアパネル表示
- NextStage(): 次ステージへ

### GardenManager.cs
- 庭グリッドの管理（配置・砂紋）
- 依頼デザインとの一致度計算
- コアメカニクス実装
- `[SerializeField] GardenZenGameManager _gameManager`
- `[SerializeField] GardenZenUI _ui`

**グリッドデータ設計:**
```
enum CellType { Empty, Stone, Plant, Decoration }
int[,] _targetGrid   // 依頼デザイン
int[,] _currentGrid  // 現在の配置
```

**入力処理:**
- Mouse.current.leftButton.wasPressedThisFrame → クリック位置判定
- Physics2D.OverlapPoint でグリッドセル検出
- using UnityEngine.InputSystem;

**配置ロジック:**
- 選択中のオブジェクト種類をタップで配置/削除
- 砂紋はドラッグで連続描画（ドラッグ軌跡に沿って波紋スプライトを置く）

**スコア計算:**
- 一致度 = 一致セル数 / 総配置セル数 × 100
- 基本スコア = 一致度 × 10（最大1000点）
- 砂紋ボーナス = 砂紋セル数 × 5（最大250点）
- 独自アレンジボーナス = 余分な素材配置 × 2（最大100点）
- パーフェクトボーナス = 一致度100%で+500点

**レスポンシブ配置:**
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;   // HUD用
float bottomMargin = 3.0f; // UIボタン用
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
float maxCellSize = 1.2f;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, maxCellSize);
```

### GardenZenUI.cs
- スコア・一致度・ステージ表示
- パレット（石・植物・装飾選択）ボタン
- 提出・リセットボタン
- ステージクリアパネル
- 全クリアパネル

## StageManager統合
- OnStageChanged(int stageIndex) でグリッドサイズ・配置物を再構築
- ステージ別パラメータ（StageConfig）:
  | Stage | speedMultiplier | countMultiplier | complexityFactor |
  |-------|-----------------|-----------------|-----------------|
  | 1     | 1.0             | 1               | 0.0             |
  | 2     | 1.0             | 2               | 0.25            |
  | 3     | 1.0             | 3               | 0.5             |
  | 4     | 1.0             | 4               | 0.75            |
  | 5     | 1.0             | 5               | 1.0             |

- complexityFactor で gridSize を決定:
  - 0.0 → 4x4, 0.25 → 4x4, 0.5 → 5x5, 0.75 → 5x5, 1.0 → 6x6

## InstructionPanel内容
- title: "GardenZen"
- description: "禅の庭をデザインして心を整えよう"
- controls: "石・植物を選んでグリッドに配置\n砂をなぞって砂紋を描こう"
- goal: "依頼通りの庭をデザインして★3評価を獲得しよう"

## ビジュアルフィードバック設計
1. **配置成功時のポップアニメーション**: 配置したオブジェクトが 1.0→1.3→1.0 にスケールパルス（0.2秒）
2. **提出時の一致度フラッシュ**: 一致セルが緑フラッシュ（Color lerp → 0.3秒）、不一致セルが赤フラッシュ
3. **砂紋描画時のウェーブエフェクト**: 砂紋スプライトがフェードイン（alpha 0→1、0.15秒）
4. **スコア乗算時のコンボ表示**: テキストがスケールアップ（1.0→1.5→1.0）+ 色変化

## スコアシステム
- 基本スコア: 一致度 × 10
- 砂紋ボーナス: 砂紋数 × 5
- 独自アレンジ: アレンジ数 × 2
- パーフェクト: 一致度100%で+500
- コンボ倍率: 連続★3で×1.5

## ステージ別新ルール表
- Stage1: 石のみ4x4、砂紋不要（チュートリアル）
- Stage2: 植物追加・左右対称制約、対称ラインUI表示
- Stage3: 装飾物追加・季節カラーテーマ（秋：茶・橙）
- Stage4: 高低差（前後レイヤー：大石を奥、植物を手前）
- Stage5: 全素材解放・自由創作（独自アレンジボーナス2倍）

## SceneSetup方針
- MenuItem: "Assets/Setup/084v2 GardenZen"
- グリッドセルをSpriteRenderer付きGameObjectとして生成
- カテゴリカラー: simulation = 茶(#795548) / 琥珀(#FF8F00)
- 背景: 砂の庭っぽい淡い茶色グラデーション
- スプライト一覧:
  - background.png（庭の砂地）
  - stone_1.png / stone_2.png / stone_3.png（石）
  - plant_1.png / plant_2.png（植物）
  - decoration_1.png（灯籠）
  - sand_pattern.png（砂紋）
  - grid_cell.png（グリッドセル背景）
  - grid_target.png（目標マーカー）
