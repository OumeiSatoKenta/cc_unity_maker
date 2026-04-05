# Design: Game021v2_BladeDash

## namespace
`Game021v2_BladeDash`

## スクリプト構成

### BladeDashGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- スコア・コンボ管理
- InstructionPanel初期化
- StageManager統合
- 各コンポーネントの参照（SerializeField）

### BladeRunner.cs（コアメカニクス）
- 3レーン管理（左: x=-1.2, 中: x=0, 右: x=1.2）
- Player位置管理（スワイプ入力によるレーン移動）
- ジャンプ/スライディング状態管理
- 障害物（Blade）スポーン・スクロール管理
- コイン（Coin）スポーン・スクロール管理
- 衝突判定（Physics2D.OverlapBoxまたはマニュアル距離判定）
- ニアミス判定
- SetupStage(StageManager.StageConfig config)でステージパラメータ適用
- カメラサイズから動的にレーン位置・スポーン位置計算

### BladeDashUI.cs
- スコア表示
- コンボ倍率表示
- ステージ「Stage X/5」表示
- ステージクリアパネル
- 最終クリアパネル
- ゲームオーバーパネル
- ニアミスボーナスフラッシュ表示

## 盤面・ステージデータ設計

### レーン配置（動的計算）
```
float camWidth = Camera.main.orthographicSize * Camera.main.aspect;
float laneSpacing = camWidth * 0.4f;
laneX[0] = -laneSpacing;
laneX[1] = 0f;
laneX[2] = laneSpacing;
```

### Player配置
- Y位置: カメラ下端 + 2.5u（UI領域確保のため）
- プレイ領域Y: Player.Y から カメラ上端-1.2u

### スポーン位置
- Y: カメラ上端 + 1.0u（画面外からスクロール）

### ステージパラメータ（StageConfig使用）
```
Stage 1: speedMultiplier=1.0, countMultiplier=1.0, complexityFactor=0
Stage 2: speedMultiplier=1.25, countMultiplier=1.1, complexityFactor=1
Stage 3: speedMultiplier=1.5, countMultiplier=1.2, complexityFactor=2
Stage 4: speedMultiplier=1.75, countMultiplier=1.3, complexityFactor=3
Stage 5: speedMultiplier=2.25, countMultiplier=1.5, complexityFactor=4
```

## Bladeタイプ
- **Normal**: 特定1レーンをブロック
- **Low**: 1レーン、ジャンプで回避（Y位置が低い）
- **High**: 1レーン、スライディングで回避（Y位置が高い）
- **Moving**: 1レーン→2レーンを横移動

complexityFactor:
- 0: Normalのみ
- 1: Normal + Low (20%)
- 2: Normal + Low + High (各20%)
- 3: Normal + Low + High + Moving (Moving 30%)
- 4: 全タイプランダム

## 入力処理フロー
1. `Touchscreen.current.primaryTouch`でスワイプ検出（Mouse fallback）
2. スワイプ開始位置記録（pointerDown）
3. pointerUp時にdelta計算
4. |deltaX| > |deltaY|: 左右移動
5. deltaY > threshold: ジャンプ
6. deltaY < -threshold: スライディング
7. しきい値: 30px以上

## InstructionPanel内容
- title: "BladeDash"
- description: "迫りくる刃を避けながらコインを集めよう"
- controls: "左右スワイプでレーン切替、上スワイプでジャンプ、下スワイプでスライディング"
- goal: "刃を避けてコインを集め、目標スコアに到達しよう"

## ビジュアルフィードバック設計
1. **コイン取得エフェクト**: コインがスケール1.0→1.5→0のアニメーションで消える（Coroutine、0.2秒）
2. **ジャンプ/スライディング時Playerスケール変化**: ジャンプ中は0.8x縮小（身を縮める感）
3. **ニアミスフラッシュ**: 画面端に黄色い発光エフェクト（SpriteRendererのcolorフラッシュ）
4. **ゲームオーバーシェイク**: カメラシェイク0.3秒

## スコアシステム
- コイン: 10pt × コンボ倍率
- ニアミスボーナス: 30pt × コンボ倍率
- コンボ倍率: 1コイン=x1.0, 3コイン=x1.5, 6コイン=x2.0, 10コイン=x3.0

## ステージ別新ルール表
| Stage | 新要素 |
|-------|-------|
| 1 | 基本Normalブレードのみ（チュートリアル的） |
| 2 | Lowブレード追加（上スワイプジャンプが必要） |
| 3 | Highブレード追加（下スワイプスライディングが必要） |
| 4 | Movingブレード追加（横移動する刃、タイミング読みが必要） |
| 5 | 全タイプランダム出現、スピード最大 |

## SceneSetup構成方針
- Setup021v2_BladeDash.cs
- メニュー: `Assets/Setup/021v2 BladeDash`
- カメラ: orthographicSize=5
- スプライトパス: `Assets/Resources/Sprites/Game021v2_BladeDash/`
- GameManager、StageManager(子)、BladeRunner(子)
- Canvas: BladeDashUI、InstructionPanel、StageClearPanel、GameOverPanel
- レーン線はワールド座標（SpriteRenderer）

## 判断ポイントの実装設計
- 刃のスポーン間隔: `baseInterval / speedMultiplier` 秒
- 複数レーンに刃が同時に来る確率: `complexityFactor * 15%`
- ニアミス判定: Player-Blade距離 < 0.35u（接触判定 < 0.25uで差別化）
