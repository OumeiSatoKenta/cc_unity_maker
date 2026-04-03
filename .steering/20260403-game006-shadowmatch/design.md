# Design: Game006v2 ShadowMatch

## スクリプト構成

```
namespace Game006v2_ShadowMatch

ShadowMatchGameManager.cs  ← ゲーム状態管理・StageManager/InstructionPanel統合
ShadowObjectController.cs  ← オブジェクト回転入力処理・影データ計算
ShadowRenderer.cs          ← 影の2D描画（Texture2D動的生成）
ShadowMatchUI.cs           ← UIスコア・ステージ・一致度・パネル管理
```

**重要**: 全クラスに `namespace Game006v2_ShadowMatch` を付与。

## クラス詳細設計

### ShadowMatchGameManager.cs

```csharp
namespace Game006v2_ShadowMatch
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class ShadowMatchGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private ShadowObjectController _shadowObjectController;
        [SerializeField] private ShadowMatchUI _ui;

        private GameState _state;
        private int _score;
        private int _judgeCount;  // 現在ステージの判定回数

        // Start(): InstructionPanel表示 → StartGame() → StageManager.StartFromBeginning()
        // OnStageChanged(int stage): ShadowObjectController.SetupStage(stageConfig) 呼び出し
        // OnAllStagesCleared(): 最終クリアUI表示
        // OnJudgeButton(): _shadowObjectController.CalculateMatch() → スコア計算
    }
}
```

