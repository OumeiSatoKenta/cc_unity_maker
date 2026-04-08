# Design: Game090v2_StarshipCrew

## namespace
`Game090v2_StarshipCrew`

## スクリプト構成

| ファイル | クラス | 役割 |
|---------|------|------|
| StarshipCrewGameManager.cs | StarshipCrewGameManager | ゲーム統括・スコア・コンボ管理、StageManager/InstructionPanel配線 |
| CrewManager.cs | CrewManager | クルーデータ・選択・ミッション派遣・結果計算・相性システム |
| StarshipCrewUI.cs | StarshipCrewUI | 全UI制御（HUD、ミッションパネル、クルーカード、リザルト） |

### データクラス（CrewManager.cs内）
- `CrewData`: クルー情報（名前、スキル種別、スキル値、相性リスト）
- `MissionData`: ミッション情報（名前、必要スキル、難易度、報酬点、最大派遣人数）

## 盤面・ステージデータ設計

### クルーデータ（全10人のプール）
```
クルー0: 艦長オリン   - CombatSkill=80, EngSkill=30, MedSkill=20 / 相性良: クルー1,3 / 相性悪: クルー5
クルー1: エンジニアKai - CombatSkill=20, EngSkill=90, MedSkill=30 / 相性良: クルー0,2
クルー2: 医療士Nora  - CombatSkill=10, EngSkill=30, MedSkill=85 / 相性良: クルー1,4
クルー3: パイロットRex - CombatSkill=60, EngSkill=70, MedSkill=20 / 相性良: クルー0,4
クルー4: 科学者Lyra  - CombatSkill=20, EngSkill=50, MedSkill=60 / 相性良: クルー2,3
クルー5: 傭兵Zack    - CombatSkill=95, EngSkill=20, MedSkill=10 / 相性悪: クルー0 / 相性良: クルー7
クルー6: 整備士Mia   - CombatSkill=30, EngSkill=80, MedSkill=40 / 相性良: クルー1,8
クルー7: 交渉人Sora  - CombatSkill=40, EngSkill=40, MedSkill=50 / 相性良: クルー5,9
クルー8: 狙撃手Ares  - CombatSkill=85, EngSkill=25, MedSkill=15 / 相性良: クルー6
クルー9: 副官Luna    - CombatSkill=50, EngSkill=60, MedSkill=70 / 相性良: クルー7,2
```

### ミッションデータ（15ミッション定義）
```
M0: 偵察任務      - 必要スキル=Combat(30), 難易度Easy
M1: エンジン修理   - 必要スキル=Eng(50), 難易度Medium
M2: 負傷者救助    - 必要スキル=Med(40), 難易度Medium
M3: 海賊撃退      - 必要スキル=Combat(60), 難易度Hard
M4: ワープ航法    - 必要スキル=Eng(70), 難易度Hard
M5: 疫病対応      - 必要スキル=Med(60), 難易度Hard
M6: 惑星探索      - 必要スキル=Combat(40)+Eng(40), 難易度Medium
M7: 外交交渉      - 必要スキル=Med(30)+Eng(40), 難易度Medium
M8: 小惑星帯突破  - 必要スキル=Eng(80), 難易度VeryHard
M9: 高度医療      - 必要スキル=Med(80), 難易度VeryHard
M10: 基地強襲     - 必要スキル=Combat(80), 難易度VeryHard
M11: 緊急脱出     - 必要スキル=Combat(50)+Eng(50)+Med(30), 難易度Hard
M12: 文明接触     - 必要スキル=Med(50)+Eng(50), 難易度VeryHard
M13: 艦隊決戦     - 必要スキル=Combat(90), 難易度Boss（Stage5のみ）
M14: 最終ミッション - 必要スキル=全スキル総合80+, 難易度Boss（Stage5のみ）
```

