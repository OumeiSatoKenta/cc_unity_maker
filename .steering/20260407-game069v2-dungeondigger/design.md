# Design: Game069v2_DungeonDigger

## namespace
`Game069v2_DungeonDigger`

## スクリプト構成

### DungeonDiggerGameManager.cs
- ゲーム状態管理（Playing / StageClear / AllClear）
- StageManager・InstructionPanel統合
- [SerializeField] StageManager _stageManager
- [SerializeField] InstructionPanel _instructionPanel
- [SerializeField] DigManager _digManager
- [SerializeField] DungeonDiggerUI _ui
- Start()でInstructionPanel.Show → OnDismissed → StartGame
- StartGame()でStageManager.StartFromBeginning()
- OnStageChanged(int) → DigManager.SetupStage(config, stageIndex)
- OnAllStagesCleared() → AllClear表示

### DigManager.cs（コアメカニクス）
- 掘削グリッド管理（5列×可変行のブロック配置）
- タップ入力: Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint
- 自動掘削（autoRate/秒でゲームループ内に自動掘削）
- ブロックHP管理（tapPowerで削る、0になると掘削完了）
- アイテムドロップ（BlockType別確率でGold/Copper/Iron/Gem/RareGem生成）
- 地下生物出現（stageIndex>=2でMonsterスポーン）
- 溶岩層（stageIndex>=3でLavaブロック追加、耐熱なし→深度ペナルティ）
- ボス（stageIndex==4でHero登場）
- コンボシステム（連続タップ3秒以内で継続、コンボ数でスコア乗算）
- SetupStage(StageManager.StageConfig config, int stageIndex)
- **レスポンシブ配置**: Camera.main.orthographicSizeから動的計算
  ```csharp
  float camSize = Camera.main.orthographicSize; // 6
  float camWidth = camSize * Camera.main.aspect;
  float topMargin = 1.8f; // HUD領域
  float bottomMargin = 3.5f; // UI領域
  float availableH = camSize * 2f - topMargin - bottomMargin;
  int cols = 5;
  float cellSize = Mathf.Min(availableH / visibleRows, camWidth * 2f / cols, 1.0f);
  ```
- 深度進行: ブロック掘削でdepthMeter増加、stageDepthTargetに到達でStageClear
- ゴールドシステム: アイテム売却でゴールド獲得、アップグレード購入
- アップグレード3種:
  - Drill: tapPowerUP（コスト: 20, 50, 120, 300, 700）
  - HeatShield: 溶岩ダメージ無効化
  - Lantern: レアアイテム発見率1.5倍

### DungeonDiggerUI.cs
- 深度テキスト（"深度: 0m"）
- ゴールドテキスト
- アイテム数テキスト
- ドリルLvテキスト
- ステージテキスト（"Stage X / 5"）
- 自動掘削速度テキスト（"/秒"）
- コンボテキスト
- ステージクリアパネル（「次のステージへ」ボタン）
- 全クリアパネル
- アップグレードボタン（Drill / HeatShield / Lantern）

## InstructionPanel内容
- gameId: "069"
- title: "DungeonDigger"
- description: "地下を掘り進めてお宝を見つけよう"
- controls: "タップで掘削、ボタンでアップグレード"
- goal: "深度目標を達成してステージクリア"

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stageIndex) {
    var config = _stageManager.GetCurrentStageConfig();
    _digManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

## ステージ別パラメータ表（StageManager.StageConfig利用）
| Stage | speedMultiplier | countMultiplier | complexityFactor | 用途 |
|-------|----------------|----------------|-----------------|------|
| 1 | 1.0 | 1.0 | 0.0 | 手動タップのみ |
| 2 | 1.2 | 1.5 | 0.3 | 自動ドリル解放 |
| 3 | 1.5 | 2.0 | 0.6 | 地下生物 |
| 4 | 2.0 | 3.0 | 0.8 | 溶岩層 |
| 5 | 2.5 | 4.0 | 1.0 | ボス |

各ステージの深度目標: [50, 200, 500, 1000, 2000]
各ステージのtapPower: [1, 1, 2, 3, 5]
各ステージのblockHP: [1, 3, 5, 8, 12]
各ステージのautoRate: [0, 0.5, 1.0, 1.5, 2.0]

## ビジュアルフィードバック設計
1. **ブロック掘削成功**: ブロックがスケールパルス（1.0→1.3→0→消滅）
2. **レアアイテム発見**: ゴールドコインがはじける色フラッシュ（黄色ハイライト0.3秒）
3. **地下生物出現**: SpriteRendererが赤点滅
4. **溶岩ダメージ**: カメラシェイク（0.3秒）+ 深度テキスト赤フラッシュ
5. **コンボ継続**: コンボテキストがスケールアップ（1.0→1.5、0.1秒）

## スコアシステム
- 基本: ゴールド獲得量
- コンボボーナス: combo>=5で×1.5、combo>=10で×2.0、combo>=20で×3.0
- 最終スコア = 総ゴールド × (1 + レアアイテム数 * 0.1)

## SceneSetup構成方針
- Camera: orthographicSize=6、背景色=#1a0a2e（ダーク紫）
- Background: 地下断面図スプライト
- DigGrid: 世界空間に5列×8行のブロック配置（スクロール不使用、同じ行を使いまわす）
- Canvas: ScreenSpaceOverlay
  - 上部HUD: ステージ、深度、ゴールド
  - 下部ボタン: ドリル強化、耐熱装備、照明 / 売却ボタン
  - 右下: メニューへ戻る

## 配線が必要なフィールド
- DungeonDiggerGameManager: _stageManager, _instructionPanel, _digManager, _ui
- DigManager: GameManager参照（GetComponentInParent）, ブロックSpriteRenderer配列
- DungeonDiggerUI: 各Textコンポーネント, StageClearPanel, AllClearPanel, UpgradeButtons

## 判断ポイントの実装設計
- アップグレードボタンはゴールド不足で非活性化（視覚的フィードバック）
- 地下生物: 5秒以内にタップしないとアイテム-1（インベントリ減少）
- 溶岩: 耐熱装備なしでLavaブロックタップ→深度-10m + シェイク