**StageManager統合:**
- `_stageManager.OnStageChanged += OnStageChanged`
- `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
- `OnStageChanged(int stageIndex)` で `_shadowObjectController.SetupStage(stageIndex)` を呼ぶ

**スコアシステム:**
- 基本スコア = matchPercent * 100
- 乗算: judgeCount=1: ×3.0, 2: ×2.0, 3: ×1.5, 4+: ×1.0
- パーフェクト(1回で95%+): +500

### ShadowObjectController.cs

**入力処理（2D/3D混合なしの純2D方式）:**
- `Mouse.current.leftButton.isPressed` でドラッグ検出
- `Mouse.current.delta.ReadValue()` でdeltaX/Y取得
- deltaX → currentRotation.y 加算
- deltaY → currentRotation.x 加算（ステージ1はY軸のみ、ロックを考慮）

**影の計算（2D投影シミュレーション）:**
- オブジェクトの頂点リスト（8頂点のBox）を定義
- 回転行列（Quaternion.Euler(rot)）でワールド座標変換
- 光源方向ベクトル（例: Vector3(0.5f, 1f, 0.3f)）から投影行列計算
- 投影した2D座標の凸包→Texture2Dにラスタライズ

**ステージ設定（SetupStage）:**
```csharp
struct StageConfig {
    float[] targetRotation;  // [x, y] の目標角度
    bool lockX, lockZ;       // 軸ロック
    int hintCount;           // ヒント残数
    bool dualLight;          // 複数光源フラグ
    bool dualObject;         // 複合オブジェクトフラグ
    float matchThreshold;    // クリア閾値(0.8)
}
```

**CalculateMatch():**
- 現在の影Texture2Dと目標シルエットTexture2Dをピクセル比較
- 一致度 = 重なりピクセル数 / (影+目標の合計ピクセル - 重なりピクセル) [IoU]
- 返り値: float (0.0〜1.0)

**レスポンシブ配置:**
```csharp
float camSize = Camera.main.orthographicSize;  // 6.0
// オブジェクト表示: 中央 y=1.5〜2.0
// 影表示: 中央 y=-0.5〜0.5
// 目標シルエット: 下部 y=-2.5〜-1.5
// 上部マージン(HUD): y > 4.0
// 下部マージン(UI): y < -3.5
```

**ビジュアルフィードバック:**
1. **クリア時**: オブジェクトのスケールパルス (1.0→1.3→1.0, 0.2秒) + 影が金色にフラッシュ
2. **ミス時**: カメラシェイク (0.1秒、振幅0.2) + 影が赤くフラッシュ (0.3秒)
3. **ヒント表示時**: 正解方向に矢印オブジェクトが1.5秒表示

### ShadowRenderer.cs

- `Texture2D _shadowTexture` (128x128, RGBA32) を動的生成
- `UpdateShadow(Vector2[] projectedPoints)`: 点列からポリゴンをラスタライズ
- スプライトとして `SpriteRenderer` に割り当て
- `OnDestroy()`: `Destroy(_shadowTexture)` でクリーンアップ

### ShadowMatchUI.cs

**必須表示要素:**
- ステージテキスト「Stage X / 5」
- スコアテキスト
- 一致度テキスト「一致度: XX%」（リアルタイム更新）
- 判定回数テキスト「判定: N回」
- ヒント残数テキスト「ヒント残り: N回」
- ステージクリアパネル（「次のステージへ」ボタン）
- 最終クリアパネル
- ゲームオーバーパネル（※このゲームにはゲームオーバーなし）

## SceneSetup設計

### Setup006v2_ShadowMatch.cs

```
[MenuItem("Assets/Setup/006v2 ShadowMatch")]
```

**配置構成:**
- Camera: orthographic, size=6, 背景色: 暗い紫 (0.08, 0.05, 0.15)
- Background: 全画面背景スプライト
- ObjectDisplay: 中央上部 (0, 1.8, 0) - ShadowObjectControllerコンポーネント
  - MainObject: SpriteRenderer (オブジェクト形状)
  - ShadowDisplay: SpriteRenderer (影の描画、y=-0.5)
  - TargetSilhouette: SpriteRenderer (目標形状、y=-2.5)
  - HintArrow: 矢印オブジェクト (非表示)
- Canvas > HUD (上部):
  - StageText: アンカー上中央, anchoredPos (0, -15)
  - ScoreText: アンカー上中央, anchoredPos (0, -55)
  - MatchText: アンカー上右, anchoredPos (-20, -45)
  - JudgeText: アンカー上左, anchoredPos (20, -45)
- Canvas > ボタン (下部):
  - JudgeButton: アンカー下中央, anchoredPos (-110, 80), size (180, 65)
  - HintButton: アンカー下中央, anchoredPos (105, 80), size (150, 65)
  - MenuButton: アンカー下左, anchoredPos (20, 20), size (150, 55)
- InstructionPanel: フルスクリーンオーバーレイ
- StageClearPanel, GameClearPanel（GameOverPanelは最小限）

**StageManager配線:**
- GameManagerの子オブジェクトとして生成
- `_stageManager` SerializeFieldに配線

**InstructionPanel配線:**
- Title/Desc/Controls/Goal/StartButton/HelpButton を配線

## ステージ別新ルール表

| Stage | 新要素 | 実装詳細 |
|-------|--------|---------|
| 1 | Y軸のみ回転 | `lockX=true, lockZ=true` |
| 2 | X+Y 2軸解禁 | `lockX=false, lockZ=true` |
| 3 | 複合オブジェクト | `dualObject=true` → 2つのSpriteをずらして配置 |
| 4 | 複数光源 | `dualLight=true` → 2方向から影を計算・表示 |
| 5 | Z軸ロック + 高精度 | `lockZ=true, matchThreshold=0.80` (厳しい許容誤差) |

## 判断ポイントの実装設計

**回転軸優先の判断:**
- X軸が±90°以上回転すると影の縦横が入れ替わる → 先にX軸を合わせてからY軸を調整するのが効率的
- ゲームはこれをプレイヤーが自然に学習する設計

**精度 vs 回数トレードオフ:**
- judgeCount=1の乗算×3.0は強力 → 最初の判定で75%超えれば次のステージより高スコアになりうる
- プレイヤーに「早めに押すか、完璧を目指すか」を常に意識させる

## 影の投影アルゴリズム（簡略版）

2D表示のため、以下の簡略アプローチを採用:

1. オブジェクト形状は「回転角度に応じた2Dプロジェクション」をテクスチャで表現
2. 実際の3D計算ではなく、ターゲット角度と現在角度の**差分**をIoUで計算
3. 影テクスチャ = `targetRotation` から事前計算したスプライトとの差分を表示
4. リアルタイム影はオブジェクトのSpriteRendererを回転させて代替表示

**現実的な実装（2D完結）:**
- ObjectSprite: 回転して見える形（立方体なら等角投影風スプライト複数枚を用意）
- ShadowSprite: 現在回転から計算したグレースケール投影
- TargetSprite: 目標角度から計算した白輪郭投影
- MatchRate: 2スプライトのピクセル差分（絶対差の合計を正規化）

## カラーパレット（puzzle カテゴリ）

- メイン: `#2196F3` (青)
- サブ: `#00BCD4` (ティール)  
- アクセント: `#E3F2FD` (淡青)
- 影色: `#555577` (暗い青紫)
- 目標シルエット: `#FFFFFF` 白輪郭 + 透明背景
- 背景: `#141428` 暗い紺