### ステージ別ミッション配分
- Stage 1: M0, M1, M2（3ミッション、2以上クリアで突破）
- Stage 2: M3, M4, M5, M6（4ミッション、3以上クリアで突破）
- Stage 3: M7, M8, M9, M6, M10（5ミッション、4以上クリアで突破）
- Stage 4: M10, M11, M8, M9, M12, M3（6ミッション、5以上クリアで突破）
- Stage 5: M13, M14, M11, M12, M9, M10, M4（7ミッション、6以上クリアで突破）

## 入力処理フロー
- **入力一元管理**: `CrewManager.Update()` でタップ判定
- クルーカード（UI Button）タップ → `OnCrewCardClicked(crewIndex)` → 選択状態トグル
- ミッションボタンタップ → `OnMissionSelected(missionIndex)` → 派遣確認UIへ
- 派遣ボタンタップ → `OnDispatchClicked()` → 成功率計算 → アニメーション → 結果表示
- 続けるボタンタップ → `OnContinueClicked()` → ミッション選択へ戻る

## SceneSetup の構成方針

### ヒエラルキー
```
Main Camera
Background (SpriteRenderer)
StarshipCrewGameManager
  StageManager (child)
  CrewManager (child) ← SerializeField で GM と UI を参照
Canvas (sortingOrder=10)
  HUD
    StageText
    ScoreText
    ComboText
  CrewPanel
    CrewCardsContainer (GridLayoutGroup)
      CrewCard x10
  MissionPanel
    MissionListContainer
      MissionButton x7
    DispatchButton
    CancelButton
  ResultPanel (hidden)
    ResultText
    BonusText
    ContinueButton
  StageClearPanel (hidden)
  AllClearPanel (hidden)
  GameOverPanel (hidden)
  InstructionPanel (Canvas above, sortingOrder=20)
  HelpButton (右下、?ボタン)
EventSystem
```

## StageManager統合

### OnStageChanged 購読
```csharp
void OnStageChanged(int stageIndex) {
    _combo = 0;
    _scoreMultiplier = 1.0f;
    _isPlaying = true;
    var config = _stageManager.GetCurrentStageConfig();
    _crewManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
    _ui.UpdateScore(_totalScore);
    _ui.UpdateCombo(_combo, _scoreMultiplier);
}
```

### ステージ別パラメータ表
| Stage | speedMultiplier | countMultiplier | complexityFactor | クルー数 | ミッション数 | 必要クリア |
|-------|----------------|----------------|-----------------|---------|------------|-----------|
| 1     | 1.0            | 1              | 0.0             | 3       | 3          | 2         |
| 2     | 1.2            | 1              | 0.3             | 5       | 4          | 3         |
| 3     | 1.5            | 2              | 0.5             | 6       | 5          | 4         |
| 4     | 1.8            | 2              | 0.8             | 8       | 6          | 5         |
| 5     | 2.0            | 3              | 1.0             | 10      | 7          | 6         |

## InstructionPanel内容
- title: "StarshipCrew"
- description: "クルーを育てて銀河探検に出発しよう"
- controls: "クルーをタップして選択、ミッションをタップして派遣"
- goal: "最強のクルー編成で全ミッションをクリアしよう"

## ビジュアルフィードバック設計

### 1. ミッション成功時: スケールポップアニメーション
- `ResultPanel` がスケール 0 → 1.2 → 1.0 に0.3秒でアニメーション
- ResultPanel背景色: 成功=緑系、失敗=赤系、完璧=金色
- `StartCoroutine(PopAnimation(resultPanel.transform))`

### 2. 相性シナジー時: 色フラッシュ + 輝き
- 良相性のクルーカード同士をタップすると緑のハイライト点滅（0.5秒）
- 3人シナジー成立時: 全選択カードがゴールドに輝く（`SpriteRenderer.color` 変更）

### 3. ミッション失敗時: 赤フラッシュ + コンボリセット演出
- 画面端が赤くフラッシュするエフェクト（Imageの透明度 0.5→0）
- コンボカウンターが揺れるシェイクアニメーション

### 4. クルーカード選択時: スケールパルス
- 選択時: localScale 1.0 → 1.1（選択状態を視覚的に示す）
- 解除時: 1.1 → 1.0 に戻る

