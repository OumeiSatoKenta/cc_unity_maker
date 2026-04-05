# Design: Game019v2_PathCut

## namespace
`Game019v2_PathCut`

## スクリプト構成

### PathCutGameManager.cs
- GameState管理 (WaitingInstruction / Playing / StageClear / Clear / GameOver)
- SerializeField: StageManager, InstructionPanel, PathCutManager, PathCutUI
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- OnStageChanged(int stage): PathCutManager.SetupStage(config, stage+1)
- OnStageClear(int cutsUsed, int cutsAllowed): スコア計算・コンボ更新・UI表示
- OnGameOver(): ゲームオーバー処理
- OnNextStage(), OnRetry(), OnReturnToMenu()
- ShowInstructions() (「?」ボタン用)

### PathCutManager.cs
- コアメカニクス管理
- 入力: Mouse.current.leftButton を使ってスワイプ検出（beginPos → endPos）
- ロープはLineRendererで表示、実際の物理はHingeJointのチェーンで実装
- ロープオブジェクト: GameObject配列で管理
- ボールオブジェクト: Rigidbody2D付き CircleCollider2D
- 星オブジェクト: CircleCollider2D (isTrigger)
- バンパー: BoxCollider2D (反射用)
- エアクッション: BoxCollider2D (isTrigger)、ボールに上向き力を加える
- 時限ロープ: countdownTimer でカウントダウン、0になったら自動カット
- SetupStage(StageManager.StageConfig config, int stageNum): ステージ構成
- スワイプ検出: Mouse.current.leftButton.wasPressedThisFrame/wasReleasedThisFrame
- ロープカット判定: スワイプラインとロープセグメントの交差判定
- レスポンシブ配置: Camera.main.orthographicSize から動的計算

### PathCutUI.cs
- 表示要素: ステージ、スコア、残りカット数、星残り
- ShowStageClearPanel(int score, int combo, int stars)
- ShowClearPanel(int totalScore)
- ShowGameOverPanel()
- HideAllPanels()
- UpdateStage(int current, int total)
- UpdateScore(int score)
- UpdateCutCount(int remaining, int max)
- UpdateStarCount(int remaining, int total)

## ステージ別パラメータ表

| Stage | ロープ数 | ボール数 | 星数 | カット上限 | complexityFactor |
|-------|---------|---------|------|-----------|-----------------|
| 1 | 1 | 1 | 1 | 3 | 0 |
| 2 | 2 | 1 | 2 | 4 | 1 (バンパー) |
| 3 | 3 | 1 | 3 | 5 | 2 (エアクッション) |
| 4 | 2 | 2 | 4 | 5 | 3 (ロープ連結) |
| 5 | 3 | 2 | 5 | 6 | 4 (時限ロープ) |

## InstructionPanel内容
- title: "PathCut"
- description: "ロープをカットしてボールを星に当てよう"
- controls: "ロープをスワイプでカット"
- goal: "少ないカット数で全ての星にボールを当てよう"

## ビジュアルフィードバック設計
1. **星取得時**: 星がスケールパルス（1.0→1.5→0.0）してフェードアウト＋黄色パーティクル風
2. **ロープカット時**: カット位置に白いフラッシュエフェクト（0.2秒）
3. **ゲームオーバー時**: カメラシェイク＋ボールが赤くフラッシュ
4. **ステージクリア時**: 画面全体に緑のフラッシュ

## スコアシステム
- 基本スコア = 1000 × ステージ番号
- 1カットで複数星取得ボーナス: 2個同時=×1.5、3個=×2.5
- 残りカット数ボーナス: (maxCuts - usedCuts) × 200
- コンボ乗算: 2連続=×1.2、3連続以上=×1.5

## SceneSetup構成方針
- Setup019v2_PathCut.cs
- [MenuItem("Assets/Setup/019v2 PathCut")]
- 物理重力設定: Physics2D.gravity = new Vector2(0, -9.81f)
- カメラ背景色: 紺色（puzzle系）
- Canvas構成: Stage/Score/CutCount/StarCountテキスト + パネル類
- StageManager子オブジェクト
- PathCutManager子オブジェクト
- 「?」ボタン（右下）

## StageManager統合
- OnStageChanged購読 → PathCutManager.SetupStage(config, stageNum)
- config.speedMultiplier: 現状は使用しない（物理重力で速度固定）
- config.countMultiplier: ロープ・ボール・星の数倍率として使用
- config.complexityFactor: 新要素の種類として使用

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5.0
float camWidth = camSize * Camera.main.aspect;
// ゲーム領域: y [-2.5, 3.5] (下2.5ユニットはUI用)
// ロープ固定点: y = 3.0 (上部)
// 星配置: y = [-1.5, 1.0]
```

## 判断ポイントの実装設計
- **どのロープをカットするか**: 複数ロープが異なる角度・位置にあり、カット順で軌道が変化
- **カット位置**: ロープの上寄りか下寄りかで振り子の長さ・速度が変化
- **時限ロープ**: 残り時間UIで表示、タイミングを合わせて手動カットと組み合わせる
- 報酬: 最少カット達成で×1.5スコアボーナス
- ペナルティ: カット上限超過でゲームオーバー

## バグ防止チェック
- 複数クラスのUpdate(): _isActive ガードで同時実行防止
- 動的生成Texture2D/Sprite: OnDestroy()でクリーンアップ
- スワイプ検出: ゲーム中(Playing状態)のみ有効
- ロープ物理オブジェクト: SetupStage()でDestroyして再生成
