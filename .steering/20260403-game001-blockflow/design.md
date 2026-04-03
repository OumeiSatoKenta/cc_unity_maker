# 設計書: Game001_BlockFlow (v2リメイク)

## namespace: `Game001_BlockFlow`

## スクリプト構成

| クラス | 責務 |
|--------|------|
| `BlockFlowGameManager` | ゲーム状態管理、StageManager/InstructionPanel統合、スコア管理 |
| `BoardManager` | 盤面生成・ブロック管理・スワイプ入力・クリア判定・5ステージ対応 |
| `BlockFlowUI` | HUD・パネル表示 |

### BlockFlowGameManager
- `[SerializeField] BoardManager _boardManager`
- `[SerializeField] BlockFlowUI _ui`
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`

**GameManager参照取得**: BoardManager は `[SerializeField]` で GameManager を参照

**Start()の流れ:**
1. `_instructionPanel.Show("001", "ブロックフロー", ...)` 
2. `_instructionPanel.OnDismissed += StartGame`
3. `StartGame()` → `_stageManager.StartFromBeginning()`

**StageManager統合:**
- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
- `OnStageChanged(int stage)` → `_boardManager.SetupStage(stage)` で盤面再構築

**状態遷移:**
- Playing → StageClear（全色隣接） → Playing（次ステージ）or Clear（Stage 5クリア）
- Playing → GameOver（手数切れ、Stage 4以降）

### BoardManager
- `[SerializeField] BlockFlowGameManager _gameManager`
- `[SerializeField] Sprite _blockSprite, _fixedBlockSprite, _warpTileSprite, _iceBlockSprite, _boardBgSprite`
- 盤面サイズ・色数・手数制限をステージごとに変更
- スワイプ入力: `Mouse.current.leftButton` + ドラッグ方向判定
- ブロック移動: 壁 or 他ブロックに当たるまで1マスずつ移動（アニメーション付き）
- クリア判定: 各色のブロックが全て隣接（BFS/FloodFill）
- `_isActive` ガードで状態管理

**盤面データ構造:**
```
int[,] _grid; // 0=空, 1〜4=色ブロック, -1=固定, -2=ワープ, -3=氷
Vector2Int _selectedBlock;
```

**入力処理:**
- マウスダウン → ブロック選択（Physics2D.OverlapPoint）
- マウスアップ → スワイプ方向計算（deltaが大きい軸）
- ブロック移動アニメーション（Coroutine、0.15秒/マス）

**5ステージ対応:**
```csharp
public void SetupStage(int stageIndex) {
    ClearBoard();
    switch(stageIndex) {
        case 0: InitBoard(3, 2, 0, false, false, false); break;  // 3×3, 2色
        case 1: InitBoard(4, 3, 0, true, false, false); break;   // 4×4, 3色, 固定ブロック
        case 2: InitBoard(5, 3, 0, true, true, false); break;    // 5×5, 3色, ワープ
        case 3: InitBoard(5, 4, 15, true, true, false); break;   // 5×5, 4色, 手数制限
        case 4: InitBoard(6, 4, 20, true, true, true); break;    // 6×6, 4色, 氷ブロック
    }
}
```

**盤面生成アルゴリズム:**
1. クリア状態（同色隣接）の盤面をまず作る
2. ランダムなスワイプ操作を逆再生して初期盤面を作る（解ける保証）
3. 固定ブロック・ワープ・氷を配置

### BlockFlowUI
- `[SerializeField]` で各テキスト・パネルを受け取る
- ステージ表示「Stage X / 5」
- スコア表示（コンボ乗算込み）
- 残り手数表示（Stage 4以降のみ表示）
- ステージクリアパネル（スコア表示 + 「次のステージへ」）
- 最終クリアパネル（全5ステージ完了）
- ゲームオーバーパネル

## InstructionPanel内容

| 項目 | 値 |
|------|-----|
| title | ブロックフロー |
| description | 同じ色のブロックを隣り合わせにするパズル |
| controls | スワイプでブロックを移動。壁や障害物に当たるまで滑ります |
| goal | 全ての同色ブロックを隣接させよう |

## ビジュアルフィードバック設計

1. **ブロック移動成功**: 移動先でスケールパルス（1.0→1.2→1.0、0.2秒）
2. **色グループ完成**: 該当色のブロック全体が色フラッシュ（白→元色、0.3秒）+ パルス
3. **手数切れ警告**: 残り3手以下で手数テキストが赤フラッシュ

## スコアシステム

| 項目 | 点数 |
|------|------|
| ステージクリア | 1000pt |
| 残り手数ボーナス | 残り手数 × 200pt |
| 色数ボーナス | 色数 × 500pt |
| ノーリセットボーナス | ×1.5倍 |

## ステージ別新ルール表

| Stage | 基本 | 新要素 |
|-------|------|--------|
| 1 | 3×3, 2色, 手数無制限 | 基本操作のみ |
| 2 | 4×4, 3色 | 固定ブロック2個（壁として利用可能） |
| 3 | 5×5, 3色 | ワープタイル2組（端→反対側） |
| 4 | 5×5, 4色, 15手制限 | 手数制限＋固定ブロック3個 |
| 5 | 6×6, 4色, 20手制限 | 氷ブロック4個（2回接触で壊れて通路に） |

## 判断ポイントの実装設計

| トリガー | 選択肢 | 報酬/ペナルティ |
|---------|--------|---------------|
| 複数色のブロックが移動可能 | どの色を先にまとめるか | 良い順序→少ない手数、悪い順序→手数浪費 |
| 固定ブロックの近くにいる | 固定を壁として使うか回避か | 活用→1手節約、回避→安全だが手数+1 |
| 手数残り少ない | 確実な2手ルートか1手リスクルートか | 成功→手数温存+スコアUP、失敗→手数浪費 |

## SceneSetup構成方針

Setup101_ChainReactorのパターンに準拠:
- カメラ: orthographic, size=5.5, backgroundColor=淡青
- Canvas: ScreenSpaceOverlay, 1080×1920
- GameManager配下にStageManager, BoardManager, BlockFlowUI, InstructionPanelController
- 全フィールドをSerializedObjectで配線
- ボタンイベント: NextStage, Restart, Reset をUnityEventTools.AddPersistentListenerで登録
- カラーパレット: puzzle（青 #2196F3 / ティール #00BCD4 / 淡青 #E3F2FD）

## SceneSetupで配線が必要なフィールド一覧

**BlockFlowGameManager:**
- `_boardManager` → BoardManager
- `_ui` → BlockFlowUI
- `_stageManager` → StageManager
- `_instructionPanel` → InstructionPanel

**BoardManager:**
- `_gameManager` → BlockFlowGameManager
- `_blockSprite` → block.png
- `_fixedBlockSprite` → fixed_block.png
- `_warpTileSprite` → warp_tile.png
- `_iceBlockSprite` → ice_block.png
- `_boardBgSprite` → board_bg.png

**BlockFlowUI:**
- `_scoreText`, `_stageText`, `_movesText`
- `_stageClearPanel`, `_stageClearText`, `_nextStageButton`
- `_clearPanel`, `_clearText`
- `_gameOverPanel`, `_gameOverText`

**InstructionPanel:**
- `_panelRoot`, `_titleText`, `_descriptionText`, `_controlsText`, `_goalText`
- `_startButton`, `_helpButton`
