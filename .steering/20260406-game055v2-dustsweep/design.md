# Design: Game055v2_DustSweep

## namespace
`Game055v2_DustSweep`

## スクリプト構成
| クラス | ファイル | 役割 |
|-------|---------|------|
| DustSweepGameManager | DustSweepGameManager.cs | ゲーム状態管理・StageManager統合・InstructionPanel制御 |
| DustBoard | DustBoard.cs | 砂埃ピクセルマップ管理・スワイプ処理・清潔度計算 |
| DustSweepUI | DustSweepUI.cs | HUD表示・タイマー・清潔度バー・ブラシ切替ボタン |

## DustBoard 実装詳細
- Texture2Dベースのピクセルマップ (256x256) でダスト状態を管理
- `bool[,] _dustPixels` で各ピクセルの汚れ状態を追跡
- SpriteRenderer でダストテクスチャを表示
- マウスドラッグ位置をワールド座標→テクスチャ座標変換してクリア
- **頑固な汚れ**: `int[,] _hardness` で各ピクセルの硬さ(0=通常,1=頑固)を管理
- **赤ゾーン**: `bool[,] _dangerZone` で危険エリアを管理。スワイプ時ペナルティ
- **再汚染**: Coroutineで定期的に一部ピクセルを再汚染

## 入力処理フロー
- `Mouse.current.leftButton.isPressed` を毎フレームチェック
- `Mouse.current.position.ReadValue()` でマウス位置取得
- スクリーン座標→ワールド座標→テクスチャUV変換
- ブラシ半径内のピクセルをクリア（円形ブラシ）

## StageManager統合
```
OnStageChanged(int stage):
  - _dustBoard.SetupStage(GetCurrentStageConfig())
  - StageManager.StageConfig で stageIndex, speedMultiplier, countMultiplier, complexityFactor を利用
```

StageManagerのStageConfig活用:
- speedMultiplier → 再汚染速度に使用
- countMultiplier → 汚れ面積比率に使用  
- complexityFactor → 頑固汚れ比率・赤ゾーン面積に使用

## InstructionPanel内容
```csharp
_instructionPanel.Show(
    "055",
    "DustSweep",
    "砂埃をスワイプで拭き取るクリーニングゲーム",
    "画面をドラッグして砂埃を除去しよう。ブラシサイズも切り替えられる！",
    "清潔度100%を達成してステージクリア！"
);
```

## ビジュアルフィードバック設計
1. **アイテム発見時**: スケールパルスアニメーション (1.0 → 1.5 → 1.0、0.3秒) + 黄色フラッシュ
2. **赤ゾーン触れ時**: カメラシェイク + テキストの赤色フラッシュ「-5秒！」
3. **清潔度100%達成**: 白フラッシュのフルスクリーンエフェクト
4. **コンボ継続**: スコアテキストの虹色グラデーション変化

## スコアシステム
```
最終スコア = (残り時間 × 50 + アイテムボーナス) × コンプリート倍率 × 効率倍率
アイテムボーナス = 発見数 × 200
コンプリート倍率 = 全アイテム発見時 2.0倍
効率倍率 = 1.0 〜 1.5 (スワイプ総距離に反比例)
コンボ = 赤ゾーン非接触でのクリーンスワイプ3回以上で追加ボーナス
```

## ステージ別パラメータ表
| Stage | dustArea | timeLimit | hasStubborn | hasRedZone | hasRespawn | itemCount |
|-------|---------|-----------|-------------|------------|------------|-----------|
| 1 | 30% | 60秒 | false | false | false | 1 |
| 2 | 50% | 50秒 | true | false | false | 2 |
| 3 | 55% | 50秒 | true | true | false | 2 |
| 4 | 70% | 60秒 | false | false | true | 3 |
| 5 | 75% | 70秒 | true | true | true | 5 |

## SceneSetup構成
```
GameManager (DustSweepGameManager)
  └ StageManager
Canvas
  ├ InstructionPanel (フルスクリーンオーバーレイ)
  ├ HUD
  │   ├ StageText ("Stage 1 / 5")
  │   ├ ScoreText ("Score: 0")
  │   ├ TimerText ("60")
  │   └ CleanlinessSlider (0-100%)
  ├ BrushPanel
  │   └ BrushButton (Small/Medium/Large toggle)
  ├ StageClearPanel
  │   ├ ClearText ("ステージクリア！")
  │   ├ ScoreResultText
  │   └ NextButton
  ├ GameClearPanel
  │   ├ GameClearText ("ゲームクリア！")
  │   └ RetryButton
  └ GameOverPanel
      ├ GameOverText ("タイムアップ！")
      └ RetryButton
DustBoard (ゲームエリア)
BackButton
```

## 判断ポイントの実装設計
- 再汚染エリア: 10秒ごとにランダムピクセル再汚染 (Stageが高いほど速度UP)
- 赤ゾーン接触: `_dangerZone[px, py]` チェックでペナルティ発動 → タイム-5秒、コンボリセット
- アイテム発見: ダストクリア時に下のアイテムを検出 (隠れアイテムのUV座標と突合)

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5.5
float camWidth = camSize * Camera.main.aspect;
// DustBoardは中央に配置
// 上部1.2u: HUD
// 下部2.8u: UIボタン
// 中央の利用可能エリアに DustBoard を配置
float boardSize = Mathf.Min(camWidth * 2f - 0.4f, camSize * 2f - 4.0f);
```
