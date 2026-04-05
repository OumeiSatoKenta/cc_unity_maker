# Design: Game017v2_CrystalSort

## スクリプト構成

```
Scripts/Game017v2_CrystalSort/
├── CrystalSortGameManager.cs   # ゲーム状態管理・StageManager/InstructionPanel統合
├── BottleManager.cs             # 瓶・クリスタル生成・移動ロジック・入力処理
└── CrystalSortUI.cs             # UI表示・パネル管理
```

### namespace
`Game017v2_CrystalSort`

### CrystalSortGameManager.cs
- **役割**: ゲーム状態（WaitingInstruction/Playing/StageClear/Clear/GameOver）管理、スコア計算
- **フィールド（SerializeField）**:
  - `StageManager _stageManager`
  - `InstructionPanel _instructionPanel`
  - `BottleManager _bottleManager`
  - `CrystalSortUI _ui`
- **Start()の流れ**:
  1. `_instructionPanel.Show("017v2", "CrystalSort", "同じ色のクリスタルを同じ瓶に集めよう", "瓶タップで選択 → 移動先の瓶タップで移動", "少ない手数で全瓶を単色に揃えよう")`
  2. `_instructionPanel.OnDismissed += StartGame`
  3. `StartGame()` で `_stageManager.StartFromBeginning()`
- **StageManager統合**:
  - `OnStageChanged(int stage)` で `_bottleManager.SetupStage(config, stage+1)` を呼び出し
  - `OnAllStagesCleared()` で最終クリアパネル表示

### BottleManager.cs
- **役割**: 瓶とクリスタルのゲームオブジェクト生成・管理、入力処理、移動ロジック
- **入力処理**: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint`
- **ステージ別パラメータ（StageManager.StageConfigから取得）**:
  - `speedMultiplier` → 移動アニメーション速度
  - `countMultiplier` → 瓶数計算
  - `complexityFactor` → 特殊要素（蓋、虹、氷漬け、タイマー）の有効化フラグ
- **レスポンシブ配置**:
  - `Camera.main.orthographicSize` と `aspect` から動的計算
  - 上部マージン: 1.5f（HUD）
  - 下部マージン: 2.8f（UI）
- **クリスタルの種類**:
  - Normal: 通常クリスタル（6色: Red, Blue, Green, Yellow, Purple, Orange）
  - Rainbow: どの色にも置ける（Stage3以降）
  - Frozen: 解凍が必要（Stage4以降）
- **瓶の種類**:
  - Normal: 通常の瓶
  - Capped: 蓋付き（Stage2以降、2タップで開封）
  - Timer: タイマー瓶（Stage5以降、指定手数で蓋が閉まる）
- **選択状態管理**: `_selectedBottleIndex`（-1=未選択）
- **移動可否判定**:
  - 移動先が空き瓶 → OK
  - 移動先の一番上が同色 and 容量以内 → OK
  - 虹クリスタルの場合 → 常にOK（容量以内）
  - 蓋付き瓶で蓋が閉まっている → NG
  - 氷漬けクリスタルが一番上 → 移動不可（解凍後はOK）
- **ビジュアルフィードバック**:
  - クリスタル選択時: `transform.localScale` → 1.3倍ポップアップアニメ（0.1秒）
  - 移動成功時: 移動先クリスタルのスケールパルス（1.0→1.2→1.0, 0.2秒）
  - 移動失敗時: 選択クリスタルの赤フラッシュ（0.15秒）
  - 瓶完成時: 瓶全体のゴールドフラッシュ（0.3秒）+ パーティクル風スケールバウンス
  - コンボ時: 選択クリスタルの色変化（白→元色）+ スケール1.4倍

### CrystalSortUI.cs
- **役割**: HUD表示・パネル管理
- **必須要素**:
  - ステージ表示「Stage X / 5」
  - スコア表示（コンボ乗算込み）
  - 残り手数表示
  - コンボテキスト（一時表示）
  - 瓶完成数表示
  - ステージクリアパネル（「次のステージへ」ボタン）
  - 最終クリアパネル（全5ステージ完了）
  - ゲームオーバーパネル
  - 「?」ボタン（説明再表示）
  - メニューへ戻るボタン

## 盤面・ステージデータ設計

### ステージ設定（StageManager.StageConfig活用）

| Stage | speedMultiplier | countMultiplier | complexityFactor |
|-------|----------------|-----------------|-----------------|
| 1 | 1.0 | 1.0 | 0.0 (基本のみ) |
| 2 | 1.0 | 1.25 | 0.25 (蓋付き瓶) |
| 3 | 1.0 | 1.5 | 0.5 (虹クリスタル) |
| 4 | 1.0 | 1.75 | 0.75 (氷漬け+深い瓶) |
| 5 | 1.0 | 2.0 | 1.0 (全要素統合) |

### 瓶パラメータ（SetupStage内で計算）
- Stage1: 3色, 4瓶(1空き), 容量4, 手数20
- Stage2: 4色, 5瓶(1空き), 容量4, 手数25
- Stage3: 5色, 6瓶(1空き), 容量4, 手数35
- Stage4: 5色, 7瓶(2空き), 容量5, 手数45
- Stage5: 6色, 8瓶(1空き), 容量4, 手数55

## 入力処理フロー

```
Update()
  → Mouse.current.leftButton.wasPressedThisFrame
    → worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue())
    → Collider2D hit = Physics2D.OverlapPoint(worldPos)
    → if (hit != null) 
        → BottleView bottle = hit.GetComponentInParent<BottleView>()
        → if (_selectedBottleIndex == -1) → SelectBottle(bottle)
        → else if (bottle == _selectedBottle) → DeselectBottle()
        → else → TryMoveToBottle(bottle)
