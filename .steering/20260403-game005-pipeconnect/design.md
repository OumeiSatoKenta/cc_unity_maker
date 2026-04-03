# Design: Game005v2 PipeConnect (Remake)

## namespace
`Game005v2_PipeConnect`

## スクリプト構成

### PipeConnectGameManager.cs
- ゲーム状態管理 (WaitingInstruction / Playing / StageClear / Clear / GameOver)
- StageManager・InstructionPanel統合
- スコア・コンボ・タイマー管理
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] PipeManager _pipeManager
- [SerializeField] PipeConnectUI _ui
- Start(): InstructionPanel.Show → OnDismissed += StartGame
- OnStageChanged(int): PipeManager.SetupStage(config, stageIndex) 呼び出し
- Update(): タイマーカウントダウン (水流開始後のみ)

### PipeManager.cs
- グリッド管理、パイプセル生成・配置・回転処理
- 入力処理一元管理 (Mouse.current + Physics2D.OverlapPoint)
- 水流シミュレーション (BFS/DFS で経路探索)
- パイプ種別: Straight(直線), Elbow(L字), TJunction(T字), Locked(ロック), Valve(バルブ)
- SetupStage(StageManager.StageConfig config, int stageIndex): ステージ別レイアウト生成
- StartWaterFlow(): 水源から水流BFS開始
- ResetPipes(): 全パイプ初期角度に戻す
- レスポンシブ配置: Camera.main.orthographicSize から動的計算
- イベント: OnFlowComplete(bool allConnected, int pathLength)
- イベント: OnPipeTapped()

### PipeCell.cs
- 個々のパイプセルコンポーネント
- PipeType enum: Empty, Straight, Elbow, TJunction, Source, Exit, Locked, Valve
- rotationIndex (0-3, 90度単位)
- isLocked (ロックパイプ)
- valveOpen (バルブ開閉状態)
- GetConnections(): 現在の向きで接続できる方向 (上下左右のbool[4])
- Rotate(): 90度時計回り回転、ロック中は無効
- ToggleValve(): バルブ開閉切替
- ビジュアルフィードバック: 回転アニメーション (DOTween代替のコルーチン使用)

### PipeConnectUI.cs
- スコア・ステージ・タイマー表示
- ステージクリアパネル・ゲームオーバーパネル
- UpdateTimer(float) / UpdateScore(int) / UpdateStage(int, int)
- ShowStageClearPanel(int stage, int score, int stars)
- ShowGameOverPanel()
- ShowClearPanel(int score)

## パイプ接続方向の定義
```
方向インデックス: 0=上, 1=右, 2=下, 3=左

Straight (回転0): 上↔下
Elbow    (回転0): 上+右
TJunction(回転0): 上+右+下
Source   : 中央から全方向（固定）
Exit     : 中央から全方向（固定、到達判定用）
Locked   : 任意向きで固定
Valve    : 閉=接続なし、開=Straightと同じ
```

## ステージ別レイアウト設計

### Stage 1 (4×4, 90秒)
- 水源: (0,2) 右向き、出口: (3,2)
- 直線・L字のみ、1本の明確な経路

### Stage 2 (5×5, 80秒)
- 水源: (0,2)、出口: (4,2)
- T字パイプ3個追加、少し迂回する経路

### Stage 3 (5×5, 70秒)
- 水源: (0,2)、出口1: (4,1)、出口2: (4,3)
- 経路分岐必須、T字パイプ複数

### Stage 4 (6×6, 60秒)
- 水源: (0,3)、出口: (5,3)
- ロックパイプ3個（固定の障害物として機能）

### Stage 5 (6×6, 50秒)
- 水源: (0,3)、出口1: (5,2)、出口2: (5,4)
- バルブパイプ2個（初期閉状態）、タップで開閉

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;    // HUD領域
float bottomMargin = 3.0f; // UIボタン領域
float availableHeight = camSize * 2f - topMargin - bottomMargin;
float cellSize = Mathf.Min(availableHeight / gridSize, camWidth * 2f / gridSize, 1.4f);
```

## SceneSetup: Setup005v2_PipeConnect.cs
- MenuItem: "Assets/Setup/005v2 PipeConnect"
- スプライト: Pillow生成 → File.WriteAllBytes → AssetDatabase.ImportAsset
- EventSystem: InputSystemUIInputModule
- InstructionPanel: フルスクリーンオーバーレイ
- StageManager: GameManagerの子オブジェクト
- PipeManager: GameManagerの子オブジェクト
- PipeCellPrefab: BoxCollider2D + SpriteRenderer + PipeCell
- Build Settings 追加

## ビジュアルフィードバック
1. **パイプ回転時**: transform.eulerAngles をコルーチンで滑らかに回転 (0.15秒)
2. **水流エフェクト**: 水の通ったパイプをシアン色にフラッシュ (順次点灯)
3. **クリア時**: カメラシェイク + 全パイプ緑色に変化
4. **ゲームオーバー**: 未接続パイプを赤色フラッシュ
5. **バルブ開閉**: スケールパルス (1.0→1.3→1.0、0.2秒)

## スコアシステム
- 基本: 残り時間 × 10 + 経路マス数 × 50
- パーフェクト（全マス通過）: ×2.0
- 全出口到達（Stage3以降）: +500ボーナス
- コンボ: ステージ連続クリアで乗算（2連続: ×1.5、3連続: ×2.0、4+: ×3.0）

## InstructionPanel内容
```
gameId: "005"
title: "PipeConnect"
description: "パイプを回転させて水源から出口まで繋ぐパズル"
controls: "タップでパイプを90度回転・水流ボタンで確認"
goal: "制限時間内に水を出口まで届けよう"
```

## ステージ別新ルール表
- Stage 1: 基本（直線・L字）。経路は1本道。チュートリアル的
- Stage 2: T字パイプ追加。分岐判断が発生する
- Stage 3: 出口2箇所。T字で経路を分岐して全出口に届ける必要がある
- Stage 4: ロックパイプ（回転不可）。固定パイプを軸に周囲を調整する
- Stage 5: バルブパイプ（初期閉）追加。タップで開けないと水が通らない

## 判断ポイントの実装設計
- 水流ボタン押下時に経路チェック → 未接続の場合は赤フラッシュでフィードバック
- ロックパイプ: PipeCell.isLocked=true の場合、タップ時に回転せずにシェイクアニメーション
- バルブ: 閉状態では水が通らない (GetConnections() が空を返す)
