# Design: Game070v2_NanoLab

## namespace
`Game070v2_NanoLab`

## スクリプト構成

### NanoLabGameManager.cs
- ゲーム状態管理（Playing / StageClear / AllClear）
- StageManager・InstructionPanel統合
- NanoMachineManagerへのステージ設定委譲
- UI更新メソッド群

### NanoMachineManager.cs
- コアメカニクス実装
- ナノマシン増殖ロジック（手動・自動）
- 技術ツリー管理（TechNode[]）
- プレステージ（時代進化）ロジック
- 突然変異イベント（Stage4+）
- 技術融合（Stage5）
- 入力処理（タップ増殖）
- 5ステージ対応 SetupStage(config, stageIndex)

### NanoLabUI.cs
- HUDテキスト更新
- 技術ノードボタン群の動的生成・更新
- ステージ表示・ステージクリアパネル
- プレステージボタン制御

## 技術ツリー設計

```
TechNode:
  - id: string
  - nameJP: string
  - description: string
  - cost: long (ナノマシン消費量)
  - effect: enum (IncreaseClickPower, AutoRate, PrestigeBonus, MutationChance, FusionUnlock)
  - value: float
  - prerequisiteId: string
  - path: enum (Efficiency, Growth, Prestige)
```

### Stage1 ノード (3つ)
- atomic_research: "原子研究" - タップ倍率+1 (cost:10)
- molecular_bond: "分子結合" - 増殖効率+10% (cost:30)
- nano_optimizer: "ナノ最適化" - タップ倍率+2 (cost:80)

### Stage2 追加ノード (分岐: 効率系 or 増殖系)
- auto_replication: "自動複製" - 自動増殖解放 (cost:50, Growthパス)
- efficiency_boost: "効率ブースト" - 全効率+20% (cost:100, Efficiencyパス)

### Stage3 追加ノード (プレステージ強化)
- prestige_amplifier: "プレステージ増幅" - プレステージ倍率+0.5 (cost:200, Prestigeパス)

### Stage4 追加ノード (突然変異)
- mutation_control: "変異制御" - 変異を有利な方向に制御 (cost:500, Efficiencyパス)

### Stage5 追加ノード (技術融合)
- cosmic_fusion: "宇宙融合" - 超技術解放 (cost:1000, 全パス所持で解放)

## 時代設計

| Era | 名前 | 目標ナノマシン (累積) |
|-----|------|---------------------|
| 0 | 原子時代 | 開始 |
| 1 | 分子時代 | 100 |
| 2 | 細胞時代 | 500 |
| 3 | 生物時代 | 2000 |
| 4 | 機械時代 | 10000 |
| 5 | 宇宙時代 | 50000 |

## 5ステージパラメータ

| Stage | targetEra | autoUnlock | prestigeUnlock | mutationEnabled | fusionUnlock | clickPowerBase | autoRateBase |
|-------|-----------|------------|----------------|-----------------|--------------|----------------|--------------|
| 1 | 1 | false | false | false | false | 1 | 0 |
| 2 | 2 | true | false | false | false | 2 | 0.5 |
| 3 | 3 | true | true | false | false | 3 | 1.0 |
| 4 | 4 | true | true | true | false | 5 | 2.0 |
| 5 | 5 | true | true | true | true | 8 | 4.0 |

StageConfig マッピング:
- speedMultiplier → autoRateBase倍率
- countMultiplier → clickPowerBase倍率
- complexityFactor → 追加機能フラグ (0=なし, 0.3=auto, 0.6=prestige, 0.8=mutation, 1.0=fusion)

## クラス参照方法
- NanoMachineManager: `[SerializeField] NanoMachineManager _nanoManager`
- NanoLabUI: `[SerializeField] NanoLabUI _ui`

## StageManager統合
```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;

void OnStageChanged(int stageIndex)
{
    var config = _stageManager.GetCurrentStageConfig();
    _nanoManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

## InstructionPanel
- title: "NanoLab"
- description: "ナノマシンを増やして科学技術を進化させよう"
- controls: "タップで増殖、ボタンで研究・時代進化"
- goal: "時代目標を達成してステージクリア"

## ビジュアルフィードバック設計
1. **タップ増殖演出**: タップ位置にナノマシン数値が浮き上がり ("+N" フロートテキスト、0.5秒で消える)
2. **技術解放演出**: 解放ボタンのスケールパルス (1.0 → 1.3 → 1.0、0.3秒)
3. **時代進化演出**: 画面全体フラッシュ（白→透明、0.5秒）+ 時代名テキストズームイン
4. **突然変異演出**: ランダムなナノマシンオブジェクトが色変化（緑=良い変異、赤=悪い変異）

## スコアシステム
- ベーススコア: 解放済み技術数 × 100
- 時代ボーナス: 到達時代 × 500
- 研究効率ボーナス: 短時間クリア時に倍率 (stageTime < 60秒で×2, < 120秒で×1.5)
- プレステージボーナス: プレステージ回数 × 200

## レスポンシブ配置
- Camera orthographicSize: 6
- 上部HUD (ステージ/時代/ナノマシン数): Y anchor top, 上端-30〜-200
- ゲーム中央: ナノマシンビジュアル（背景スプライトで対応）
- 下部UI: Y=10〜150 にボタン群（タップエリア・技術ボタン・プレステージボタン）
- 技術ツリーエリア: 中段〜下段に縦スクロールパネルで表示

## SceneSetup構成
- Camera (背景色 #0D1B2A)
- Background SpriteRenderer
- NanoLabGameManager
  - StageManager (child)
  - NanoMachineManager (child)
- Canvas (ScreenSpaceOverlay, 1080x1920)
  - StageText (top)
  - EraText (top)
  - NanoCountText (top)
  - AutoRateText (top)
  - TapButton (center, 大きなタップエリア)
  - TechPanel (ScrollRect, 技術ノードボタン群)
  - PrestigeButton (下段)
  - BackToMenuButton (底部左)
  - StageClearPanel (非表示)
  - AllClearPanel (非表示)
  - InstructionPanel (最前面)
- EventSystem (InputSystemUIInputModule)
