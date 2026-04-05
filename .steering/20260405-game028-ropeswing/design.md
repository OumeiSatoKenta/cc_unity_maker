# Design: Game028v2_RopeSwing

## namespace
`Game028v2_RopeSwing`

## スクリプト構成

| ファイル | 担当 |
|---------|------|
| `RopeSwingGameManager.cs` | ゲーム状態管理、StageManager/InstructionPanel統合、スコア管理 |
| `RopeController.cs` | 振り子物理シミュレーション（ロープ掴み/離し、飛行、着地判定） |
| `PlatformManager.cs` | 足場生成・管理（固定/移動/崩壊タイプ対応、5ステージ設定） |
| `RopeSwingUI.cs` | スコア・ステージ・コンボ表示、各種パネル |

## ゲーム設計

### 物理シミュレーション（Physics使わずコード実装）
- RopeController がプレイヤーの振り子運動を手計算で実装
- ロープ掴み中: 角速度を積分してプレイヤー位置を更新（`angle += angularVelocity * dt`）
- 飛行中: 重力加速度を適用した放物線移動
- 着地判定: プレイヤーの矩形と足場の矩形の重なりチェック

### ステージ設計
各ステージでPlatformManagerとRopeControllerに`SetupStage(config)`を呼ぶ

#### StageConfig パラメータ対応
- `speedMultiplier`: 移動足場の移動速度倍率
- `countMultiplier`: 足場数（3, 5, 7, 9, 12）
- `complexityFactor`: 0.0〜1.0で新要素の有効化率（0.3 = 移動足場30%, 0.4 = 崩壊足場40%等）

### ステージ別パラメータ
| Stage | platformCount | platformWidth | ropeLength | hasMobilePlatform | hasWind | hasCollapsePlatform |
|-------|--------------|---------------|------------|-------------------|---------|---------------------|
| 1 | 3 | 2.0 | 2.5(固定) | No | No | No |
| 2 | 5 | 1.6 | 1.8〜3.0(可変) | No | No | No |
| 3 | 7 | 1.4 | 1.5〜3.0 | Yes(30%) | No | No |
| 4 | 9 | 1.2 | 1.5〜3.0 | Yes(30%) | Yes | No |
| 5 | 12 | 1.0 | 1.5〜3.0 | Yes(40%) | Yes | Yes(40%) |

### レスポンシブ配置
- `camSize = Camera.main.orthographicSize` (= 5.0)
- `camWidth = camSize * aspect`
- 足場はカメラ横幅を均等分割して配置
- 上部: HUDテキスト領域 (y = camSize - 0.8)
- 下部: UIボタン領域 (bottomMargin = 2.5u)
- ゲーム領域: -camSize+2.5 〜 camSize-1.2
- プレイヤー初期位置: 左端の最初の足場上

### InstructionPanel
- title: "RopeSwing"
- description: "ロープを掴んで振り子で飛び移るアクションゲーム！"
- controls: "画面をタップしてロープを掴み、離して足場に飛び移ろう"
- goal: "全ての足場を渡りきって、ゴールに着地しよう！"

### ビジュアルフィードバック
1. **着地成功**: プレイヤーのスケールパルス（1.0→1.3→1.0、0.2秒）+ 着地評価テキストポップアップ
2. **ミス/落下**: 赤フラッシュ + カメラシェイク（0.4秒）
3. **コンボ更新**: 黄色テキスト「Combo x3!」の一瞬表示
4. **崩壊足場**: 足場が点滅して赤くなってから消える演出

### スコアシステム
- 着地精度:
  - Perfect（中央50%以内）: 200pt × コンボ倍率
  - Good（中央70%以内）: 100pt × コンボ倍率
  - OK（端）: 50pt × コンボ倍率
- ステージクリアボーナス: 500 × ステージ番号
- コンボ倍率: x1.0 / x1.5 / x2.0 / x3.0（連続Good以上で上昇）

## SceneSetup 構成方針

`Setup028v2_RopeSwing.cs`（`Assets/Editor/SceneSetup/`）

- MenuItem: `"Assets/Setup/028v2 RopeSwing"`
- カメラ: orthographic, size=5, 背景色 (0.07, 0.12, 0.22) 深みのある夜空色
- スプライト自動生成（Python Pillow, actionカテゴリカラー）

### 配線が必要なフィールド
**RopeSwingGameManager**:
- `_stageManager`: StageManager
- `_instructionPanel`: InstructionPanel
- `_ropeController`: RopeController
- `_platformManager`: PlatformManager
- `_ui`: RopeSwingUI

**RopeController**:
- `_gameManager`: RopeSwingGameManager
- `_platformManager`: PlatformManager
- `_playerSprite`: SpriteRenderer（Player）

**PlatformManager**:
- `_gameManager`: RopeSwingGameManager
- `_platformSprite`: Sprite（Platform）
- `_goalSprite`: Sprite（Goal）
- `_ropeSprite`: Sprite（Rope/Anchor）

**RopeSwingUI**:
- `_gameManager`: RopeSwingGameManager
- 各TextMeshProUGUI

### UI配置（Canvas/1080x1920基準）
- StageText: 左上 (anchor:0,1) offset:(15,-35)
- ScoreText: 右上 (anchor:1,1) offset:(-15,-35)
- TimerText: 上中央 (anchor:0.5,1) offset:(0,-35)
- ComboText: 中央上 offset:(0, 200) 非表示から表示
- MenuButton: 左下 (anchor:0,0) offset:(10,10) size:(160,55)
- LandingFeedbackText: 中央 表示後フェードアウト
- StageClearPanel: 中央 非表示
- GameOverPanel: 中央 非表示
- FinalClearPanel: 中央 非表示
