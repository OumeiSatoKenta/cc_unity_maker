# 設計書: Game086v2_CityBonsai

## namespace

`Game086v2_CityBonsai`

## スクリプト構成

### CityBonsaiGameManager.cs
- ゲーム状態管理（Playing / StageClear / AllClear / GameOver）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] CityBonsaiManager _bonsaiManager`
- `[SerializeField] CityBonsaiUI _ui`
- Start() → InstructionPanel表示 → OnDismissed → StartGame → StageManager.StartFromBeginning()
- OnStageChanged(int) → bonsaiManager.SetupStage()
- OnAllStagesCleared → AllClear表示
- スコア計算・コンボ管理
- GameManager参照取得: SerializeField

### CityBonsaiManager.cs（コアメカニクス）
- 盆栽の枝スロット管理
- 建物配置ロジック
- 剪定ロジック
- ターン進行（満足度変動・季節変化・災害イベント・ライバル都市）
- 入力処理: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint` を一元管理
- `[SerializeField] CityBonsaiGameManager _gameManager` (SerializeField)
- `[SerializeField] CityBonsaiUI _ui`
- `_isActive` ガードで Update() 制御
- `SetupStage(StageManager.StageConfig config, int stageIndex)` でパラメータ適用

### CityBonsaiUI.cs
- ステージ表示「Stage X / 5」
- スコア表示（コンボ乗算込み）
- 人口 / 美しさ / 満足度 表示
- 建物選択ボタン群
- 剪定ボタン / ターン進行ボタン
- ステージクリアパネル（「次のステージへ」ボタン）
- 最終クリアパネル
- ゲームオーバーパネル

## 盆栽・ステージデータ設計

### 枝スロット
- 円形配置: 盆栽の幹を中心に放射状に枝スロットを配置
- 各スロットは `BranchSlot` クラスで管理
  - position (Vector3 - カメラ依存で動的計算)
  - building (BuildingType enum: None, House, Shop, Public, Shrine, Park)
  - hasFlower (bool - 剪定後に花が咲く)
  - isDisabled (bool - 災害で折れた場合)
- 隣接判定: インデックスの差が1 or 最初と最後が隣接（円形）

### 建物パラメータ
| 種類 | 人口 | 満足度 | 美しさ | 解放ステージ |
|------|------|--------|--------|-------------|
| 住宅 | +3 | 0 | -2 | 1 |
| 商業 | 0 | +15 | -3 | 2 |
| 公共 | +1 | +5 | +5 | 3 |
| 神社 | +2 | +10 | +10 | 4 |
| 公園 | 0 | +20 | +8 | 4 |

### ステージ別パラメータ表
| パラメータ | S1 | S2 | S3 | S4 | S5 |
|-----------|----|----|----|----|-----|
| スロット数 | 6 | 8 | 9 | 10 | 12 |
| 人口目標 | 8 | 15 | 22 | 30 | 40 |
| 美しさ目標 | 30 | 40 | 55 | 65 | 80 |
| 建物種類数 | 1 | 2 | 3 | 5 | 5 |
| 季節変化 | × | × | ○ | ○ | ○ |
| 災害 | × | × | × | ○ | ○ |
| ライバル | × | × | × | × | ○ |
| 要望 | × | ○ | ○ | ○ | ○ |

## 入力処理フロー

1. 建物ボタンタップ → `_selectedBuilding` を設定
2. 剪定ボタンタップ → `_isPruningMode` トグル
3. Update() で `Mouse.current.leftButton.wasPressedThisFrame` 検出
4. `Physics2D.OverlapPoint` で枝スロット判定
5. 剪定モード → 枝を剪定 / 配置モード → 建物配置
6. ターンボタン → AdvanceTurn() で満足度・季節・災害処理

## SceneSetup 構成方針

- `Setup086v2_CityBonsai.cs` を `Assets/Editor/SceneSetup/` に作成
- `[MenuItem("Assets/Setup/086v2 CityBonsai")]`
- カメラ: orthographic, size=6, bg=茶系(simulation)
- 背景スプライト: background.png
- 盆栽幹: trunk sprite (中央配置)
- 枝スロット: branch sprite × 12個 (最大数で生成、ステージで有効数を制御)
- GameManager hierarchy → StageManager, BonsaiManager, UI
- InstructionPanel (fullscreen overlay, sortOrder=100)
- Canvas: HUD + 建物ボタン + 剪定/ターンボタン + パネル群

### SceneSetup で配線が必要なフィールド

