# Design: Game076v2_ChordCatch

## namespace
`Game076v2_ChordCatch`

## スクリプト構成

### ChordCatchGameManager.cs
- ゲーム状態管理（Playing / StageClear / GameOver / AllClear）
- StageManager・InstructionPanel統合
- スコア・コンボ管理
- ChordController / ChordCatchUI への橋渡し

### ChordController.cs
- コアメカニクス実装
- AudioSource でプロシージャルに和音を生成（正弦波合成）
- コード定義（周波数テーブル）
- BPMカウンター・回答タイマー管理
- コードボタンのタップ処理
- Perfect/Great/Good/Miss判定
- ステージ設定（SetupStage）

### ChordCatchUI.cs
- HUD表示（スコア、コンボ、ミス数、問題進捗）
- 判定テキスト表示（アニメーション付き）
- コードボタン動的生成
- ステージクリア/ゲームオーバー/全クリアパネル

## コード定義（周波数）
和音はプロシージャルに生成（AudioClip + SetData）。各コードは4〜5音のリスト。

| コード | 構成音（Hz近似） |
|--------|----------------|
| C      | C4(261), E4(329), G4(392) |
| F      | F3(174), A3(220), C4(261) |
| G      | G3(196), B3(247), D4(294) |
| Am     | A3(220), C4(261), E4(329) |
| Dm     | D3(147), F3(174), A3(220) |
| Em     | E3(164), G3(196), B3(247) |
| G7     | G3(196), B3(247), D4(294), F4(349) |
| Dm7    | D3(147), F3(174), A3(220), C4(261) |
| C/E    | E3(164), C4(261), E4(329), G4(392) （転回形） |
| F/A    | A3(220), F3(174), A4(440), C5(523) （転回形） |

## 盤面・ステージデータ設計

```csharp
struct StageData {
    int bpm;
    string[] chords;       // 表示・出題コード一覧
    float answerTime;      // 秒
    int replayLimit;       // -1=無制限
    int questionCount;
    bool progressionMode;  // Stage5: コード進行問題
}
```

## 入力処理フロー
- コードボタンは Canvas UI Button（UnityEvent でタップ検知）
- リプレイボタンも同様
- タイミング判定: `_answerStartTime` からの経過時間と BPM周期の誤差で判定

## タイミング判定
```
BPM beat duration = 60f / bpm
answer elapsed = Time.time - questionStartTime
timing offset = |elapsed - beat_duration|
Perfect: offset < 0.04s
Great:   offset < 0.1s
Good:    offset < 0.2s
Miss:    タイムアウト or 誤答
```

## SceneSetup 構成方針
`Setup076v2_ChordCatch.cs`
- MenuItem: `Assets/Setup/076v2 ChordCatch`
- Camera, Canvas, GameManager, StageManager, ChordController, InstructionPanel, EventSystem 自動生成
- コードボタン群はランタイムで ChordController が動的に生成（SceneSetupでは親オブジェクトのみ作成）
- BPMガイド（ビジュアルメトロノーム）UIオブジェクトも生成

## StageManager統合
```csharp
// GameManager.Start()
_instructionPanel.Show("076", "ChordCatch", desc, controls, goal);
_instructionPanel.OnDismissed += StartGame;

// StartGame()
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

// OnStageChanged(int stage)
var config = _stageManager.GetCurrentStageConfig();
_chordController.SetupStage(config, stage);
_ui.UpdateStage(stage + 1, 5);
```

## StageConfig 設計
```
Stage 1: speedMultiplier=1.0, countMultiplier=1, complexityFactor=0.0
Stage 2: speedMultiplier=1.33, countMultiplier=1, complexityFactor=0.25
Stage 3: speedMultiplier=1.67, countMultiplier=1, complexityFactor=0.5
Stage 4: speedMultiplier=2.0, countMultiplier=1, complexityFactor=0.75
Stage 5: speedMultiplier=2.33, countMultiplier=1, complexityFactor=1.0
```
- speedMultiplier → BPM計算に使用
- complexityFactor → 0.0=メジャー3種のみ, 0.25=マイナー追加, 0.5=セブンス追加, 0.75=転回形追加, 1.0=進行問題

## InstructionPanel内容
- title: "ChordCatch"
- description: "和音を聴いて正しいコードをタップしよう！"
- controls: "鳴った和音に対応するコードボタンをタップ。リプレイボタンでもう一度聴けるよ"
- goal: "全問回答して正解率50%以上を達成しよう。コンボを繋げば高スコア！"

## ビジュアルフィードバック設計
1. **判定テキストアニメーション**: Perfect/Great/Good/Miss テキストが中央にポップアップ（スケール 0→1.3→1.0、0.3秒）
2. **コードボタン正解フラッシュ**: 正解ボタンが緑に光ってスケールパルス（1.0→1.2→1.0）
3. **ミス時カメラシェイク**: 誤答/タイムアウト時にカメラシェイク（amplitude 0.15, 0.3秒）
4. **BPMビート視覚化**: 画面上部のメトロノームインジケーターがBPMに合わせてパルス

## スコアシステム
- コンボ倍率: `multiplier = 1.0 + Mathf.Min(combo * 0.15f, 2.0f)` (Perfect時)
- Good以上で正解率カウント
- ステージ終了時: 全問回答 + 正解率50%以上→クリア、Missが5回到達→ゲームオーバー

## レスポンシブ配置
- Camera.orthographicSize = 6
- コードボタン群: Canvas下部（Y=80〜200）に横並び（2〜4列）
- BPMインジケーター: 画面上部中央
- 判定テキスト: 画面中央
- HUD（スコア、コンボ、ミス数）: 画面上部

## 判断ポイントの実装設計
- **リプレイ消費判断**: `_replayCount > 0` の時のみリプレイボタン有効化。残りリプレイ数を常時表示
- **タイミング精度トレードオフ**: 正確に聴き取るほど時間がかかるが、タイミングが遅れるとGreat/Goodに落ちる
- **ミス許容**: 累計5ミスでゲームオーバー。ミスカウンターを常時表示してプレッシャーを演出
