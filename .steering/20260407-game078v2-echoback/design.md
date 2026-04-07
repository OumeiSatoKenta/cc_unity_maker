# Design: Game078v2 EchoBack

## namespace
`Game078v2_EchoBack`

## スクリプト構成

### EchoBackGameManager.cs
- StageManager・InstructionPanel統合
- ゲーム状態管理（Listening / Inputting / StageClear / AllClear / GameOver）
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] EchoManager _echoManager`
- `[SerializeField] EchoBackUI _ui`
- Start()でInstructionPanel.Show()、OnDismissed += StartGame
- StartGame()でStageManager.StartFromBeginning()
- OnStageChanged(int stage)でEchoManager.SetupStage(config, stage)
- OnAllStagesCleared()で最終クリア

### EchoManager.cs
- コアメカニクス担当
- 鍵盤ゲームオブジェクト参照（4〜7個、ステージ依存）
- お手本パターン生成・再生（AudioClip不使用、AudioSource.PlayOneShot + 周波数別Clip生成）
- 入力受付・判定（Mouse.current / Touch）
- SetupStage(StageManager.StageConfig config, int stageIndex)でステージ別パラメータ適用
- 状態: Listening / Inputting / Completed / Failed
- Miss カウント管理（3回でGameOver）
- コンボ・スコア計算
- リプレイ管理（残り回数）
- Perfect判定: ±50ms、Great: ±120ms、Good: ±250ms

### EchoBackUI.cs
- スコア・コンボ・ステージ・判定テキスト・フェーズ表示
- ステージクリアパネル・全クリアパネル・ゲームオーバーパネル
- パターン進捗ドット表示
- リプレイ残り回数表示

## StageManager統合
- OnStageChanged購読でEchoManager.SetupStage()呼び出し
- StageConfig活用:
  - speedMultiplier → BPM係数（Stage1: 1.0, Stage2: 1.29, Stage3: 1.57, Stage4: 1.86, Stage5: 2.14）
  - countMultiplier → パターン長係数（Stage1: 3, Stage2: 5, Stage3: 6, Stage4: 8, Stage5: 10）
  - complexityFactor → 追加要素フラグ（0.0=単音のみ, 0.25=休符, 0.5=和音, 0.75=逆再生, 1.0=テンポ変化）

## InstructionPanel内容
- title: "EchoBack"
- description: "鳴り響くメロディを記憶して、同じパターンを鍵盤で再現する音楽記憶ゲーム"
- controls: "メロディを聴いて鍵盤をタップ！同じ音を同じリズムで入力しよう。リプレイボタンでもう一度聴ける"
- goal: "5ステージのパターンを完璧に再現してマスターエコーを目指せ！"

## ビジュアルフィードバック設計
1. **鍵盤ヒット演出**: キータップ時に `transform.localScale` 1.0→1.2→1.0（0.15秒）+ 色フラッシュ
2. **判定テキストアニメ**: Perfect/Great/Good/Miss テキストが上方向にフェードアウト（0.8秒）
3. **コンボ演出**: コンボ数字がスケールパルス（combo>=10でゴールド色）
4. **パーフェクトボーナス**: 鍵盤全体が虹色に光る（0.5秒）

## スコアシステム
- Perfect: 120pt × (1.0 + combo × 0.12)、最大 × 3.0
- Great: 70pt × (1.0 + combo × 0.06)、最大 × 2.0
- Good: 25pt
- Miss: 0pt、コンボリセット
- パターン全音Perfect: × 1.5 ボーナス乗算

## ステージ別パラメータ
| Stage | BPM | 鍵盤数 | リプレイ | パターン長 | 特殊ルール |
|-------|-----|--------|---------|-----------|---------|
| 1 | 70 | 4 | 無制限(-1) | 3〜4音 | なし |
| 2 | 90 | 5 | 3回 | 4〜6音 | 休符あり |
| 3 | 110 | 7 | 2回 | 5〜8音 | 和音あり |
| 4 | 130 | 7 | 1回 | 6〜10音 | 逆再生 |
| 5 | 150 | 7 | 0回 | 8〜12音 | 1回のみ+テンポ変化 |

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
// 鍵盤エリア: 下部 bottomMargin=3.0 確保
// 鍵盤配置: y = -camSize + 2.5f (鍵盤中心)
// 鍵盤横幅: camWidth * 1.8f / keyCount
float keyAreaY = -camSize + 2.2f;
float keyWidth = Mathf.Min(camWidth * 1.8f / keyCount, 1.4f);
```

## SceneSetup構成方針
- MenuItem: `"Assets/Setup/078v2 EchoBack"`
- カメラ背景色: 暗紫色 (0.06, 0.02, 0.12)
- キャンバス上: HUD（スコア・コンボ・ステージ・フェーズ・リプレイ回数）
- キャンバス下: ゲーム領域（鍵盤ボタン群、リプレイボタン）
- 鍵盤はワールド空間ではなくCanvasUI上に配置（ボタンUI）
- StageManager子オブジェクトとしてGameManagerに配置
- InstructionPanel生成（フルスクリーンオーバーレイ）
- ステージクリアパネル生成

## 配線フィールド一覧
EchoBackGameManager:
- _stageManager → StageManager
- _instructionPanel → InstructionPanel
- _echoManager → EchoManager
- _ui → EchoBackUI

EchoManager:
- _gameManager → EchoBackGameManager
- _ui → EchoBackUI
- _keyObjects[] → Key Button transforms (4〜7個)

EchoBackUI:
- _scoreText, _comboText, _stageText, _phaseText, _judgementText
- _replayCountText
- _progressDots[]
- _stageClearPanel, _allClearPanel, _gameOverPanel
- _nextStageButton, _retryButton, _menuButton

## AudioClip生成（ProceduralAudio）
AudioClipを生成せず、AudioSource.pitch を変えて単一サイン波Clipを再生する。
鍵盤周波数比（ドレミファソラシド）: 1.0, 1.122, 1.26, 1.335, 1.498, 1.682, 1.888, 2.0

## 判断ポイントの実装設計
- 入力受付タイムウィンドウ: ±250ms（Miss閾値）
- 正解音の期待時刻: `patternStartTime + noteIndex * (60f / bpm)`
- タップ時刻と期待時刻の差分で判定
- 逆再生: pattern配列をReverse()したものを期待順序とする