**CityBonsaiGameManager:**
- `_stageManager` → StageManager
- `_instructionPanel` → InstructionPanel
- `_bonsaiManager` → CityBonsaiManager
- `_ui` → CityBonsaiUI

**CityBonsaiManager:**
- `_gameManager` → CityBonsaiGameManager
- `_ui` → CityBonsaiUI
- `_trunkRenderer` → SpriteRenderer (幹)
- `_slotRenderers` → SpriteRenderer[] (枝スロット)
- `_slotColliders` → BoxCollider2D[] (枝スロット)
- `_buildingSprites` → Sprite[] (住宅, 商業, 公共, 神社, 公園)
- `_flowerSprite` → Sprite (花)
- `_emptySlotSprite` → Sprite (空きスロット)

**CityBonsaiUI:**
- `_stageText` → TMP
- `_scoreText` → TMP
- `_populationText` → TMP
- `_beautyText` → TMP
- `_satisfactionText` → TMP
- `_messageText` → TMP
- `_buildingButtons` → Button[]
- `_buildingButtonTexts` → TMP[]
- `_pruneButton` → Button
- `_turnButton` → Button
- `_stageClearPanel` → GameObject
- `_stageClearScoreText` → TMP
- `_nextStageButton` → Button
- `_allClearPanel` → GameObject
- `_allClearScoreText` → TMP
- `_gameOverPanel` → GameObject

**InstructionPanel:**
- `_titleText`, `_descriptionText`, `_controlsText`, `_goalText` → TMP
- `_startButton` → Button
- `_helpButton` → Button
- `_panelRoot` → GameObject

## StageManager統合

- `OnStageChanged` → `bonsaiManager.SetupStage(config, stageIndex)` でスロット数・建物種類・目標値を再設定
- ステージ切り替え時に盆栽をリセット（建物全撤去、花全除去、新スロット配置）

## InstructionPanel内容

- title: "CityBonsai"
- description: "盆栽の中に小さな都市を育てよう"
- controls: "建物ボタンで選択、枝をタップで配置\n剪定ボタンで枝を整えて美しさUP"
- goal: "人口と美しさを両立して理想の盆栽都市を作ろう"

## ビジュアルフィードバック設計

1. **建物配置成功**: スロットのスケールパルス (1.0 → 1.3 → 1.0, 0.2秒) + 緑フラッシュ
2. **剪定実行**: 切られた枝の赤フラッシュ + 花が咲くアニメーション（スケール 0 → 1.0, 0.3秒）
3. **コンボ時**: 全スロットがゴールドにフラッシュ + スケール波紋
4. **災害発生**: カメラシェイク (0.3秒) + 被害枝の赤フラッシュ
5. **目標達成**: 達成した指標テキストのスケールパルス + 色変化（白→緑）

## スコアシステム

- 基本スコア: 建物配置 +10pt、剪定 +5pt、ターン生存 +3pt
- コンボ: 連続剪定 → 2連続 ×1.3、3連続 ×1.5、4連続以上 ×2.0
- 隣接ボーナス: 同種建物隣接 → 効果 ×1.2
- 両立ボーナス: 人口目標 AND 美しさ目標 同時達成 → スコア ×1.5
- 要望応答ボーナス: +20pt

## ステージ別新ルール表

- Stage 1: 基本ルール（住宅配置 + 剪定）
- Stage 2: 商業施設追加 + 住民要望（ランダムに「商業がほしい」等。応えると満足度+20）
- Stage 3: 公共施設追加 + 季節変化（4ターン1サイクル。春:美+5、夏:美+0、秋:美+3、冬:美-3）
- Stage 4: 神社・公園追加 + 災害イベント（確率でランダム枝破壊）
- Stage 5: ライバル都市（毎ターン目標+1上昇）+ 全建物

## 判断ポイントの実装設計

- **トリガー1**: 空きスロットをタップ時 → 配置する建物種類の選択（報酬: 人口or満足度or美しさ / ペナルティ: 他指標の低下）
- **トリガー2**: 剪定ボタンON + スロットタップ時 → 建物付き枝の剪定判断（報酬: 美しさ+10, コンボ倍率UP / ペナルティ: 住民-3, 満足度-10）
- **トリガー3**: ターン進行ボタン → 即進行か、もう1手配置/剪定してから進行するか

## レスポンシブ配置

- 盆栽中心: (0, camSize * 0.15) → やや上寄り
- 枝スロット: 中心から半径 `camSize * 0.45` の円形配置
- 上部マージン: camSize - 1.2 以上をHUD用
- 下部マージン: 2.8u をUI用
- スロットサイズ: `Mathf.Min(camSize * 0.15, 0.8)` で動的計算
