# Design: Game027v2_DotDodge

## namespace
`Game027v2_DotDodge`

## スクリプト構成

### DotDodgeGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- InstructionPanel.Show() → StartGame()
- StageManager.OnStageChanged / OnAllStagesCleared 購読
- スコア・コンボ管理
- ニアミス検出（PlayerController経由）
- GameOver / Clear の状態遷移

### PlayerController.cs
- ドラッグ入力処理（Mouse.current）
- 自キャラ位置をマウス/タッチ位置に追従
- ニアミス検出（ドットとの距離がniarmiss閾値以内）
- カメラ境界内クランプ
- `_isActive` ガード

### DotSpawner.cs
- ドットの動的生成・管理
- ドット種別: Normal（直線）/ Chaser（追尾）/ Expander（拡大）/ Safe（未使用）
- SetupStage(StageConfig): ステージ別パラメータ適用
- 画面外からランダムなベクトルでスポーン
- 画面外に出たドットは削除
- `_isActive` ガード

### DotDodgeUI.cs
- 生存時間・スコア・コンボ・ステージ表示
- StageClearPanel / GameClearPanel / GameOverPanel の表示制御
- Initialize(DotDodgeGameManager)

## ステージ別パラメータ表

| Stage | speedMultiplier | countMultiplier | complexityFactor | 時間(秒) |
|-------|----------------|-----------------|-----------------|---------|
| 1 | 1.0 | 1 | 0.0 | 15 |
| 2 | 1.2 | 2 | 0.2 | 20 |
| 3 | 1.4 | 2 | 0.4 | 25 |
| 4 | 1.7 | 3 | 0.6 | 30 |
| 5 | 2.0 | 3 | 1.0 | 35 |

speedMultiplier: ベース速度2.5 × multiplier
countMultiplier: ベース同時存在数8 × multiplier
complexityFactor: 0=Normal only, 0.2=Chaser追加, 0.4=Expander追加, 0.6=SafeZone追加, 1.0=全要素+画面シェイク

## InstructionPanel内容
- gameId: "027v2"
- title: "DotDodge"
- description: "画面を埋め尽くすドットを避け続けるサバイバル！"
- controls: "画面をドラッグして青いプレイヤーを操作。赤いドットに当たるとゲームオーバー！"
- goal: "全5ステージを生き延びてクリアを目指せ！ニアミスでボーナスポイントも狙え！"

## ビジュアルフィードバック

1. **ニアミス時**: 自キャラが黄色にフラッシュ（SpriteRenderer.color、0.15秒）+ スケールパルス(1.0→1.4→1.0)
2. **ゲームオーバー時**: カメラシェイク（0.4秒）+ 自キャラが赤に変色
3. **コンボ更新時**: UIテキストのスケールアニメーション

## スコアシステム
- 生存スコア: 0.1秒ごとに `10 × stageNum` pt
- ニアミスボーナス: +30pt × comboMultiplier
- コンボ倍率: 3秒以内に再ニアミスで x1.5→x2.0→x3.0（5秒無ニアミスでリセット）
- ステージクリアボーナス: +500 × stageNum

## ステージ別新ルール
- Stage 1: Normalドット（直線移動）のみ
- Stage 2: Chaserドット出現（プレイヤーを20%の確率で追尾）
- Stage 3: Expanderドット出現（3秒ごとに半径1.2倍拡大、最大2倍）
- Stage 4: SafeZone出現（5秒ごとに白円が1つ出現、3秒後消滅）
- Stage 5: 画面シェイク（カメラ位置をノイズで揺らす）+ 全ドット種別混在

## 判断ポイントの実装設計
- ニアミス判定: プレイヤーとドットの距離 < ドット半径 × 1.8 かつ > ドット半径 × 1.0
- SafeZone: 半径1.2uの白い半透明円。ドットはSafeZone内に入ったら方向反転
- チェイサー補正: 毎フレーム自キャラ方向に5%だけ進行ベクトルを傾ける

## SceneSetup構成
- MenuItem: "Assets/Setup/027v2 DotDodge"
- カメラ: orthographicSize=5, 背景色=濃い紺
- Canvas: Screen Space Overlay, InputSystemUIInputModule
- HUD: 上部（Score左, Time中央, Stage右）
- SafeZoneObj: SpriteRenderer（白い円スプライト）
- GameManager → [SerializeField] DotSpawner, UI, StageManager, InstructionPanel
- PlayerController: [SerializeField] mainCamera

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5
float camWidth = camSize * Camera.main.aspect; // ~8.9
// ゲーム領域: 全画面 (ドットは全域を使用)
// プレイヤーはカメラ境界内でクランプ: ±camWidth, -camSize+0.5 ~ camSize-1.0
```

## アセット一覧
- Background.png: 256x256, 濃い紺グラデーション
- Player.png: 128x128, 青い円（グロウ効果）
- DotNormal.png: 64x64, 赤い円（グラデーション）
- DotChaser.png: 64x64, オレンジ赤の円（目付き）
- DotExpander.png: 64x64, 紫がかった赤円（外周脈動感）
- SafeZone.png: 128x128, 白い半透明円（軟らかなグロウ）