```

## SceneSetup 構成方針

`Setup017v2_CrystalSort.cs` in `Assets/Editor/SceneSetup/`

- `[MenuItem("Assets/Setup/017v2 CrystalSort")]`
- Camera: 背景色 `(0.08f, 0.05f, 0.15f)`（深い紫）
- Sprites: Pillow生成（puzzle カラー: #2196F3青系）
- Canvas（ScreenSpaceOverlay, sortingOrder=10）
- HUD上部: ステージ, スコア, 残り手数, コンボ
- 下部ボタン: 「メニューへ戻る」（左下）、「?」ボタン（右下）
- パネル: StageClearPanel, ClearPanel, GameOverPanel
- GameManager → StageManager, BottleManager を子オブジェクト
- InstructionPanel（全画面オーバーレイ, sortingOrder=100）

## InstructionPanel内容

- **title**: "CrystalSort"
- **description**: "同じ色のクリスタルを同じ瓶に集めよう"
- **controls**: "瓶タップで選択 → 移動先の瓶タップで移動"
- **goal**: "少ない手数で全瓶を単色に揃えよう"

## ビジュアルフィードバック設計

1. **クリスタル選択**: スケール1.3倍ポップ（0.1秒）+ Y位置+0.3上昇
2. **移動成功**: 移動先スケールパルス（1.0→1.2→1.0, 0.2秒）
3. **移動失敗**: 赤フラッシュ（SpriteRenderer.color → 赤、0.15秒後リセット）
4. **瓶完成**: ゴールドフラッシュ（0.3秒）+ バウンスアニメ（全クリスタルが1.1倍パルス）
5. **コンボ**: スケール1.4倍 + テキスト「コンボ×N!」を0.5秒表示

## スコアシステム

```
baseScore = 1000 × (stage + 1)
stageScore = baseScore × (1 + remainingMoves / maxMoves)
comboMultiplier = combo >= 5 ? 2.0 : combo >= 3 ? 1.6 : combo >= 2 ? 1.3 : 1.0
bottleCompleteMultiplier = 1.1 ^ completedBottles
finalScore = stageScore × comboMultiplier × bottleCompleteMultiplier
```

同色移動コンボ: 2連続=+50pt, 3連続=+150pt, 5連続=+500pt

## ステージ別新ルール表

| Stage | ルール |
|-------|--------|
| 1 | 基本のみ（同色の上に積む、空き瓶に移動） |
| 2 | 蓋付き瓶: 2タップ（1回目=蓋開け、2回目=通常操作） |
| 3 | 虹クリスタル: どの色にも置けるが最後に除去が必要 |
| 4 | 氷漬けクリスタル: 隣接同色2個で自動解凍。容量5の深い瓶が出現 |
| 5 | タイマー瓶: 指定手数以内に空にしないと蓋が閉まる。全要素複合 |

## 判断ポイントの実装設計

- **空き瓶使用の判断**: 詰まり検出（全瓶の一番上が異なる色 and 空き瓶なし）→ ゲームオーバー警告
- **虹クリスタル使用**: 任意の瓶の一番上に配置可能、配置すると後続の同色移動がブロックされる可能性があるため慎重な判断が必要
- **報酬/ペナルティ**:
  - 正解移動（同色揃い方向）: コンボカウント+1
  - 不正解移動（バラバラにする方向）: コンボリセット
  - 瓶完成: ボーナス得点 + 乗算倍率+0.1
