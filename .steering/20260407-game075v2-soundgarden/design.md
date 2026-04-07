# Design: Game075v2_SoundGarden

## Namespace
`Game075v2_SoundGarden`

## スクリプト構成

### SoundGardenGameManager.cs
- ゲーム状態管理（Playing / StageClear / AllClear / GameOver）
- StageManager・InstructionPanel統合
- GardenController, SoundGardenUI への指示
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] GardenController _gardenController`
- `[SerializeField] SoundGardenUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager.StartFromBeginning()
- OnStageChanged(int stage): GardenController.SetupStage(config, stage)
- OnAllStagesCleared(): GardenController停止 → ShowAllClear

### GardenController.cs（コアメカニクス）
- 植物（Plant）の管理・生成・更新
- BPMタイマー管理
- タップ入力（Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint）
- 害虫生成・管理（ステージ3以降）
- コンボ・スコア計算
- `using UnityEngine.InputSystem;`
- SetupStage(StageManager.StageConfig config, int stageIndex):
  - BPM, 植物数, 制限時間, フラグを適用
  - レスポンシブ配置でプロットを動的計算
- Update(): BPMタイマー → ビート通知 → タイムカウントダウン
- HandleTap(): 植物または害虫のタップ判定
- 判定窓: Perfect±0.12s / Great±0.25s / Good±0.45s

### Plant.cs（植物エンティティ）
- 成長レベル（0〜3）とゲージ管理
- ビートサイン（光るアニメーション）ON/OFF
- スプライト切り替え（成長段階ごと）
- ビジュアルフィードバック: 
  - 成功タップ: スケールパルス（1.0→1.3→1.0、0.15秒）
  - Miss: 赤フラッシュ（SpriteRenderer.color）
  - 最大成長達成: 黄色グロー + 大スケールパルス

### SoundGardenUI.cs
- スコア・コンボ・ステージ・タイマー表示
- ステージクリアパネル / 全クリアパネル / ゲームオーバーパネル
- 判定テキスト（Perfect/Great/Good/Miss、植物付近に一時表示）

## レスポンシブ配置設計
```csharp
float camSize = Camera.main.orthographicSize; // 6.0
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.5f;   // HUD領域
float bottomMargin = 3.0f; // Canvas UIボタン領域
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// 植物配置: 2x1, 3x1, 2x2, 3x2, 2x3 グリッド（植物数に応じて）
// セルサイズ: availableHeight / rows, 最大1.8
```

植物数ごとの配置:
- 2植物: 1行2列
- 3植物: 1行3列
- 4植物: 2行2列
- 5植物: 2行3（上2, 下3）
- 6植物: 2行3列

## SceneSetup構成方針（Setup075v2_SoundGarden.cs）
- MenuItem: `"Assets/Setup/075v2 SoundGarden"`
- Camera backgroundColor: 暗い緑 (0.05, 0.12, 0.05)
- 背景スプライト配置
- GameManager → GardenController, StageManager, InstructionPanel配線
- Canvas: HUD（Stage, Score, Combo, Timer）＋各パネル
- ボタン最小サイズ: (150, 55)

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stageIndex)
{
    var config = _stageManager.GetCurrentStageConfig();
    _gardenController.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

StageConfig値:
- Stage 0: speedMult=1.0, countMult=1.0, complexity=0.0
- Stage 1: speedMult=1.25, countMult=1.0, complexity=0.25
- Stage 2: speedMult=1.5, countMult=1.0, complexity=0.5
- Stage 3: speedMult=1.75, countMult=1.0, complexity=0.75
- Stage 4: speedMult=2.0, countMult=1.0, complexity=1.0

## InstructionPanel設定
- gameId: "075"
- title: "SoundGarden"
- description: "植物をリズムに合わせてタップして育てよう"
- controls: "植物が光ったらタイミングよくタップ！Perfect判定で大きく成長するよ"
- goal: "全ての植物を最大成長させてハーモニーを完成させよう"

## ビジュアルフィードバック設計
1. **成功タップ（Perfect/Great）**: スケールパルス（1.0→1.3→1.0、Coroutine、0.15秒）
2. **Miss**: SpriteRenderer.color赤フラッシュ（0.3秒後に白に戻す）
3. **最大成長達成**: 黄色グロー＋大スケールパルス（1.0→1.8→1.0、0.3秒）
4. **コンボ50以上**: UI側でComboテキストにスケールアニメーション

## スコアシステム
- Perfect: 100pt × multiplier（combo×0.1+1.0、max3.0）
- Great: 60pt × multiplier（combo×0.05+1.0、max2.0）
- Good: 20pt（倍率なし）
- Miss: コンボリセット
- 完全ハーモニーボーナス: 全植物同時完成で×2.0

## ステージ別パラメータ表
| Stage | BPM | 植物数 | 制限秒 | 同時タップ | 害虫 | BPM変動 | 共鳴ボーナス |
|-------|-----|--------|--------|-----------|------|---------|------------|
| 1 | 80 | 2 | 60 | なし | なし | なし | なし |
| 2 | 100 | 3 | 55 | あり | なし | なし | なし |
| 3 | 120 | 4 | 50 | あり | あり | なし | なし |
| 4 | 140 | 5 | 45 | あり | あり | あり | なし |
| 5 | 160 | 6 | 40 | あり | あり | あり | あり |

## Buggy Code防止
- Physics2D比較: `gameObject.name` または `GetComponent<Plant>()` で判定（タグ/レイヤー文字列比較は使わない）
- `_isActive` ガードで複数Update競合を防ぐ
- Texture2D/Sprite生成物は `OnDestroy()` でクリーンアップ
- `Mouse.current.position.ReadValue()` を使用（Input.mousePosition禁止）
