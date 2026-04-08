# Design: Game093v2_ColorPerception

## namespace
`Game093v2_ColorPerception`

## スクリプト構成

| クラス | ファイル | 役割 |
|--------|---------|------|
| `ColorPerceptionGameManager` | `ColorPerceptionGameManager.cs` | ゲーム状態管理、StageManager/InstructionPanel統合、スコア管理 |
| `ColorPuzzleManager` | `ColorPuzzleManager.cs` | パズルロジック、視点管理、タイル描画、入力処理 |
| `ColorPerceptionUI` | `ColorPerceptionUI.cs` | HUD、ステージクリア/ゲームオーバーパネル表示 |

## タイルシステム設計

```csharp
enum TileState { Wall, Path, Goal, Start, ColorZone }
```

各ステージのマップは「基底マップ」＋「視点マスク」で構成:
- 基底マップ: 全タイルの論理的な情報（Start/Goal/ColorZone位置）
- 視点マスク: 各視点（0/1/2）ごとに「Wall/Path」が異なる

実装方式: `TileState[,][] viewMaps` — `viewMaps[row, col][viewIndex]` = そのタイルが視点viewIndexで見えるか（Wall or Path）

## 視点システム
```csharp
int _currentView; // 0=通常, 1=色覚A, 2=色覚B
```
- 視点切替ボタンで `_currentView` を変更（手数消費なし）
- 壁/通路判定: `_viewMaps[r, c, _currentView] == WallMask`
- タイル色: 視点ごとに異なるカラーパレットで表示

## 色変化ゾーン（Stage4〜5）
`ColorZone`タイルを踏むと、周囲2マス以内の壁/通路マスクをトグル
→ 新しいルートが開通したり既存のルートが塞がったりするギミック

## 周期変化（Stage5）
- `_turnCounter` が `_changePeriod(3)` に達するたびに全マスの視点マスクをローテーション
- UI に残りターン数カウントダウン表示

## 入力処理
- 移動: 画面内に上下左右の4ボタン（Canvas UI）
- 視点切替: 画面下部に視点数分のボタン
- 全入力をColorPuzzleManagerに一元管理

## GameManager参照取得
- `[SerializeField]` で SceneSetup 時に配線

## Start()フロー
```csharp
void Start() {
    _instructionPanel.Show("093", "ColorPerception",
        "視点を切り替えて隠れた道を見つけよう",
        "ボタンで視点切替、上下左右で移動",
        "色の見え方を変えてゴールまで辿り着こう");
    _instructionPanel.OnDismissed += StartGame;
}
```

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

void OnStageChanged(int stageIndex) {
    var config = _stageManager.GetCurrentStageConfig();
    _puzzleManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

## ステージ別パラメータ表

| Stage | gridSize | viewCount | movesLimit | hasColorZone | hasCycleChange | changePeriod |
|-------|---------|-----------|-----------|--------------|----------------|--------------|
| 1     | 5       | 2         | 0(なし)   | false        | false          | -            |
| 2     | 5       | 2         | 20        | false        | false          | -            |
| 3     | 6       | 3         | 15        | false        | false          | -            |
| 4     | 6       | 3         | 12        | true         | false          | -            |
| 5     | 7       | 3         | 10        | true         | true           | 3            |

## InstructionPanel内容
- title: "ColorPerception"
- description: "視点を切り替えて隠れた道を見つけよう"
- controls: "ボタンで視点切替、上下左右で移動"
- goal: "色の見え方を変えてゴールまで辿り着こう"

## ビジュアルフィードバック設計
1. **ゴール到達時**: プレイヤーのスケールパルス（1.0→1.4→1.0, 0.3秒）+ 黄色フラッシュ
2. **壁に当たる時（移動失敗）**: プレイヤーが赤くフラッシュ（0.15秒）+ 小バウンス
3. **視点切替時**: タイル全体が0.2秒でフェードイン（新しい見え方に切替演出）
4. **色変化ゾーン発動**: 対象タイルが緑→白に0.3秒フラッシュ（ゾーン変化を視覚的に示す）
5. **手数超過/ゲームオーバー**: カメラシェイク + プレイヤー赤フラッシュ

## スコアシステム
```
baseScore = 100 × (stageIndex + 1)
movesBonus = Mathf.Max(0, movesLimit - movesUsed) × 15   // 手数制限なし時は0
viewSwitchBonus = (viewSwitchCount <= 3) ? 50 : 0        // 最小視点切替ボーナス
combo++（連続ステージクリア）
multiplier = combo >= 3 ? 1.5f : combo >= 2 ? 1.2f : 1.0f
earned = (baseScore + movesBonus + viewSwitchBonus) × multiplier
```

## SceneSetup構成方針
- `Assets/Setup/093v2 ColorPerception` メニュー
- カメラ背景: ダーク紫 `(0.05, 0.0, 0.12)`
- カテゴリ unique → ネオン緑 `#76FF03` / ネオン紫 `#D500F9` パレット
- 視点切替ボタン: 横並び3個、画面下部 Y=130〜180
- 移動ボタン: 十字配置、画面下部 Y=65〜130
- StageManager（GameManagerの子）、InstructionPanel（別Canvas sortOrder=100）
- StageClearPanel / AllClearPanel / GameOverPanel

## 配線が必要なフィールド（SetField対象）
### GameManager
- `_stageManager`, `_instructionPanel`, `_puzzleManager`, `_ui`

### PuzzleManager
- `_gameManager`, `_ui`
- `_sprPlayer`, `_sprGoal`, `_sprStart`（視点ごとのタイル色はコードで描画）

### UI
- `_stageText`, `_scoreText`, `_movesText`, `_viewText`, `_comboText`
- `_stageClearPanel`, `_stageClearScoreText`
- `_gameOverPanel`, `_gameOverScoreText`
- `_allClearPanel`, `_allClearScoreText`

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;  // 6f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;
float bottomMargin = 3.5f;  // 視点ボタン+移動ボタン分
float availableHeight = camSize * 2f - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 0.9f);
float zoneCenter = camSize - topMargin - availableHeight / 2f;
```

## 判断ポイントの実装設計
- **トリガー条件**: プレイヤーが移動しようとしたとき、前方に壁がある場合
  - 壁の場合: 他の視点では通路かチェックするヒント演出（タイルが微点滅）
- **選択の報酬/ペナルティ**:
  - 視点切替（手数消費なし）して確認: 手数温存だが視点が変わる
  - そのまま別方向に進む: 手数消費するが確実
  - 視点切替せず壁に突進: 移動失敗、手数消費なし（境界チェックで弾く）

## Buggy Code防止チェックリスト
- `[SerializeField]` 未配線の null チェック（Awake で Assert）
- ステージ切替時に前ステージのゲームオブジェクトを全 Destroy
- `_isActive` ガードで入力二重受付を防ぐ
- 動的生成 Texture2D は `OnDestroy()` で `Destroy()` する
- `SetupStage()` のたびに `_viewMaps`, `_playerPos` を再計算
