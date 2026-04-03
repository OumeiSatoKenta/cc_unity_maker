# 設計書: Game004v2_WordCrystal (Remake)

## namespace
`Game004v2_WordCrystal`

## スクリプト構成

### WordCrystalGameManager.cs
- GameState管理: WaitingInstruction / Playing / StageClear / Clear / GameOver
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] WordManager _wordManager
- [SerializeField] WordCrystalUI _ui
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): _stageManager.StartFromBeginning()
- OnStageChanged(int): _wordManager.SetupStage(config)
- OnAllStagesCleared(): GameState=Clear, ShowClearPanel
- OnWordSubmitted(string word, int score): スコア加算・コンボ更新・目標チェック
- OnTimerEnd(): GameState=GameOver, ShowGameOverPanel
- コンボカウンター・スコア乗算管理

### WordManager.cs (コアメカニクス)
- クリスタル配置・文字タイル管理・単語判定
- [SerializeField] GameObject _crystalPrefab (通常)
- [SerializeField] GameObject _crystalHiddenPrefab (裏面)
- [SerializeField] GameObject _crystalBonusPrefab (ボーナス金色)
- [SerializeField] GameObject _crystalPoisonPrefab (毒紫)
- [SerializeField] GameObject _letterTilePrefab
- Update(): Mouse.current.leftButton.wasPressedThisFrame → Physics2D.OverlapPoint
- SetupStage(StageConfig): クリスタル配置・単語リスト設定
- OnCrystalTapped(CrystalObject): 破壊アニメーション→LetterTile生成
- OnLetterTileTapped(LetterTile): スロットに追加
- OnSlotLetterTapped(int slotIndex): スロットから除去
- SubmitWord(): 単語検証→スコア計算→GameManagerへ通知
- ClearSlots(): スロット全クリア
- レスポンシブ配置: Camera.main.orthographicSize から動的計算

### WordCrystalUI.cs
- タイマー表示・スコア・目標スコア・コンボ倍率
- ステージ表示「Stage X / 5」
- 文字スロットUI（最大8スロット）
- 確定ボタン・クリアボタン
- ステージクリアパネル（★評価・次へボタン）
- 最終クリアパネル
- ゲームオーバーパネル（リトライ・メニューボタン）

### CrystalObject.cs
- CrystalType enum: Normal, Hidden, Bonus, Poison
- タップ検知はWordManagerに委譲（OnCrystalTapped呼び出し）
- 破壊アニメーション: スケール縮小→消滅（0.2秒）

### LetterTile.cs
- char _letter; bool _isBonus;
- タップ検知はWordManagerに委譲
- ポップアニメーション: 1.0→1.3→1.0（0.2秒）

## レスポンシブ配置設計

```
Camera orthographicSize = 6
上部マージン(HUD): 上端〜-1.5u → タイマー・スコア・ステージ
クリスタルエリア: -1.5u〜2.0u → グリッド配置
スロットエリア: 2.0u〜3.5u → 文字スロット（ワールド座標）
下部(Canvas UI): 下端2.8u → 確定・クリア・メニューボタン
```

```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;
float bottomMargin = 3.5f; // スロット+ボタン確保
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// クリスタルを3列×n行グリッド配置
int cols = 3;
int rows = Mathf.CeilToInt(crystalCount / (float)cols);
float cellSize = Mathf.Min(availableHeight / rows, camWidth * 2f / cols, 1.8f);
```

## ビジュアルフィードバック設計
1. **クリスタル破壊**: スケール0→1.5→0（0.25秒）+ パーティクル風（小球3個放散）
2. **文字スロット追加**: バウンスアニメーション（1.0→1.3→1.0, 0.15秒）
3. **正解時**: スロット全体が緑フラッシュ（0.3秒）+ スコアポップアップ
4. **誤答時**: スロット全体が赤フラッシュ（0.3秒）+ シェイク
5. **コンボ更新**: コンボテキストスケールパルス + 色変化（金色）

## スコアシステム
- 基本スコア: 3文字=100, 4文字=250, 5文字=500, 6文字以上=800
- コンボ倍率: 1連=x1.0, 2連=x1.5, 3連=x2.0, 4連以上=x3.0
- ボーナス文字使用: x2
- 誤答: コンボリセット（スコア減算なし）

## ステージ別パラメータ表

| Stage | timeLimit | targetScore | crystalCount | hasHidden | hasBonus | hasPoison | theme |
|-------|-----------|-------------|-------------|-----------|----------|-----------|-------|
| 1 | 60 | 500 | 8 | false | false | false | null |
| 2 | 60 | 800 | 9 | true | false | false | null |
| 3 | 50 | 1200 | 10 | true | true | false | null |
| 4 | 40 | 1500 | 10 | true | true | true | null |
| 5 | 35 | 2000 | 11 | true | true | true | "animal" |

## InstructionPanel内容
- title: "WordCrystal"
- description: "クリスタルから文字を集めて英単語を作るパズル"
- controls: "タップでクリスタル破壊、文字タイルをタップして並べる"
- goal: "制限時間内に目標スコアを達成しよう"

## SceneSetup構成（Setup004v2_WordCrystal.cs）
- MenuItem: "Assets/Setup/004v2 WordCrystal"
- Scene: Assets/Scenes/004v2_WordCrystal.unity
- GameManager → StageManager(子) → WordManager(子) → InstructionPanelController(子)
- Canvas: StageText, ScoreText, TargetScoreText, TimerText, ComboText
- 文字スロットPanel（WordSlotsContainer）: 8スロット横並び
- 確定ボタン・クリアボタン（下段横並び）
- メニューボタン（左下）
- StageClearPanel / GameClearPanel / GameOverPanel
- InstructionPanel（フルスクリーンオーバーレイ）
- 「?」ヘルプボタン（右下）

## SceneSetupで配線が必要なフィールド（GameManager）
- _stageManager → StageManager
- _instructionPanel → InstructionPanelController
- _wordManager → WordManager
- _ui → WordCrystalUI

## 判断ポイントの実装設計
- 毎10秒: 残り時間UIが赤に変化→プレイヤーに短単語切替を促す
- 裏クリスタルタップ時: 文字出現→良い文字なら使う/悪い文字なら無視（タイルを残したまま）
- 確定ボタン押下時に辞書照合→正解/不正解でコンボ更新
- 目標スコア達成瞬間: エフェクト+StageClearパネル表示