## スコアシステム
- **基本スコア**: ミッション成功=30pt、完璧クリア（成功率90%超）=追加50pt
- **コンボ乗算**: combo=1:×1.0, combo=2:×1.2, combo=3:×1.5, combo=4+:×2.0
- **相性ボーナス**: 良相性ペア1組=+30pt、3人シナジー=+80pt（追加）
- **ミッション難易度ボーナス**: Easy=×1.0, Medium=×1.2, Hard=×1.5, VeryHard=×2.0, Boss=×3.0
- **コンボリセット**: 失敗でコンボ0、乗算も×1.0に戻る

## ステージ別新ルール表
| Stage | 新ルール |
|-------|---------|
| 1 | 基本ルールのみ。3人から選んで3ミッションに挑戦 |
| 2 | 相性システム追加。クルー間の相性が成功率±20%に影響。UIに相性表示 |
| 3 | 装備スロット追加。ミッション派遣前に装備を1つ選択（スキル+10〜20補正） |
| 4 | 緊急イベント追加。ミッション中にランダムイベント（隕石・裏切り）が発生し、失敗上限が1回減 |
| 5 | ボスミッション追加（M13, M14）。3人の全相性コンボが成功率に大きく影響。シナジー×1.5必須 |

## 判断ポイントの実装設計

### トリガー条件
1. **クルー選択時**: 選択人数が1〜3人の間で「派遣ボタン」が有効化 → 成功率をリアルタイム表示
2. **ミッション選択時**: ミッションボタンをタップすると成功率プレビュー（%）表示
3. **装備選択時（Stage3+）**: 装備選択ドロップダウンで成功率変化を即反映

### 報酬/ペナルティ数値
- 成功率計算式: `(totalSkillMatch / requiredSkill) * 100 * compatBonus * equipBonus`
- 良相性ペア: +20%成功率、悪相性: -15%成功率
- 3人シナジー: ×1.5倍乗算
- 装備ボーナス: +10〜+25%成功率（装備種別による）
- 失敗時: コンボリセット、次回同じミッションの難易度-5%（「経験値」）

## GameManager参照取得方法
- `CrewManager._gameManager`: `[SerializeField]` でSceneSetupから配線
- `CrewManager._ui`: `[SerializeField]` でSceneSetupから配線
- `StarshipCrewUI._gameManager`: `[SerializeField]` でSceneSetupから配線
- `StarshipCrewUI._crewManager`: `[SerializeField]` でSceneSetupから配線

## SceneSetup配線フィールド一覧
StarshipCrewGameManager:
- `_stageManager` → StageManager component
- `_instructionPanel` → InstructionPanel component
- `_crewManager` → CrewManager component
- `_ui` → StarshipCrewUI component

CrewManager:
- `_gameManager` → StarshipCrewGameManager component
- `_ui` → StarshipCrewUI component
- Sprite fields: `_sprCrew0`〜`_sprCrew9`, `_sprBackground`

StarshipCrewUI:
- `_gameManager` → StarshipCrewGameManager component
- `_crewManager` → CrewManager component
- UI Text fields: `_stageText`, `_scoreText`, `_comboText`
- Panel fields: `_stageClearPanel`, `_allClearPanel`, `_gameOverPanel`, `_resultPanel`
- Button fields: `_dispatchButton`, `_cancelButton`, `_continueButton`, `_nextStageButton`
- `_crewCardsContainer` → GridLayoutGroup parent
- `_missionButtonsContainer` → parent of mission buttons

## カテゴリカラーパレット（simulation）
- メインカラー: `#795548` (茶)
- サブカラー: `#FF8F00` (琥珀)
- アクセント: `#EFEBE9` (淡茶)
- 宇宙ゲームなのでカラーは宇宙寄りに調整:
  - 背景: 深宇宙（#0A0A2E 濃紺 → #1A1A4E グラデーション）
  - クルーカード: 茶系ベース（#5D4037 → #795548）
  - ミッションボタン: 琥珀系（#E65100 → #FF8F00）
  - 相性表示: 良相性=緑（#4CAF50）、悪相性=赤（#F44336）
