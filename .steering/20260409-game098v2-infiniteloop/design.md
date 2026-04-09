# Design: Game098v2 InfiniteLoop

## namespace
`Game098v2_InfiniteLoop`

## スクリプト構成

### InfiniteLoopGameManager.cs
- 責務: ゲーム状態管理（Idle / Playing / StageClear / GameOver）
- StageManager・InstructionPanel統合
- スコア・コンボ管理
- LoopManagerから結果を受け取りUI更新

### LoopManager.cs
- 責務: ループ制御・変化要素管理・脱出条件判定
- コアメカニクス全体を担当
- ステージ設定（変化要素数・偽変化数・逆行ループ有無）を受け取る
- 部屋内のオブジェクトをタップしたとき調査イベント発火
- 法則発見フラグ管理・脱出試行判定
- 入力処理: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint`

### InfiniteLoopUI.cs
- 責務: UI表示管理
- ステージ・スコア・ループ残数・メモパネル表示
- クリア/ゲームオーバーパネル表示
- ボタンイベント登録

## 盤面・ステージデータ設計

### StageConfig 利用
- `speedMultiplier`: ループ制限の係数（1.0=制限10, 2.0=制限5相当）
- `countMultiplier`: 変化要素数（2=2個, 3=3個）
- `complexityFactor`: 偽変化率（0.0=なし, 0.5=1個, 1.0=2個）

### ステージ別パラメータ
| Stage | speedMultiplier | countMultiplier | complexityFactor | 特殊 |
|-------|----------------|----------------|-----------------|-----|
| 1 | 1.0 | 1 | 0.0 | なし |
| 2 | 1.0 | 2 | 0.0 | なし |
| 3 | 1.2 | 2 | 0.0 | ランダム出現順 |
| 4 | 1.4 | 3 | 0.5 | 偽変化1個 |
| 5 | 2.0 | 3 | 1.0 | 偽変化2個+逆行 |

### ループ制限計算
```csharp
int baseLimit = 10;
int loopLimit = Mathf.RoundToInt(baseLimit / config.speedMultiplier);
// Stage1=10, Stage2=10, Stage3=8, Stage4=7, Stage5=5
```

## 変化要素システム設計

### ChangeElement クラス
```csharp
class ChangeElement {
    string elementId;   // "door", "window", "book", "clock" など
    bool isReal;        // true=本物の法則、false=偽変化（トラップ）
    int appearsOnLoop;  // このループ番号に現れる（ランダム配置）
    bool isReverse;     // Stage5: 逆行フラグ
}
```

### 部屋内オブジェクト構成
- `RoomObject`（SpriteRenderer付きコライダ）を4〜6個配置
- オブジェクト種類: door, window, book, clock, picture, plant
- 各ループ開始時に変化要素に応じてオブジェクトの色/形が変わる
- タップで「発見！」メッセージ + メモに追加（本物・偽物関係なく）
- 脱出アクションボタン押下時にのみ法則判定

## 入力処理フロー
1. `LoopManager.Update()` で `Mouse.current.leftButton.wasPressedThisFrame` 検出
2. `Mouse.current.position.ReadValue()` でスクリーン座標取得
3. `Camera.main.ScreenToWorldPoint()` でワールド座標変換
4. `Physics2D.OverlapPoint()` でRoomObjectのコライダヒット判定
5. ヒットした場合 `OnObjectTapped(gameObject.name)` 呼び出し

## SceneSetup 構成方針

### Setup098v2_InfiniteLoop.cs
- MenuItem: `"Assets/Setup/098v2 InfiniteLoop"`
- 部屋背景（背景色は暗い青紫＝unique カテゴリ）
- RoomObjectを5個配置（円コライダ付き、ワールド空間に配置）
- InfiniteLoopGameManager（親）
  - StageManager（子）
  - LoopManager（子）
- Canvas（メインHUD）
  - StageText（左上）
  - ScoreText（右上）
  - LoopCountText（中上、目立つ赤色）
  - MemoButton（右下、常時表示）
  - EscapeButton（下中央、脱出試行ボタン）
  - BackToMenuButton（左下）
  - MemoPanel（全画面オーバーレイ、メモ内容表示）
  - StageClearPanel
  - GameOverPanel
  - AllClearPanel
- InstructionPanel（共通）
- EventSystem + InputSystemUIInputModule

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

void OnStageChanged(int stageIndex) {
    var config = _stageManager.GetCurrentStageConfig();
    _loopManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

## InstructionPanel内容
- title: `"InfiniteLoop"`
- description: `"ループする世界の法則を見つけ出そう"`
- controls: `"タップでオブジェクト調査・アクション実行、ボタンでメモ確認"`
- goal: `"ループの法則を発見して世界から脱出しよう"`

## ビジュアルフィードバック設計

### 1. オブジェクト発見時: スケールパルス
```csharp
IEnumerator PulseScale(Transform t) {
    Vector3 orig = t.localScale;
    float t0 = 0f;
    while (t0 < 0.2f) {
        t0 += Time.deltaTime;
        float s = 1f + 0.3f * Mathf.Sin(t0 / 0.2f * Mathf.PI);
        t.localScale = orig * s;
        yield return null;
    }
    t.localScale = orig;
}
```

### 2. ループリセット時: 画面フラッシュ（白→透明）
- 全画面の白いImageを一瞬表示してフェードアウト（0.3秒）

### 3. 偽変化に引っかかった時: 赤フラッシュ + テキスト「だまされた！」
- SpriteRenderer.color を赤に変化させて元に戻す

### 4. 正しい脱出: 緑フラッシュ + スケールアップ演出

## スコアシステム
- 基本スコア: 変化発見1回 = 50pt × (stageIndex+1)
- 連続正解発見（同ループ内で複数発見）: ×1.3乗算
- クリア早期ボーナス: 残りループ数 × 20pt
- 最少ループ数クリア（残5以上）: スコア2倍
- メモ未使用クリア: +150pt

## レスポンシブ配置（必須）
```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;  // HUD用
float bottomMargin = 3.0f;  // ボタン用
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// 部屋の縦幅: availableHeight (≒7.5 units)
// RoomObjectは横に均等配置
```

## ステージ別新ルール表
| Stage | 基本 | 新要素 |
|-------|------|-------|
| 1 | 変化1個・ループ10 | チュートリアル的。必ずヒントを表示 |
| 2 | 変化2個・ループ10 | 2つを組み合わせないと脱出不可 |
| 3 | 変化2-3個・ループ8 | 変化の出現ループが毎回ランダム |
| 4 | 変化3個・ループ7 | 偽変化1個追加（見た目同じ） |
| 5 | 変化3個・ループ5 | 偽変化2個＋逆行ループ（時々逆順） |

## SceneSetupでの全フィールド配線
- `InfiniteLoopGameManager._stageManager` ← StageManagerコンポーネント
- `InfiniteLoopGameManager._instructionPanel` ← InstructionPanelコンポーネント
- `InfiniteLoopGameManager._loopManager` ← LoopManagerコンポーネント
- `InfiniteLoopGameManager._ui` ← InfiniteLoopUIコンポーネント
- `LoopManager._gameManager` ← InfiniteLoopGameManagerコンポーネント
- `LoopManager._ui` ← InfiniteLoopUIコンポーネント
- `LoopManager._roomObjects` ← RoomObjectの配列（5個）
- `LoopManager._flashImage` ← フラッシュ用Imageコンポーネント
- `InfiniteLoopUI._stageText`, `_scoreText`, `_loopCountText`
- `InfiniteLoopUI._memoPanel`, `_escapeButton`, `_backToMenuButton`
- `InfiniteLoopUI._stageClearPanel`, `_gameOverPanel`, `_allClearPanel`
- `InfiniteLoopUI._gameManager` ← InfiniteLoopGameManagerコンポーネント

## 判断ポイントの実装設計
- **トリガー条件**: 脱出ボタン押下時 → `LoopManager.TryEscape()`
  - `currentLoop <= loopLimit - 3` かつ 法則未確認 → 高リスク（ループ消費）
  - `currentLoop >= loopLimit - 1` → 強制試行（ループ節約不可）
- **報酬**: 正しい法則で脱出 → ステージクリア + ボーナスpt
- **ペナルティ**: 間違いで脱出試行 → ループ+2消費（重ペナルティ）
