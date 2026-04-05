# Design: Game023v2 ChainSlash

## スクリプト構成

| クラス | ファイル | 役割 |
|-------|---------|------|
| `ChainSlashGameManager` | `ChainSlashGameManager.cs` | ゲーム状態管理、StageManager/InstructionPanel統合、スコア管理 |
| `ChainSlashController` | `ChainSlashController.cs` | コアメカニクス（敵生成・ドラッグ入力・鎖・斬撃処理）、5ステージ対応 |
| `ChainSlashUI` | `ChainSlashUI.cs` | UI表示（スコア・タイマー・ステージ・コンボ倍率・各パネル） |

**namespace**: `Game023v2_ChainSlash`

## 盤面・ステージデータ設計

### EnemyType
```csharp
enum EnemyType { Normal, Shield, Bomb }
```

### EnemyData
```csharp
class EnemyData {
    int instanceId;
    EnemyType type;
    int colorIndex; // 0=赤,1=青
    bool isMoving;
    bool shieldActive; // シールド残り（最大1）
    float moveAngle;   // 移動方向
    float moveSpeed;
    bool isChained;    // 鎖に繋がれているか
    bool isSliced;     // 斬撃済みか
    GameObject go;
    SpriteRenderer sr;
}
```

### StageConfig活用
- `speedMultiplier`: 移動敵の速度倍率
- `countMultiplier`: 敵数倍率
- `complexityFactor`: 爆弾/シールド敵の出現率倍率

## 入力処理フロー（ChainSlashController）

```
Mouse.current.leftButton.wasPressedThisFrame
  → BeginChain()
  → ドラッグ中: 毎フレーム Mouse.current.position.ReadValue()
    → Physics2D.OverlapPoint で敵ヒット検出
    → 新しい敵なら TryAddToChain(enemy)
      → Bomb敵なら即キャンセル（CancelChain）
      → Shield敵なら1回目スキップ（shieldActive=false、2回目で連結）
Mouse.current.leftButton.wasReleasedThisFrame
  → SlashChain() → スコア計算 → 消滅エフェクト
```

## SceneSetup 構成方針

`Setup023v2_ChainSlash.cs`（`Assets/Editor/SceneSetup/`）
- `[MenuItem("Assets/Setup/023v2 ChainSlash")]`
- 基本構成はSetup022v2_GravityBall.csを参考に踏襲
- 敵スプライト: EnemyRed.png, EnemyBlue.png, EnemyShield.png, EnemyBomb.png, ChainLink.png, Background.png

## StageManager統合

```csharp
void StartGame() {
    _stageManager.OnStageChanged += OnStageChanged;
    _stageManager.OnAllStagesCleared += OnAllStagesCleared;
    _stageManager.StartFromBeginning();
}

void OnStageChanged(int stage) {
    State = ChainSlashGameState.Playing;
    var config = _stageManager.GetCurrentStageConfig();
    _controller.SetupStage(config);
    _ui.UpdateStageDisplay(stage + 1, _stageManager.TotalStages);
    // タイマーリセット
}
```

## InstructionPanel内容
- title: "ChainSlash"
- description: "敵をなぞって繋げ、指を離すと一気に斬れるぞ！"
- controls: "敵をドラッグしてなぞり鎖で繋ぐ → 指を離すと一斉に斬撃！\n多く繋ぐほど二乗でスコアがアップ！"
- goal: "制限時間内に最大チェインを狙い高スコアを獲得しよう"

## ビジュアルフィードバック設計

1. **チェイン連結時**: 敵スケールパルス（1.0 → 1.2 → 1.0、0.15秒）+ 黄色にティント
2. **斬撃発動時**: 敵を順番に縮小消滅（localScale 1.0→0 を0.1秒）+ スコアポップアップテキスト
3. **キャンセル（爆弾接触）**: 鎖エフェクト全消去 + カメラシェイク（0.3秒、amplitude=0.15）+ 赤フラッシュ
4. **コンボ倍率上昇時**: 中央に「COMBO x2.0!」テキストを大きく表示（0.5秒）

## スコアシステム
- 基本: count^2 × 10pt
- 同色ボーナス: × 1.5倍
- コンボ倍率: 3秒以内連続斬撃で x1.5→x2.0→x3.0
- コンボ表示: 倍率上昇時にポップアップ

## ステージ別新ルール表
| Stage | 敵タイプ | 移動 | 追加ルール |
|-------|---------|------|---------|
| 1 | Normal(1色) | なし | 基本チェインのみ |
| 2 | Normal(2色) | なし | 同色ボーナス+50%導入 |
| 3 | Normal(2色) | 一部あり(30%) | 移動敵への対応 |
| 4 | Normal+Shield(20%) | あり | シールド敵(2回なぞり) |
| 5 | Normal+Shield+Bomb(15%) | あり | 爆弾回避チェイン（全複合）|

## 判断ポイントの実装設計
- **チェイン数判断**: なぞり中にチェイン数を画面上部に表示し、プレイヤーが「あと何体繋げるか」を常に意識させる
- **爆弾回避**: Bomb敵に触れた瞬間、赤フラッシュ+シェイク+チェイン切断で強烈なペナルティフィードバック
- **同色狙い**: 鎖がチェイン中の色を示し（1色のみの場合は光る）、混色になると通常表示に戻る

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;
float bottomMargin = 2.8f;
// ゲーム領域: Y=-2.2 ~ +3.8
// 敵は乱数で配置。ただし上部/下部マージン内は禁止
float minY = -camSize + bottomMargin + 0.5f;  // = -1.5
float maxY = camSize - topMargin - 0.5f;        // = 3.3
float minX = -camWidth + 0.8f;
float maxX = camWidth - 0.8f;
```

## Buggy Code防止対策
- _isActive ガード: Controller の Update() は State==Playing 時のみ処理
- OnDestroy(): 動的生成した敵GOs/LineRendererは CleanupAll() でDestroy
- Physics2D.OverlapPoint 使用: タグ・レイヤー比較は避ける（name比較も避ける → EnemyData辞書でIDルックアップ）
- `using UnityEngine.InputSystem;` 必須
