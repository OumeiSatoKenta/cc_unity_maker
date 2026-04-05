# Design: Game039v2_BoomerangHero

## namespace
`Game039v2_BoomerangHero`

## スクリプト構成

### BoomerangHeroGameManager.cs
- ゲーム状態管理（WaitingInstruction / Playing / StageClear / Clear / GameOver）
- スコア・残弾数・残敵数の管理
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] BoomerangMechanic _mechanic
- [SerializeField] BoomerangHeroUI _ui
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager購読 → StartFromBeginning()
- OnStageChanged(int): 盤面再構築
- OnAllStagesCleared(): 最終クリア

### BoomerangMechanic.cs
コアメカニクスの実装
- ブーメランの発射・飛行・反射・ヒット判定
- 入力: Mouse.current でドラッグ開始/中/終了を検出
- 発射方向・力のプレビュー（LineRenderer）
- 壁反射計算（Vector2.Reflect）
- 敵へのヒット判定（Physics2D.OverlapCircle）
- **5ステージ対応**: SetupStage(StageManager.StageConfig config)
- **レスポンシブ配置**: Camera.main.orthographicSize から動的計算
- 入力一元管理（Update()で全入力処理）

### BoomerangHeroUI.cs
- ステージ表示「Stage X / 5」
- 残弾数、残敵数表示
- スコア表示
- ステージクリアパネル
- 最終クリアパネル
- ゲームオーバーパネル

## 盤面・ステージデータ設計

```csharp
struct StageData {
    int enemyCount;
    bool hasShieldedEnemy;
    bool hasMovingEnemy;
    int ammoCount;
    Vector2[] enemyPositions;
    Vector2[] wallPositions;
    Vector2[] wallScales;
    float wallRotations[];
}
```

ステージデータはBoomerangMechanic内で静的配列として定義。

## 入力処理フロー

```
Update():
  if Mouse.leftButton.wasPressedThisFrame → StartAim(mousePos)
  if IsAiming && Mouse.leftButton.isPressed → UpdateAim(mousePos)
  if Mouse.leftButton.wasReleasedThisFrame && IsAiming → LaunchBoomerang(direction, force)
```

## ブーメラン飛行ロジック
- Rigidbody2D + AddForce で初速を与える
- FixedUpdate()でコリジョン判定
- 壁との反射: ContactPoint → Vector2.Reflect(velocity, normal)
- ブーメランのスプライトは飛行方向に回転

## 敵の種類
- Normal Enemy: 通常の敵、任意方向から当たれば撃破
- Shielded Enemy (Stage 4+): 正面に盾、裏側からのみ当たる
- Moving Enemy (Stage 5+): 一定速度で往復移動

## SceneSetup構成方針
- MenuItem: "Assets/Setup/039v2 BoomerangHero"
- SceneSetup039v2_BoomerangHero.cs

## StageManager統合
- OnStageChanged購読 → BoomerangMechanic.SetupStage()呼び出し
- OnAllStagesCleared購読 → 最終クリア処理

ステージ別パラメータ:
| Stage | speedMultiplier | countMultiplier | complexityFactor |
|-------|-----------------|-----------------|------------------|
| 0(1)  | 1.0 | 1.0 | 0.0 |
| 1(2)  | 1.0 | 1.0 | 0.3 |
| 2(3)  | 1.1 | 1.2 | 0.6 |
| 3(4)  | 1.2 | 1.5 | 0.8 |
| 4(5)  | 1.3 | 1.8 | 1.0 |

## InstructionPanel内容
- title: "BoomerangHero"
- description: "ブーメランを壁で反射させて敵を倒そう"
- controls: "ドラッグで角度と力を調整、リリースで発射"
- goal: "限られた弾数で全ての敵を倒そう"

## ビジュアルフィードバック設計
1. **敵撃破時**: スケールパルス（1.0 → 1.5 → 0.0、0.3秒でフェードアウト）+ 色フラッシュ（白→消滅）
2. **ブーメラン発射時**: 回転アニメーション（毎フレーム回転角度加算）
3. **壁反射時**: 短い閃光エフェクト（SpriteRenderer色変化 → 0.1秒後元に戻す）
4. **盾ブロック時**: 赤フラッシュ + はじき飛びアニメーション

## スコアシステム
- 直接ヒット: 10pt
- 壁1回反射後ヒット: 30pt
- 壁2回反射後ヒット: 60pt
- 1投で複数ヒット: 最後のヒット数 × 20pt 追加ボーナス
- 残弾ボーナス: 残弾数 × 50pt（ステージクリア時）

## ステージ別新ルール表
- Stage 1: 基本ルールのみ（直接投げ）。敵2体、壁なし
- Stage 2: L字型の壁が1つ追加。反射が必要な配置
- Stage 3: 複数の壁。2回反射が必要な敵配置
- Stage 4: 盾持ち敵の追加（正面無効、裏側のみ有効）
- Stage 5: 移動敵の追加（タイミングを合わせた投げが必要）

## 判断ポイントの実装設計
- **トリガー**: ドラッグ中に軌道プレビュー（LineRenderer）が表示される
- **選択**: どの角度・力で投げるかを1-3秒で判断
- **リスク/リワード**: 直接投げ（10pt、確実）vs 反射投げ（30-60pt、難しい）
- **弾数制限**: Stage 1-3は3発、Stage 4-5は4発

## レスポンシブ配置設計
```csharp
float camSize = Camera.main.orthographicSize;  // 5.0
float camWidth = camSize * Camera.main.aspect; // ~3.0 (9:16)
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 2.8f; // Canvas UI領域
float availableH = camSize * 2f - topMargin - bottomMargin; // ~6.0
```

プレイヤー: 左寄り (-camWidth * 0.4f, 0)
敵の配置: ステージデータの相対座標にcamSizeを掛けたワールド座標
壁の配置: ステージデータの相対座標にcamSizeを掛けたワールド座標
