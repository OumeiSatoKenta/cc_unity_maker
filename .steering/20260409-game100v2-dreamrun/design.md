# Design: Game100v2 DreamRun (remake)

## スクリプト構成

### namespace: `Game100v2_DreamRun`

| クラス | ファイル | 役割 |
|-------|---------|------|
| `DreamRunGameManager` | DreamRunGameManager.cs | ゲーム状態管理・StageManager/InstructionPanel統合 |
| `DreamRunManager` | DreamRunManager.cs | コアメカニクス（キャラ・障害物・断片生成・スクロール） |
| `DreamRunUI` | DreamRunUI.cs | UI表示・ボタンイベント |

## 盤面・ステージデータ設計

### レーンシステム
```
Lane 2 (上): y = +1.5
Lane 1 (中): y = 0.0  ← デフォルト
Lane 0 (下): y = -1.5
```
Stage1は Lane 0 と Lane 1 のみ使用（2レーン）
Stage2〜5は Lane 0〜2 全使用（3レーン）

### スクロール
- 背景・障害物・断片がX方向に左スクロール
- キャラは画面左1/3付近の固定X位置に存在
- スクロール速度はStageConfigのspeedMultiplierに従う

### ステージ別パラメータ表

| Stage | speedMultiplier | countMultiplier | complexityFactor | customData |
|-------|----------------|----------------|-----------------|------------|
| 1     | 1.0             | 1               | 0.0             | "2,5,0.8,false,false" |
| 2     | 1.5             | 1               | 0.3             | "3,7,0.7,false,false" |
| 3     | 2.0             | 1               | 0.5             | "3,8,0.6,true,false" |
| 4     | 2.5             | 1               | 0.7             | "3,10,0.5,true,true" |
| 5     | 3.0             | 1               | 1.0             | "3,12,0.4,true,true" |

customData形式: "laneCount,fragmentCount,obstacleInterval,airObstacle,gravityFlip"

## 入力処理フロー

```
Mouse.current.leftButton.wasPressedThisFrame
 ↓
スクリーン座標をビューポートに変換
 ├── x < 0.33f → 左レーンへ移動
 ├── x < 0.67f → ジャンプ
 └── x >= 0.67f → 右レーンへ移動
```

using `UnityEngine.InputSystem`
`Mouse.current.leftButton.wasPressedThisFrame`
`Mouse.current.position.ReadValue()`

## SceneSetup の構成方針

- `Setup100v2_DreamRun.cs` で全オブジェクトを自動構成
- `[MenuItem("Assets/Setup/100v2 DreamRun")]`
- キャラクタースプライト、背景、障害物、断片スプライトを配線
- StageManager（GMの子）、InstructionPanel（別Canvas sortOrder=100）を配線

## StageManager統合

```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stageIndex) {
    var config = _stageManager.GetCurrentStageConfig();
    _dreamRunManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
}
```

## InstructionPanel内容
- title: "DreamRun"
- description: "夢の世界を走り抜けよう"
- controls: "画面左タップでレーン左移動、中央タップでジャンプ、右タップでレーン右移動"
- goal: "夢の断片をすべて集めてストーリーを完成させよう"

## ビジュアルフィードバック設計

1. **断片収集時（成功）**: コレクタブルのスケールパルス（1.0→1.5→0.0）+ 黄色フラッシュ演出
2. **障害物衝突時（失敗）**: キャラの赤フラッシュ（`SpriteRenderer.color`を赤→白、0.3秒）+ カメラシェイク（0.2秒）
3. **コンボ時**: コンボテキストの拡大アニメ（スケール0.5→1.2→1.0）

## スコアシステム
- 基本スコア: 100 × ステージ番号 × コンボ乗算
- コンボ乗算: `1.0 + comboCount * 0.1` (最大2.0倍)
- ニアミスボーナス: 障害物に0.3u以内で通過 → +10pt × コンボ数

## ステージ別新ルール

| Stage | 新要素 |
|-------|--------|
| 1 | 地上障害物・2レーン・基本操作のみ |
| 2 | 3レーン解放・空中断片（ジャンプ必須） |
| 3 | 空中浮遊障害物追加（ジャンプが常に安全でない） |
| 4 | 重力反転ゾーン（特定区間でジャンプ操作が反転）|
| 5 | 全要素+背景高速シュール変化（視覚ノイズ） |

## 判断ポイントの実装設計
- プレイヤーが選択を迫られる瞬間: 障害物と断片が同じレーンに存在する時
- 断片を取る: +100pt×コンボ、ただし衝突リスク
- スルー: 安全だがコンボリセット
- 重力反転ゾーン: フィールド上の特定X範囲でフラグ管理

## DreamRunManager 主要フィールド（SerializeField + private）
```csharp
[SerializeField] DreamRunGameManager _gameManager;
[SerializeField] Sprite[] _runnerSprites;        // キャラアニメフレーム
[SerializeField] Sprite _obstacleGroundSprite;
[SerializeField] Sprite _obstacleAirSprite;
[SerializeField] Sprite _fragmentSprite;
[SerializeField] Sprite[] _backgroundSprites;   // 視差背景レイヤー
```

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float laneSpacing = camSize * 0.4f;  // レーン間隔をカメラサイズから計算
float laneY1 = -laneSpacing;         // Lane 0
float laneY2 = 0f;                   // Lane 1
float laneY3 = laneSpacing;          // Lane 2
float characterX = -camWidth * 0.5f; // 画面左1/3
float spawnX = camWidth + 1f;        // 画面右外
```

ボトムマージン: 下端から2.8u（UI Button領域）
トップマージン: 上端から1.2u（HUD領域）
