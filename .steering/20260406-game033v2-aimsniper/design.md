# Design: Game033v2 AimSniper

## スクリプト構成

| クラス | ファイル | 担当 |
|-------|---------|-----|
| AimSniperGameManager | AimSniperGameManager.cs | ゲーム状態・スコア・StageManager統合 |
| AimSniperMechanic | AimSniperMechanic.cs | スコープ操作・射撃・ターゲット管理・入力一元管理 |
| TargetController | TargetController.cs | 個別ターゲットの移動・ステート管理 |
| AimSniperUI | AimSniperUI.cs | HUD・パネル表示 |

namespace: `Game033v2_AimSniper`

## 盤面・ステージデータ設計

```csharp
// AimSniperMechanic.SetupStage(StageManager.StageConfig config, int stageIndex) 内で適用
struct StageParams {
    int targetCount;      // config.countMultiplier
    float targetSpeed;    // config.speedMultiplier * baseSpeed (0 = static)
    int bulletCount;      // stageIndex: [5,6,7,7,8]
    bool hasWind;         // config.complexityFactor >= 0.3
    bool hasDistance;     // config.complexityFactor >= 0.5
    bool hasObstacle;     // config.complexityFactor >= 0.7
}
```

StageConfig 値:
```
Stage 1: speed=0.0, count=3,  complexity=0.0  → 静止・弾5
Stage 2: speed=1.0, count=4,  complexity=0.2  → 低速移動・弾6
Stage 3: speed=1.5, count=5,  complexity=0.3  → 移動+風・弾7
Stage 4: speed=1.5, count=6,  complexity=0.5  → 移動+風+距離差・弾7
Stage 5: speed=2.0, count=7,  complexity=0.7  → 全要素+遮蔽物・弾8
```

## 入力処理フロー

AimSniperMechanic が全入力を一元管理:
```
Update():
  if Mouse.current.leftButton.isPressed → スコープ移動（ドラッグ）
  if Mouse.current.leftButton.wasReleasedThisFrame → 射撃判定
```

スコープ中心のワールド座標をカメラで変換:
```csharp
Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
_scopeCenter = Vector2.Lerp(_scopeCenter, mouseWorld, Time.deltaTime * scopeFollowSpeed);
```

## スコープ揺れシステム

スコープは常に微振動する（Perlin Noiseベース）:
```csharp
float noiseX = (Mathf.PerlinNoise(Time.time * swayFreq, 0f) - 0.5f) * swayAmplitude;
float noiseY = (Mathf.PerlinNoise(0f, Time.time * swayFreq) - 0.5f) * swayAmplitude;
_scopeVisualOffset = new Vector2(noiseX, noiseY);

// 風の影響（Stage 3以上）
_scopeVisualOffset += _windOffset;
```

揺れ幅:
- Stage 1-2: swayAmplitude = 0.5
- Stage 3-4: swayAmplitude = 0.7
- Stage 5: swayAmplitude = 0.9

## 射撃判定

```csharp
Vector2 actualAimPos = _scopeCenter + _scopeVisualOffset;  // 実際の照準位置（揺れ込み）
foreach (var target in activeTargets) {
    float dist = Vector2.Distance(actualAimPos, target.WorldPosition);
    if (dist < hitRadius) {
        bool headshot = dist < headshotRadius;  // 0.3
        target.OnHit(headshot);
        // ビジュアルフィードバック
    }
}
```

## SceneSetup の構成方針

`Setup033v2_AimSniper.cs` → `Assets/Editor/SceneSetup/`
MenuItem: `"Assets/Setup/033v2 AimSniper"`

構成:
1. Camera（背景色：暗い夜空 #0D1117）
2. Background（夜景スプライト）
3. GameManager（AimSniperGameManager + StageManager子オブジェ）
4. AimSniperMechanic（GameManagerの子）
5. ScopeOverlay（SpriteRenderer, sortOrder=10）
6. Canvas → HUD / InstructionPanel / StageClearPanel / GameOverPanel

## StageManager統合

```csharp
_stageManager.SetConfigs(new StageManager.StageConfig[] {
    new() { speedMultiplier=0.0f, countMultiplier=3, complexityFactor=0.0f, stageName="Stage 1" },
    new() { speedMultiplier=1.0f, countMultiplier=4, complexityFactor=0.2f, stageName="Stage 2" },
    new() { speedMultiplier=1.5f, countMultiplier=5, complexityFactor=0.3f, stageName="Stage 3" },
    new() { speedMultiplier=1.5f, countMultiplier=6, complexityFactor=0.5f, stageName="Stage 4" },
    new() { speedMultiplier=2.0f, countMultiplier=7, complexityFactor=0.7f, stageName="Stage 5" },
});
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();
```

OnStageChanged → mechanic.SetupStage(config, stageIndex)

## InstructionPanel内容

```csharp
_instructionPanel.Show("033v2", "AimSniper", 
    "スコープでターゲットを狙い撃とう",
    "ドラッグでスコープ移動、タップで射撃",
    "限られた弾数で全ターゲットを撃破しよう");
```

## ビジュアルフィードバック設計

1. **命中成功（通常）**: ターゲットがスケールパルス（1.0 → 1.3 → 消滅）+ 白フラッシュ
2. **ヘッドショット**: ターゲット消滅 + 黄色フラッシュ + SpriteRenderer色変化（0.15秒）
3. **外れ（ミス）**: スコープ外枠を赤フラッシュ（0.2秒）
4. **ゲームオーバー**: カメラシェイク（0.5秒）

## スコアシステム

- 通常命中: 10pt
- ヘッドショット: 30pt
- 連続ヘッドショット2連: ×1.5
- 連続ヘッドショット3連以上: ×2.0
- 残弾ボーナス: 残弾数 × 50pt（ステージクリア時）

## ステージ別新ルール表

| ステージ | 新要素 |
|---------|-------|
| Stage 1 | 静止ターゲット・スコープ揺れのみ（チュートリアル） |
| Stage 2 | ターゲットが左右往復移動開始 |
| Stage 3 | 風エフェクト追加（スコープに定常オフセット） |
| Stage 4 | 遠距離ターゲットは揺れ大・近距離は揺れ小（距離によるスコープ揺れ変化） |
| Stage 5 | 障害物（遮蔽物）出現、ターゲットが定期的に隠れる |

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;  // 5.0
float camWidth = camSize * Camera.main.aspect;

// ゲーム領域（ターゲット配置）
float topY = camSize - 1.5f;    // HUD下端
float bottomY = -camSize + 3.0f; // UIボタン上端
// ターゲットはこの範囲内にランダム配置
```

## Buggy Code防止チェック

- `Physics2D.OverlapPoint` でターゲット当たり判定（タグ比較なし）
- 各TargetController に `_isActive` ガード
- `Texture2D`・`Sprite` は `OnDestroy()` でクリーンアップ
- スコープ位置は毎フレーム `Camera.main.ScreenToWorldPoint` で変換
- 固定座標ハードコーディングなし（全てcamSize基準）
