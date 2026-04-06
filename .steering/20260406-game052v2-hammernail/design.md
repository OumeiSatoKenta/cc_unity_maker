# Design: Game052v2_HammerNail

## namespace
`Game052v2_HammerNail`

## スクリプト構成

### HammerNailGameManager.cs
- ゲーム状態管理 (Idle/Playing/StageClear/AllClear/GameOver)
- StageManager・InstructionPanel統合
- スコア・コンボ管理
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] NailManager _nailManager`
- `[SerializeField] TimingGauge _timingGauge`
- `[SerializeField] HammerNailUI _ui`

### NailManager.cs
- 釘の生成・管理（通常/硬い/ボス釘種別）
- 釘を順番に選択し、現在の釘へのハンマー打撃処理
- `StageConfig`のパラメータでステージ設定を適用
- レスポンシブ配置（orthographicSize基準）
- 釘の種類: Normal(1打), Hard(2打), Boss(5打)

### TimingGauge.cs
- 0〜1を往復するゲージ値管理
- PERFECTゾーン、GOODゾーン、MISSゾーンの判定
- 速度・不規則パターンのステージ制御
- `HitResult GetHitResult()` → PERFECT/GOOD/MISS

### HammerNailUI.cs
- スコア、コンボ、ステージ、ミス数、残り釘数表示
- 判定エフェクト(PERFECT!/GOOD/MISS)
- ステージクリア・ゲームオーバーパネル
- ハンマー振り下ろしアニメーション

## ゲージ設計
```
[MISS][GOOD][  PERFECT  ][GOOD][MISS]
  0   0.2   0.35  0.65  0.8    1.0
```
- PERFECT: 中央±(perfectZoneHalf) 範囲
- GOOD: PERFECT外側±(goodZoneHalf) 範囲
- MISS: それ以外

## 状態遷移
- Idle → Playing (InstructionPanel dismiss後)
- Playing → StageClear (全釘打ち込み完了)
- Playing → GameOver (MISS 3回)
- StageClear → Playing (次ステージ) or AllClear (5ステージ完了)

## StageManager統合
```csharp
_stageManager.SetConfigs(new StageManager.StageConfig[] {
    new() { stageName="Stage 1", speedMultiplier=1.0f, countMultiplier=3, complexityFactor=0.0f },
    new() { stageName="Stage 2", speedMultiplier=1.5f, countMultiplier=5, complexityFactor=0.2f },
    new() { stageName="Stage 3", speedMultiplier=1.5f, countMultiplier=5, complexityFactor=0.4f },
    new() { stageName="Stage 4", speedMultiplier=2.0f, countMultiplier=7, complexityFactor=0.6f },
    new() { stageName="Stage 5", speedMultiplier=2.2f, countMultiplier=8, complexityFactor=0.8f },
});
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();
```

## InstructionPanel内容
- title: "HammerNail"
- description: "リズムよくタップして釘を打ち込もう"
- controls: "ゲージがPERFECTゾーンにある時にタップ！タイミングで釘の沈み具合が変わるよ"
- goal: "全ての釘を打ち込んでステージクリア！"

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 5.0
float camWidth = camSize * Camera.main.aspect;
// 釘配置: X方向に均等配置、Y位置は盤面中央
float boardY = 0.5f; // 盤面Y座標（カメラ中央より少し上）
// 釘間隔: 利用可能幅 / 釘本数
float availableWidth = camWidth * 1.6f;
float nailSpacing = availableWidth / (nailCount + 1);
```

## ビジュアルフィードバック
1. **PERFECT時**: 釘オブジェクトがスケールパルス(1.0→1.3→1.0、0.15秒) + 黄色フラッシュ
2. **MISS時**: 釘が傾く(Z回転±15度) + 赤フラッシュ + 画面シェイク
3. **コンボ時**: スコアテキストにスケールアニメーション

## スコアシステム
- PERFECT: 100pt
- GOOD: 50pt  
- MISS: 0pt + missCount++
- コンボボーナス: 連続PERFECT × 50pt
- ×1.5倍: コンボ5以上
- ×2.0倍: コンボ10以上

## ステージ別新ルール表
| Stage | 釘の種類 | ゲージ速度multiplier | PERFECTゾーン幅 | 新要素 |
|-------|---------|-------------------|--------------|-------|
| 1 | 通常のみ | 1.0 | 0.30 | なし（チュートリアル） |
| 2 | 通常のみ | 1.5 | 0.22 | ゲージ速度アップ |
| 3 | 通常+硬い釘(1〜2本) | 1.5 | 0.20 | 硬い釘登場（2打必要） |
| 4 | 通常+硬い釘+不規則 | 2.0 | 0.15 | 不規則ゲージ（speedが変動） |
| 5 | 全種類（ボス釘含む） | 2.2 | 0.15 | ボス釘（5打必要） |

## SceneSetup構成方針
- MenuItem: `Assets/Setup/052v2 HammerNail`
- Canvas上にタイミングゲージUI（スライダー形式）
- 板(Board)はワールド空間のスプライト
- 釘はNailManagerが動的生成
- GameManagerの子にStageManager
- InstructionPanel（Canvas上、フルスクリーンオーバーレイ）

## SceneSetupでの配線フィールド
HammerNailGameManager:
- _stageManager → StageManagerオブジェクト
- _instructionPanel → InstructionPanelオブジェクト
- _nailManager → NailManagerオブジェクト
- _timingGauge → TimingGaugeオブジェクト
- _ui → HammerNailUIオブジェクト

NailManager:
- _nailPrefab → NailPrefab
- _normalNailSprite, _hardNailSprite, _bossNailSprite

TimingGauge:
- _gaugeImage → ゲージUI Image
- _indicatorImage → インジケーター Image

HammerNailUI:
- _scoreText, _comboText, _stageText, _missText, _remainingNailsText
- _judgmentText → PERFECT!/GOOD/MISS表示
- _stageClearPanel, _gameOverPanel, _allClearPanel
- _nextStageButton, _retryButton, _menuButton
