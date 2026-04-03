# Design: Game010v2_GearSync

## namespace
`Game010v2_GearSync`

## スクリプト構成

### GearSyncGameManager.cs
- StageManager・InstructionPanel統合
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear）
- スコア・コンボ管理
- Start()でInstructionPanel.Show()→OnDismissed→StartGame()
- StartGame()でStageManager.StartFromBeginning()
- OnStageChanged(int stage)でGearSyncManagerにSetupStage()を呼ぶ
- OnAllStagesCleared()で全クリアパネル表示
- GearSyncManager.OnTestResult(bool success, int testCount)でスコア計算

### GearSyncManager.cs
- グリッド管理（GridCell配列）
- 歯車データ: GearCell { GearType, GearSize, IsFixed, IsGoal, RequiredDir }
- 入力処理: マウスクリックでパーツ選択→グリッド配置/回収
- 隣接判定: 上下左右でGearTypeが噛み合うか確認
- 伝達シミュレーション: BFSで動力源から全歯車へ回転方向を伝播
- ベルト接続（Stage5）: 指定された2セル間を同方向で繋ぐ
- SetupStage(StageConfig config)でステージ構成を切り替え

### GearSyncUI.cs
- スコア・ステージ・テスト回数テキスト更新
- ステージクリアパネル制御
- 全クリアパネル制御
- パーツリストボタン管理
- テスト結果演出（成功/失敗）

## 盤面設計

```csharp
// グリッドセル
enum GearType { None, SmallGear, LargeGear, Belt }
enum RotDir { None, CW, CCW }

class GearCell {
    GearType type;
    GearSize size; // 1x1 or 2x2
    bool isFixed;  // ステージ4: 動かせない
    bool isGoal;   // ゴール歯車
    RotDir requiredDir; // ゴール歯車の必要回転方向
    RotDir currentDir;  // シミュレーション結果
    bool isPowerSource; // 動力源（Stage1は左端固定）
}
```

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD用
float bottomMargin = 2.8f; // UIボタン用
float availableHeight = camSize * 2f - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, (camWidth * 2f - 1.0f) / gridSize, 1.2f);
// グリッド中央配置
Vector2 gridOrigin = new Vector2(-cellSize * gridSize / 2f, camSize - topMargin - cellSize * 0.5f);
```

## 入力処理フロー
1. Mouse.current.leftButton.wasPressedThisFrame でクリック検出
2. Camera.main.ScreenToWorldPoint でワールド座標変換
3. グリッド座標に変換（(worldPos - gridOrigin) / cellSize）
4. 選択中パーツあり → 空きセルなら配置、配置済みセルなら回収
5. 選択中パーツなし → 配置済みセルをタップで回収

## 伝達シミュレーション（BFS）
```
1. 動力源セルをキューに追加（回転方向=CW）
2. キューから取り出し、隣接セルをチェック
3. 直接噛み合い → 逆方向を伝播
4. ベルト接続 → 同方向を伝播
5. 全セル処理後、全ゴールセルの currentDir == requiredDir を確認
```

## SceneSetup構成方針

`Setup010v2_GearSync.cs`:
- MenuItem: `"Assets/Setup/010v2 GearSync"`
- Camera（orthographicSize=5）+ Background
- Canvas (ScreenSpaceOverlay)
  - HUD: StageText, ScoreText, TestCountText, ComboText
  - パーツリスト（下部パネル）: SmallGearButton, LargeGearButton, BeltButton
  - TestButton (「起動テスト」)
  - MenuButton (「メニューへ戻る」)
  - StageClearPanel (「ステージクリア！」+ NextStageButton)
  - GameClearPanel (全クリア)
  - InstructionPanel (フルスクリーン)
- GameManager (GearSyncGameManager + StageManager)
- GearSyncManager
- GearSyncUI

## StageManager統合
- `_stageManager.OnStageChanged += OnStageChanged`
- OnStageChanged(int stage) → GearSyncManager.SetupStage(GetCurrentStageConfig())
- StageConfig.speedMultiplier → 未使用（歯車はアニメーション速度に使用）
- StageConfig.countMultiplier → グリッドサイズ・パーツ数に対応

## InstructionPanel内容
```csharp
_instructionPanel.Show(
    "010",
    "GearSync",
    "歯車を配置して全てを連動させる機械パズル",
    "パーツをタップして選択、グリッドをタップして配置。配置済みをタップで回収",
    "全ての歯車を噛み合わせて機械を起動しよう"
);
```

## ビジュアルフィードバック設計
1. **歯車回転アニメーション**: テスト実行時に全歯車がspeedに応じてrotationをアニメーション
   - `transform.Rotate(0, 0, rotSpeed * Time.deltaTime)`
   - CW: -1、CCW: +1
2. **成功フラッシュ**: ゴール歯車が成功時に緑色フラッシュ（Color → green → white、0.5秒）
3. **失敗フラッシュ**: 失敗した歯車が赤色フラッシュ + カメラシェイク
4. **配置パルス**: 歯車配置時にスケールパルス（1.0 → 1.2 → 1.0、0.15秒）

## スコアシステム
```
基本スコア = 1000 × stageMultiplier
テストボーナス = Mathf.Max(0, 5 - testCount) × 100
マスターボーナス = testCount == 1 ? 3.0f : 1.0f
コンボ乗算 = 1.0 + (combo - 1) × 0.2
totalScore += (基本スコア + テストボーナス) × マスターボーナス × コンボ乗算
```

## ステージ別新ルール表
| Stage | グリッド | 固定済み | 追加可能 | 新ルール |
|-------|---------|---------|---------|---------|
| 1 | 4x4 | 動力源(0,1)+ゴール(3,1) | 小×1 | 基本噛み合わせ |
| 2 | 4x4 | 動力源(0,0)+中継(1,2)+ゴール(3,2) | 小×2 | L字チェーン |
| 3 | 5x5 | 動力源(0,2)+大(2,1-2)+ゴール(4,2) | 小×1+大×1 | 大小速度比 |
| 4 | 5x5 | 動力源(0,0)+固定×2+方向指定ゴール | 小×2+大×1 | 固定歯車+方向指定 |
| 5 | 6x6 | 動力源(0,2)+固定×2+ゴール×2 | 小×2+大×1+ベルト×1 | ベルト同方向伝達 |

## 判断ポイントの実装設計
- ゴール歯車に RotDir indicator（矢印スプライト）を表示
- テスト失敗時、失敗した歯車を赤ハイライトして「どこが違うか」を示す
- テスト回数カウンターをリアルタイム表示（少ないほど良い評価を視覚的に示す）
