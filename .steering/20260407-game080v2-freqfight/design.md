# Design: Game080v2 FreqFight

## Namespace
`Game080v2_FreqFight`

## スクリプト構成

### FreqFightGameManager.cs
- **役割**: ゲーム状態管理、StageManager・InstructionPanel統合
- **フィールド**:
  - `[SerializeField] StageManager _stageManager`
  - `[SerializeField] InstructionPanel _instructionPanel`
  - `[SerializeField] FreqFightManager _freqManager`
  - `[SerializeField] FreqFightUI _ui`
- **Start()**: InstructionPanel.Show() → OnDismissed += StartGame
- **StartGame()**: スコアリセット → StageManager購読 → StartFromBeginning()
- **OnStageChanged(int)**: FreqFightManager.SetupStage() + UI更新
- **OnAllStagesCleared()**: 最終クリア表示
- **公開メソッド**: OnStageClear/NextStage/OnGameOver/UpdateScore/UpdateCombo/ShowJudgement/UpdatePhase/UpdateBeat

### FreqFightManager.cs
- **役割**: コアメカニクス（周波数バトル、BPMタイマー、判定ロジック）
- **状態**: enum BattlePhase { Attack, Defense }
- **主要フィールド**:
  - `_gameManager`: GetComponentInParent<FreqFightGameManager>()
  - `_playerFreq`: float（スライダー値）
  - `_enemyFreq`: float（敵の現在周波数）
  - `_targetFreq`: float（次のビートの目標周波数）
  - `_bpm`: float
  - `_beatInterval`: float (60f / _bpm)
  - `_beatTimer`: float
  - `_enemyHp`: float
  - `_playerHp`: float (100f)
  - `_combo`: int
  - `_totalScore`: int
  - `_isActive`: bool（ガード）
  - `_currentPhase`: BattlePhase
  - `_fakeoutActive`: bool（フェイントフラグ、Stage3+）
  - `_enemyCount`: int（1 or 2, Stage4+）
- **SetupStage(StageConfig config, int stageIndex)**:
  - stageIndex 0: BPM=60, range=200-400, noDefense
  - stageIndex 1: BPM=80, range=200-600, withDefense
  - stageIndex 2: BPM=100, range=150-800, withFakeout
  - stageIndex 3: BPM=120, range=100-1000, dualEnemy
  - stageIndex 4: BPM=140, range=100-1200, bossMode
- **Update()**: `if(!_isActive) return;` → BPMタイマー更新 → ビートタイミングでJudge()
- **OnSliderChanged(float value)**: プレイヤー周波数更新
- **Judge()**: 周波数差でPerfect/Great/Good/Miss判定 → ダメージ計算 → コンボ更新
- **新Input System禁止**: スライダーはUnity UIのOnValueChanged使用
- **レスポンシブ配置**: Canvas上のスライダーのため固定座標ではなくアンカー使用

### FreqFightUI.cs
- **役割**: UI表示管理
- **必須表示**:
  - ステージ表示「Stage X / 5」
  - スコア表示（コンボ倍率込み）
  - ステージクリアパネル（「次のステージへ」ボタン）
  - 最終クリアパネル
  - ゲームオーバーパネル
  - コンボ表示（アニメーション付き）
  - 判定テキスト（Perfect/Great/Good/Miss）
  - フェーズ表示（攻撃/防御）
  - 自分HPバー
  - 敵HPバー（1または2本）
  - ビートガイド（円形インジケーター）
  - 周波数スライダー（1または2本）
  - 敵周波数マーカー

## InstructionPanel内容
- **gameId**: "080"
- **title**: "FreqFight"
- **description**: "敵の周波数に合わせてスライダーを調整し、ビートに乗ってロックオン攻撃！"
- **controls**: "スライダーをドラッグして敵と同じ音程に合わせよう。ビートに同期して自動判定される"
- **goal**: "5ステージの敵を全滅させて周波数マスターになれ！"

## StageManager統合
```csharp
// OnStageChanged(int stageIndex)
var config = _stageManager.GetCurrentStageConfig();
_freqManager.SetupStage(config, stageIndex);
_ui.UpdateStage(stageIndex + 1, 5);
```

## ステージ別パラメータ表
| Stage | BPM | FreqMin | FreqMax | EnemyHP | HasDefense | HasFakeout | EnemyCount |
|-------|-----|---------|---------|---------|------------|------------|------------|
| 1 | 60 | 200 | 400 | 200 | false | false | 1 |
| 2 | 80 | 200 | 600 | 350 | true | false | 1 |
| 3 | 100 | 150 | 800 | 500 | true | true | 1 |
| 4 | 120 | 100 | 1000 | 400（×2） | true | true | 2 |
| 5 | 140 | 100 | 1200 | 800 | true | true | 1 |

StageConfig対応:
- `speedMultiplier` → BPM倍率 (基準BPM×speedMultiplier)
- `countMultiplier` → enemyCount
- `complexityFactor` → hasDefense/hasFakeout判定（>0でdefense有効、>0.4でfakeout有効）

## ビジュアルフィードバック設計
1. **判定ヒット時スケールパルス**: 判定テキストが1.0→1.5→1.0に0.2秒でパルス + 色変化（Perfect=金、Great=緑、Good=白、Miss=赤）
2. **敵HPダメージ時の敵キャラ揺れ**: SpriteRenderer付き敵オブジェクトが左右シェイク0.15秒
3. **コンボ10以上でゴールド表示**: コンボテキストが黄金色に変化
4. **ビートガイドの拍動**: 円形イメージがBPMに合わせてスケールパルス（0.9→1.1→0.9）

## スコアシステム
- 基本スコア = ダメージ × コンボ倍率
- Perfect: 倍率 = 1.0 + combo × 0.2（上限4.0）
- Great: 倍率 = 1.0 + combo × 0.1（上限2.5）
- Good: 倍率 = 1.0（固定）
- Miss: スコア0 + コンボリセット

## SceneSetup構成方針（Setup080v2_FreqFight.cs）
- `[MenuItem("Assets/Setup/080v2 FreqFight")]`
- Camera背景色: ダーク紫 (0.04, 0.01, 0.10)
- Canvas: 1080×1920, ScaleWithScreenSize
- GameManager階層:
  - FreqFightGameManager
    - StageManager
    - FreqFightManager
- 全フィールド配線:
  - GM._stageManager, GM._instructionPanel, GM._freqManager, GM._ui
  - FreqFightManager._gameManager（GetComponentInParent使用）
  - UI._stageText, UI._scoreText, UI._comboText, UI._judgementText
  - UI._phaseText, UI._beatGuide, UI._playerHpSlider, UI._enemyHpSlider1/2
  - UI._playerFreqSlider, UI._enemyFreqSlider（スライダー2本対応）
  - UI._enemySprite（揺れ演出用）
  - UI._stageClearPanel, UI._gameOverPanel, UI._allClearPanel

## レスポンシブ配置
- 全UIはCanvas上のアンカー配置（ワールド座標なし）
- 敵キャラはワールド座標配置: `camSize * 0.5`の位置
- スライダー: 画面下部、Canvas下端Y=150付近に固定

## Buggy Code防止
- `_isActive`ガードを全Update()に配置
- `Texture2D`生成は使用後`Destroy(tex)`でクリーンアップ（OnDestroy内）
- スライダーのOnValueChangedは`RemoveAllListeners()`後に登録
