# Design: Game042v2 ColorDrop

## namespace
`Game042v2_ColorDrop`

## スクリプト構成

### ColorDropGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- SerializeField: `_stageManager`, `_instructionPanel`, `_mechanic`, `_ui`
- Start()でInstructionPanel.Show() → OnDismissed → StartGame()
- StartGame()でStageManager購読 + StartFromBeginning()
- OnStageChanged(int stage)でmechanic.SetupStage()を呼ぶ
- スコア管理、ライフ管理

### ColorDropMechanic.cs
- コアメカニクス（ドロップ生成・落下・バケツ判定）
- 入力処理: スワイプ（マウス/タッチ）の方向を検知してバケツに振り分け
- ドロップ種別: Normal(Red/Blue/Green)、Rainbow、Bomb
- バケツ: 2〜3個（ステージによって可変）、画面下部に配置
- 5ステージ対応: SetupStage(int stageIndex)
- ビジュアルフィードバック:
  1. 正解時: ドロップがバケツに飛び込む演出 + バケツスケールパルス
  2. 失敗時: 赤フラッシュ + カメラシェイク
  3. コンボ時: 中央にコンボテキスト表示
  4. バケツシャッフル時: ドロップ+バウンスアニメーション
- 入力: Mouse.current.leftButton（スワイプ方向検知）
- レスポンシブ配置: Camera.main.orthographicSizeから動的計算

### ColorDropUI.cs
- スコア・ライフ・コンボ・進捗・ステージ表示
- ステージクリアパネル・最終クリアパネル・ゲームオーバーパネル

## ステージパラメータ表
| Stage | 目標数 | 色数 | 落下速度 | 虹ドロップ | 爆弾ドロップ | バケツシャッフル |
|-------|-------|------|---------|----------|------------|--------------|
| 1(idx=0) | 20 | 2 | 3.0 | なし | なし | なし |
| 2(idx=1) | 30 | 3 | 4.0 | なし | なし | なし |
| 3(idx=2) | 40 | 3 | 4.5 | あり(15%) | なし | なし |
| 4(idx=3) | 50 | 3 | 5.5 | あり(15%) | あり(10%) | なし |
| 5(idx=4) | 60 | 3 | 6.5 | あり(20%) | あり(15%) | あり(8秒毎) |

## レスポンシブ配置
```
camSize = Camera.main.orthographicSize (= 5)
camWidth = camSize * Camera.main.aspect

バケツY = -camSize + 1.2f (バケツ中心Y)
ドロップ生成Y = camSize - 0.5f (上端)
バケツ幅 = min(1.5f, camWidth * 2 / colorCount * 0.85f)
```

## ドロップ物理
- ドロップ落下: Translateで直線落下（Rigidbody不使用）
- スワイプ検知: mouseDown位置とmouseUp位置の差で方向（左/右）を判定
- バケツ対応: 落下中のドロップに対し、スワイプ方向のバケツに「飛ぶ」アニメーション

## スコアシステム
- 基本: 100点/個
- コンボボーナス: N連続 × 50点
- コンボ倍率: combo≥5→×2, combo≥10→×3, combo≥20→×5
- 虹ドロップ: 500点
- ステージクリアボーナス: 残ライフ × 300点

## SceneSetup (Setup042v2_ColorDrop.cs)
- MenuItem: "Assets/Setup/042v2 ColorDrop"
- カメラ背景: ダークブルー #0A1628
- Canvas + EventSystem(InputSystemUIInputModule)
- StageManager(GameManagerの子)、Mechanic(GameManagerの子)
- InstructionPanel(フルスクリーンオーバーレイ)
- ステージクリアパネル・最終クリアパネル・ゲームオーバーパネル
- スプライトパス: Assets/Resources/Sprites/Game042v2_ColorDrop/

## InstructionPanel内容
- title: "ColorDrop"
- description: "色付きの雫を同じ色のバケツに振り分けよう"
- controls: "左右にスワイプして雫を振り分ける"
- goal: "目標数の雫を正しく振り分けてステージクリア！"

## ビジュアルフィードバック
1. **正解演出**: バケツのスケールパルス(1.0→1.3→1.0、0.2秒) + パーティクル風エフェクト(色付き小円が広がる)
2. **失敗演出**: SpriteRenderer.colorの赤フラッシュ(0.15秒) + カメラシェイク(magnitude=0.1, duration=0.3)
3. **コンボ演出**: コンボテキストが中央に表示(スケールアップ演出)
4. **バケツシャッフル**: バケツが横移動アニメーション

## カラーパレット (casual カテゴリ)
- メイン: #4CAF50 (緑)
- サブ: #FFEB3B (黄)
- アクセント: #E8F5E9 (淡緑)
- 背景: #0A1628 (ダークネイビー)

## ドロップカラー
- Red: #F44336
- Blue: #2196F3  
- Green: #4CAF50
- Rainbow: グラデーション(虹色)
- Bomb: #212121 (黒/ダーク、「💣」アイコン)
