# Design: Game067v2_TapDojo

## namespace
`Game067v2_TapDojo`

## スクリプト構成

### TapDojoGameManager.cs
- StageManager・InstructionPanel統合
- ゲーム状態管理 (Playing / StageClear / AllClear)
- スコア(MP)管理
- 参照取得: `[SerializeField]`
- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`

### DojoManager.cs
- コアメカニクス：タップ処理、コンボ、自動修行、技管理、大会、特訓
- `SetupStage(StageManager.StageConfig config, int stageIndex)` でステージパラメータ適用
- コンボ判定: 0.5秒タイムアウトでリセット
- 自動修行: InvokeRepeating or Timer
- 入力: `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint` / UI Button
- レスポンシブ配置: Camera.main.orthographicSize から動的計算

### TapDojoUI.cs
- HUD更新: MP, 段位, コンボ, 自動速度, ステージ
- StageClearパネル/AllClearパネル/ゲームオーバーパネル表示
- ステージクリアパネル「次のステージへ」ボタン

## 盤面・ステージデータ設計

StageManager.StageConfig:
- speedMultiplier: 自動修行速度倍率 (1x, 2x, 3x, 4x, 5x)
- countMultiplier: タップ基本値倍率 (1x, 2x, 3x, 5x, 8x)
- complexityFactor: 難易度 (0〜1.0), 大会勝率や特訓難易度に影響

ステージ別パラメータ:
| Stage | speedMultiplier | countMultiplier | complexityFactor | MP目標 |
|-------|----------------|----------------|-----------------|--------|
| 0 | 0 (自動なし) | 1.0 | 0.2 | 1,000 |
| 1 | 1.0 | 2.0 | 0.4 | 5,000 |
| 2 | 2.0 | 3.0 | 0.6 | 20,000 |
| 3 | 3.5 | 5.0 | 0.8 | 80,000 |
| 4 | 5.0 | 8.0 | 1.0 | 300,000 |

## 入力処理フロー
1. UIボタン（技習得/大会/特訓）→ DojoManager.BuyTech() / EnterTournament() / StartIntensiveTraining()
2. タップエリアクリック → DojoManager.OnTap() → MPAdd + コンボ更新

## SceneSetup構成方針

`Setup067v2_TapDojo.cs`
- `[MenuItem("Assets/Setup/067v2 TapDojo")]`
- カメラ設定: orthographicSize=6, 茶色系背景
- スプライト: `Resources/Sprites/Game067v2_TapDojo/` から読み込み
- Canvas上:
  - InstructionPanel (fullscreen overlay)
  - HUD (上部): Stage, MP, コンボ, 自動速度
  - DojoPanel (中央): 武道家画像 + タップ領域
  - TechPanel (左下): 技習得ボタン群
  - TournamentPanel (右下): 大会ボタン(Stage3解放)
  - StageClearPanel
  - AllClearPanel
  - MenuButton (右下固定)
- GameManagerオブジェクト → StageManager子オブジェクト

## StageManager統合
- `OnStageChanged(int stage)` → `DojoManager.SetupStage(config, stage)`
- `OnAllStagesCleared()` → `DojoManager.SetActive(false)`, AllClearPanel表示

## InstructionPanel内容
- title: "TapDojo"
- description: "タップで修行して最強の武道家を目指そう"
- controls: "道場をタップして修行、ボタンで技習得・大会参加"
- goal: "段位目標の修行ポイントを達成してステージクリア"

## ビジュアルフィードバック設計
1. **タップ成功**: 武道家スプライトのスケールパルス (1.0→1.15→1.0, 0.1秒)
2. **コンボUP**: コンボテキストの色変化 (白→黄→橙→赤) + スケール拡大
3. **技習得**: スプライトの緑フラッシュ (SpriteRenderer.color 0.3秒)
4. **大会勝利**: カメラシェイク + 金色フラッシュ

## スコアシステム
- 基本: タップで baseMP * comboMultiplier 加算
- コンボ倍率: combo 0-9=x1, 10-29=x2, 30-59=x3, 60+=x5
- 技習得でbaseMPボーナス追加
- 自動修行: autoRate MP/秒 (バックグラウンド加算)

## ステージ別新ルール表
- Stage 1: 手動タップのみ。「正拳突き」技購入可（50MP）
- Stage 2: 自動修行（弟子派遣）解放、「回し蹴り」（200MP）解放
- Stage 3: 大会ボタン解放（500MP消費で対戦、勝率60%で1,000MP報酬）
- Stage 4: 特訓イベントボタン解放（15秒間30タップで奥義「虎砲」習得）
- Stage 5: 師範試験ボタン解放（全技習得済み+5,000MP消費で合格、autoRate x2）

## 判断ポイントの実装設計
- **MP投資 vs 蓄積**: 技コストが現在MPを超える場合、UI上でボタンをグレーアウト + コスト表示
- **大会リスク**: 500MP消費し失敗(40%)なら0返還 → プレイヤーが勝率を意識する設計
- **特訓チャレンジ**: 失敗してもペナルティなし、成功で恒久ボーナス → 気軽に挑戦できるが報酬が大きい
