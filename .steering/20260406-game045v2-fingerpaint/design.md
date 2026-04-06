# Design: Game045v2 FingerPaint

## namespace
`Game045v2_FingerPaint`

## スクリプト構成

### FingerPaintGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] FingerPaintCanvas _canvas
- [SerializeField] FingerPaintUI _ui
- Start(): InstructionPanel.Show() → StartGame()
- StartGame(): _stageManager.StartFromBeginning()
- OnStageChanged(int stage): CanvasをリセットしてSetupStage()
- スコア計算: 一致率×100 + 残インク×50 + 残時間×30 + コンボ×1.5 + パーフェクト2000

### FingerPaintCanvas.cs（コアメカニクス）
- Texture2D ベースの描画システム（512x512 ピクセル）
- SetupStage(StageConfig, int stage): お手本パターン生成・インク初期化
- ドラッグ入力処理: Mouse.current.leftButton / position → ワールド → テクスチャ座標変換
- ダブルタップ検出: 0.3秒以内の2回クリックでオーバーレイ切替
- お手本テクスチャ（Texture2D _templateTexture）: 各ステージのパターンをプロシージャル生成
- キャンバステクスチャ（Texture2D _canvasTexture）: 実際の描画先
- 一致率計算: サンプリングポイントで色比較（1000点サンプル）
- ブラシ描画: 円形ブラシ（太:25px, 細:8px）
- 消しゴム: 透明色で上書き（インク消費×3倍）
- インク管理: 1ピクセル描画ごとにインク消費（太=大、細=小）
- Stage5: 乾くインク（30秒後に描いた部分のアルファを徐々に低下）
- 色正確度コンボ: 正しい色で連続塗り時にコンボカウント増加

ビジュアルフィードバック:
- 色選択時: パレットボタンにスケールパルス（1.0→1.3→1.0, 0.15秒）
- クリア時: キャンバス全体がゴールドでフラッシュ（アルファ0.5→0, 0.5秒）

### FingerPaintUI.cs
- 一致率テキスト（%、リアルタイム更新）
- インク残量スライダー
- タイマーテキスト（秒）
- ステージ表示「Stage X / 5」
- パレットボタン群（色選択）
- ブラシサイズトグル（太/細）
- 消しゴムボタン（Stage4以降）
- ステージクリアパネル（「次のステージへ」ボタン）
- 最終クリアパネル
- ゲームオーバーパネル

## InstructionPanel 内容
- title: "FingerPaint"
- description: "お手本に合わせて指でキャンバスに絵を描こう"
- controls: "ドラッグで描く・パレットで色を選ぶ・ダブルタップでお手本表示切替"
- goal: "制限時間内にお手本との一致率を目標値以上にしてクリア！"

## ステージ別パラメータ
| Stage | speedMultiplier | countMultiplier | complexityFactor | 特記 |
|-------|----------------|-----------------|-----------------|------|
| 1 | 1.0 | 1 | 0.0 | 1色、太ブラシ、60秒、インク100% |
| 2 | 1.0 | 1 | 0.2 | 2色、色ペナルティ、60秒、インク90% |
| 3 | 1.0 | 2 | 0.4 | 3色、細ブラシ解放、55秒、インク85% |
| 4 | 1.0 | 2 | 0.7 | 4色、消しゴム解放、50秒、インク80% |
| 5 | 1.0 | 3 | 1.0 | 5色、乾くインク、45秒、インク75% |

## SceneSetup構成
- MenuItem: "Assets/Setup/045v2 FingerPaint"
- カメラ: orthographic, size=5, 黒背景
- キャンバスオブジェクト: 中央配置のQuad（5x5ワールドユニット）
- GameManager（root）→ StageManager、FingerPaintCanvas（子）
- Canvas（UI）: ScaleWithScreenSize 1080x1920
- StageManager配線: gm._stageManager
- InstructionPanel配線: gm._instructionPanel
- パレットボタン: 下部横並び5個（ステージで表示数変化）
- ブラシサイズトグル: パレット横
- 消しゴムボタン: パレット横（初期非表示）

## レスポンシブ配置
- camSize=5, aspect考慮
- キャンバス: ワールド座標(0,0)、4x4ワールドユニット（上下マージン確保）
- 上部マージン：HUD（一致率・タイマー・ステージ）y=430〜480
- 下部マージン：パレット・ボタン y=20〜80（Canvas座標）
- ゲームキャンバスとUIの重なり防止

## アセット一覧（Pillow生成）
- Background.png: 暗いグレーのキャンバス背景
- Canvas.png: 白いキャンバスエリア（細い枠線付き）
- PaletteFrame.png: パレット背景
- BrushIcon.png: ブラシアイコン
- EraserIcon.png: 消しゴムアイコン
- TemplateOverlay.png: お手本オーバーレイの半透明フレーム
- PerfectEffect.png: クリア演出エフェクト（ゴールドスター）

カラーパレット（casualカテゴリ）: メイン#4CAF50、サブ#FFEB3B、アクセント#E8F5E9
