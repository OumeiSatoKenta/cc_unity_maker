# Design: Game048v2 GlassBall

## namespace
`Game048v2_GlassBall`

## スクリプト構成

### GlassBallGameManager.cs
- ゲーム状態管理（Idle/Playing/StageClear/AllClear/GameOver）
- StageManager・InstructionPanel統合
- スコア・コンボ管理
- RailManager/GlassBallController へのゲームイベント通知
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] RailManager _railManager
- [SerializeField] GlassBallController _ballController
- [SerializeField] GlassBallUI _ui

### RailManager.cs
- マウスドラッグでレールを描画（LineRenderer使用）
- インク管理（最大100%）
- EdgeCollider2D でレール物理を付与
- ダブルクリック or クリアボタンでレールリセット
- 発射ボタン押下時にボールを転がし始める
- 入力: Mouse.current.leftButton + Mouse.current.position.ReadValue()
- using UnityEngine.InputSystem;

### GlassBallController.cs
- Rigidbody2D でリアルな物理転がり
- 衝撃ゲージ管理（0〜100%）
- コイン収集（OnTriggerEnter2D）
- ゴール到達判定（OnTriggerEnter2D）
- 障害物衝突で衝撃値加算（OnCollisionEnter2D）
- 坂道エリア（ImpactZone）での衝撃倍率適用
- 風エリアでの横力付加（ステージ5）
- GetComponentInParent<GlassBallGameManager>() でGM取得

### GlassBallUI.cs
- 衝撃ゲージ表示（Slider 0〜1）
- インク残量表示（Slider 0〜1）
- コイン数表示「N/M」
- ステージ表示「Stage X / 5」
- スコア表示
- ステージクリアパネル、最終クリアパネル、ゲームオーバーパネル

## ステージ別パラメータ

| Stage | speedMultiplier | impactMultiplier | nailCount | hasHammer | hasWind | hasThinIce | coinCount |
|-------|----------------|-----------------|-----------|-----------|---------|-----------|-----------|
| 1 | 1.0 | 1.0 | 0 | false | false | false | 0 |
| 2 | 1.0 | 1.0 | 3 | false | false | false | 3 |
| 3 | 1.2 | 1.3 | 3 | false | false | false | 4 |
| 4 | 1.3 | 1.4 | 4 | true | false | false | 5 |
| 5 | 1.5 | 1.5 | 5 | true | true | true | 6 |

## InstructionPanel 内容
- title: "GlassBall"
- description: "ガラスのボールをゴールまで誘導しよう"
- controls: "ドラッグでレールを描いて「発射」ボタンを押そう / ダブルクリックでレールリセット"
- goal: "衝撃を与えずにガラスボールをゴールまで届けよう"

## ビジュアルフィードバック設計
1. **ひび割れ演出**: 衝撃時にボールのSpriteRenderer色を赤フラッシュ（0.3秒、Color.red→元色）
2. **コイン収集演出**: コインが消えるときにスケールパルス（1.0→1.5→0.0、0.15秒）+ スコアポップアップテキスト
3. **ゴール到達演出**: カメラシェイク（0.2秒）+ ゴールエリアが金色にフラッシュ
4. **衝撃危険演出**: 衝撃80%以上でボールの縁が赤く点滅

## スコアシステム
- クリア基本点: 500
- 衝撃最小ボーナス: (100 - impactPercent) × 30
- 残インクボーナス: inkPercent × 20
- コイン: 1枚200点
- 全コイン取得: ×2倍乗算
- ノーダメージ: +1500

## SceneSetup 構成方針

### Setup048v2_GlassBall.cs
- MenuItem: "Assets/Setup/048v2 GlassBall"
- カメラ設定: orthographicSize=5, bgColor=水色系（casual）
- レスポンシブ配置:
  - topMargin = 1.2f（HUD）
  - bottomMargin = 3.0f（UI buttons）
  - ゲームフィールド: 中央エリア
- ガラスボール: 画面上部中央に配置（y=2.5付近）
- ゴールエリア: 画面下部（y=-2.0付近）
- StageManager子オブジェクト（5ステージ設定）
- InstructionPanel全画面オーバーレイ

### StageManager統合
- OnStageChanged → RailManager.SetupStage() + GlassBallController.SetupStage()
- OnAllStagesCleared → AllClear

### 配線が必要なフィールド
GameManager側:
- _stageManager → StageManager
- _instructionPanel → InstructionPanel
- _railManager → RailManager
- _ballController → GlassBallController
- _ui → GlassBallUI

RailManager側:
- _gameManager → GlassBallGameManager
- _ballController → GlassBallController

GlassBallController側:
- _gameManager → GlassBallGameManager（GetComponentInParent使用）
- _ui → GlassBallUI

GlassBallUI側:
- 各Text, Slider, Panel, Button の参照

## 実装アプローチ
- レール描画: LineRenderer + EdgeCollider2D（GetPositions→ Convert to Vector2[]）
- インク: ドラッグ距離累積で消費（maxInkLength = 30f; 1ユニット = 1/30 消費）
- 物理ボール: Rigidbody2D + CircleCollider2D、gravity=1.0（ゆっくり転がる）
- 障害物生成: SceneSetup or SetupStage()で動的生成せずにPrefab的な固定配置
- **簡略化**: 動的物理レールは複雑なので、LineRendererで視覚表示のみ、ボールはforceで誘導するシミュレーション方式を採用

## 判断ポイントの実装設計
- コインは障害物（釘）の近くに配置 → 衝撃リスクを取るかどうかの選択
- インク残量バー表示 → 残量を見ながらルート設計
- 発射前にレール確認できる（発射ボタン押すまで転がらない）

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5.0
float camWidth = camSize * Camera.main.aspect; // ~2.81
// ゲームフィールド Y範囲: [-1.5, 3.5]（topMargin=1.5, bottomMargin=2.5）
// ボール開始位置: y=3.0
// ゴール位置: y=-1.0
```
