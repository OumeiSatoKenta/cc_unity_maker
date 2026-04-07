# 設計: Game079v2 SilentBeat

## namespace
`Game079v2_SilentBeat`

## スクリプト構成

### SilentBeatGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- Start() → InstructionPanel.Show() → OnDismissed → StartGame() → _stageManager.StartFromBeginning()
- _stageManager.OnStageChanged += OnStageChanged
- _stageManager.OnAllStagesCleared += OnAllStagesCleared
- OnStageChanged: RhythmManagerにステージ設定を渡す
- スコア管理（コンボ倍率計算含む）

### RhythmManager.cs
- コアメカニクス: BPMベースのリズムタップ判定
- ガイドフェーズ: 視覚パルスをBPMに合わせて表示
- タップフェーズ: タップ間隔と期待間隔のズレで判定
- 入力処理一元管理: `Mouse.current.leftButton.wasPressedThisFrame`
- `_isActive` ガードで無効時のUpdate停止
- ステージ設定:
  ```
  Stage1: BPM=60, guide=8, taps=20, visualHint=true
  Stage2: BPM=90, guide=4, taps=25, visualHint=false
  Stage3: BPM=120, guide=4, taps=30, bpmChange=true (→100 at tap15)
  Stage4: BPM=80→120, guide=4+4, taps=40, gradualBPM=true
  Stage5: BPM=150, guide=2, taps=50, randomSpeed=true
  ```
- BPM変化ロジック: Stage3では中間でBPM変更、Stage4では段階的変化、Stage5ではランダムゾーン
- 判定:
  - Perfect: ±20ms → 150pt × (1.0 + combo × 0.15, max 4.0)
  - Great: ±50ms → 80pt × (1.0 + combo × 0.08, max 2.5)
  - Good: ±100ms → 30pt × 1.0
  - Miss: >100ms → 0pt, コンボリセット
- Miss連続3回でゲームオーバー通知
- 全タップPerfect判定フラグ → ボーナス×3.0

### SilentBeatUI.cs
- HUD: スコア、コンボ、Stage X/5、残りタップ、BPM表示（ガイド中のみ）
- 判定テキスト（中央、一時表示: Perfect/Great/Good/Miss + 色分け）
- 精度インジケーター（ズレ方向の視覚表示）
- ステージクリアパネル（「次のステージへ」ボタン）
- 最終クリアパネル
- ゲームオーバーパネル
- タップエリアの視覚パルス演出（ガイドフェーズ）

## GameManager参照取得
- RhythmManager: `GetComponentInParent<SilentBeatGameManager>()` でGameManager取得
- SilentBeatUI: `[SerializeField]` で直接参照

## 状態遷移フロー

### クリアフロー
Playing → (全タップ完了) → StageClear → (ボタン) → Playing (次ステージ)
→ (Stage5完了) → AllClear

### ゲームオーバーフロー
Playing → (Miss×3連続) → GameOver

## SceneSetup配線フィールド
- SilentBeatGameManager._stageManager
- SilentBeatGameManager._instructionPanel
- SilentBeatGameManager._ui (SilentBeatUI)
- SilentBeatGameManager._rhythmManager (RhythmManager)
- RhythmManager._tapArea (SpriteRenderer - タップエリア)
- RhythmManager._pulseIndicator (SpriteRenderer - パルスインジケーター)
- SilentBeatUI._scoreText, _comboText, _stageText, _remainText
- SilentBeatUI._judgementText, _bpmText
- SilentBeatUI._accuracyIndicator (Image - ズレ方向表示)
- SilentBeatUI._stageClearPanel, _allClearPanel, _gameOverPanel
- SilentBeatUI._nextStageButton, _retryButton, _menuButton系

## InstructionPanel内容
- title: "SilentBeat"
- description: "無音の中でリズムを感じ取りタップし続けよう"
- controls: "画面をタップしてリズムを刻もう"
- goal: "ガイドと同じテンポでタップしてコンボを繋ごう"

## ビジュアルフィードバック設計
1. **タップパルス**: タップ時にタップエリアのScale 1.0→1.3→1.0（0.15秒）+ 判定に応じた色変化
   - Perfect: シアン輝き
   - Great: 緑
   - Good: 黄
   - Miss: 赤フラッシュ
2. **ガイドパルス**: BPMに合わせてpulseIndicatorのAlpha 0→1→0のビート表示
3. **コンボ演出**: コンボ10以上でスコアテキストにスケールパルス

## レスポンシブ配置
- カメラorthographicSize基準で動的計算
- タップエリア: 画面中央、半径 = camSize * 0.5
- HUD: 上部マージン1.2u以上
- UIボタン: 下部マージン2.5u以上
