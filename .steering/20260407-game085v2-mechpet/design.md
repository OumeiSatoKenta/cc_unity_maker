# Design: Game085v2_MechPet

## namespace

`Game085v2_MechPet`

## スクリプト構成

| ファイル | クラス | 役割 |
|--------|--------|------|
| `MechPetGameManager.cs` | `MechPetGameManager` | ゲーム状態管理・StageManager/InstructionPanel統合 |
| `MechPetManager.cs` | `MechPetManager` | ロボット組み立て・ミッション処理・シナジー計算 |
| `MechPetUI.cs` | `MechPetUI` | UI表示（スコア・エネルギー・スロット・パネル） |

## クラス詳細

### MechPetGameManager

**フィールド:**
```csharp
[SerializeField] StageManager _stageManager
[SerializeField] InstructionPanel _instructionPanel
[SerializeField] MechPetManager _mechPetManager
[SerializeField] MechPetUI _ui
int _combo
float _scoreMultiplier
int _totalScore
```

**Start()フロー:**
1. `_instructionPanel.Show("085", "MechPet", "メカパーツでロボットペットを組み立てよう", "スロットをタップしてパーツ切り替え\nエネルギーをチャージしてロボットを強化", "ミッションをクリアして最強のメカペットを目指そう")`
2. `_instructionPanel.OnDismissed += StartGame`

**StartGame()フロー:**
1. `_combo = 0; _scoreMultiplier = 1.0f; _totalScore = 0`
2. `_stageManager.OnStageChanged += OnStageChanged`
3. `_stageManager.OnAllStagesCleared += OnAllStagesCleared`
4. `_stageManager.StartFromBeginning()`

**OnStageChanged(int stage):**
- `_mechPetManager.SetupStage(_stageManager.GetCurrentStageConfig(), stage)`
- `_ui.UpdateStage(stage + 1, 5)`

**OnMissionResult(bool success, int score):**
- 成功: `_combo++; _scoreMultiplier = Mathf.Min(1.0f + _combo * 0.1f, 2.0f); _totalScore += (int)(score * _scoreMultiplier)`
- 失敗: `_combo = 0; _scoreMultiplier = 1.0f`
- スコア到達でStageClear

### MechPetManager

**パーツデータ構造:**
```csharp
enum PartSlot { Head, Body, Arm, Leg }
enum PartType { Normal, Speed, Shield, Heavy, Legendary }

class Part {
    string name;
    PartSlot slot;
    PartType type;
    int attack;
    int defense;
    int speed;
    Sprite sprite;
}
```

**フィールド:**
```csharp
[SerializeField] MechPetGameManager _gameManager
[SerializeField] MechPetUI _ui
// ロボット表示用 SpriteRenderer（4スロット分）
[SerializeField] SpriteRenderer _headRenderer
[SerializeField] SpriteRenderer _bodyRenderer
[SerializeField] SpriteRenderer _armRenderer
[SerializeField] SpriteRenderer _legRenderer
// Sprites
[SerializeField] Sprite[] _headSprites
[SerializeField] Sprite[] _bodySprites
[SerializeField] Sprite[] _armSprites
[SerializeField] Sprite[] _legSprites
List<Part>[] _availableParts (4スロット分)
int[] _selectedIndex (4スロット分)
float _energy
float _maxEnergy = 100f
int _stageTargetScore
bool _isActive
```

**SetupStage(StageManager.StageConfig config, int stageIndex):**
- `stageIndex+1` に応じてパーツ種類数を設定（4/8/12/16/20）
- ステージ2〜: シナジー有効化フラグON
- ステージ3〜: エネルギー充電コスト増加
- 各スロットの初期インデックスを0にリセット
- `_stageTargetScore` = ステージ別目標スコア（100/200/350/500/700）
- `_energy = _maxEnergy`
- ロボット表示更新

**CalculateSynergyBonus():**
- ステージ2以降有効
- 同PartTypeが2個: ×1.3
- 同PartTypeが3個: ×1.5
- 同PartTypeが4個: ×1.8（伝説パーツはステージ5のみ）
- 戻り値: float multiplier

**StartMission():**
- `_isActive = false`（入力ガード）
- エネルギーコスト消費（20）
- totalPower = (attack + defense + speed) * synergyBonus * energyFactor
- ミッション難易度（ステージ別）と比較してsuccess判定
- コルーチンでアニメーション→結果コールバック

**ChargeEnergy():**
- エネルギー+20（ステージ3以降はコスト増加）
- ポップアニメーション

**ビジュアルフィードバック:**
1. ミッション成功: 全SpriteRendererでスケールパルス（1.0→1.3→1.0、0.2秒）+ 緑フラッシュ
2. ミッション失敗: 赤フラッシュ（SpriteRenderer.color→赤→白、0.3秒）
3. シナジー発動: 黄色オーラ（color→黄→白）

**レスポンシブ配置:**
```csharp
float camSize = Camera.main.orthographicSize;  // 6
// ロボット表示は画面中央〜上部
// 頭: (0, camSize - 2.5)
// 胴体: (0, camSize - 4.0)
// 腕: ±1.5 横に展開
// 脚: (0, camSize - 5.5)
```

### MechPetUI

**フィールド:**
```csharp
[SerializeField] TextMeshProUGUI _stageText
[SerializeField] TextMeshProUGUI _scoreText
[SerializeField] TextMeshProUGUI _comboText
[SerializeField] TextMeshProUGUI _synergyText
[SerializeField] Slider _energySlider
[SerializeField] Button[] _slotButtons (4個)
[SerializeField] TextMeshProUGUI[] _slotTexts (4個: 頭/胴体/腕/脚)
[SerializeField] Button _missionButton
[SerializeField] Button _chargeButton
[SerializeField] GameObject _stageClearPanel
[SerializeField] TextMeshProUGUI _stageClearScoreText
[SerializeField] Button _nextStageButton
[SerializeField] GameObject _allClearPanel
[SerializeField] TextMeshProUGUI _allClearScoreText
[SerializeField] GameObject _gameOverPanel
[SerializeField] MechPetManager _mechPetManager
```

## SceneSetup構成方針

**ファイル**: `Setup085v2_MechPet.cs`
**MenuItem**: `"Assets/Setup/085v2 MechPet"`

### 生成階層
```
Camera
Background（SpriteRenderer）
MechPetGameManager
  └── StageManager
  └── MechPetManager
      └── RobotDisplay（子オブジェクト）
          ├── HeadRenderer
          ├── BodyRenderer
          ├── LeftArmRenderer
          ├── RightArmRenderer
          └── LegRenderer
Canvas（sortingOrder: 10）
  ├── StageText
  ├── ScoreText
  ├── ComboText
  ├── SynergyText
  ├── EnergySlider
  ├── SlotPanel
  │   ├── HeadButton
  │   ├── BodyButton
  │   ├── ArmButton
  │   └── LegButton
  ├── MissionButton
  ├── ChargeButton
  ├── BackButton
  ├── StageClearPanel
  ├── AllClearPanel
  └── GameOverPanel
InstructionCanvas（sortingOrder: 100）
  └── InstructionPanel
EventSystem
```

### StageManager設定
```
Stage 1: speedMultiplier=1.0, countMultiplier=1, complexityFactor=0.0
Stage 2: speedMultiplier=1.0, countMultiplier=2, complexityFactor=0.25
Stage 3: speedMultiplier=1.2, countMultiplier=3, complexityFactor=0.5
Stage 4: speedMultiplier=1.5, countMultiplier=4, complexityFactor=0.75
Stage 5: speedMultiplier=2.0, countMultiplier=5, complexityFactor=1.0
```

### InstructionPanel内容
- title: "MechPet"
- description: "メカパーツでロボットペットを組み立てよう"
- controls: "スロットをタップしてパーツ切り替え\nエネルギーをチャージしてロボットを強化"
- goal: "ミッションをクリアして最強のメカペットを目指そう"

### UIレスポンシブ配置
- 上部（Y=-30〜-90）: Stage / Score / Comboテキスト
- 中部（Y=-200〜-500）: SynergyText
- 中央（World座標）: ロボット表示（Camera.orthographicSize基準）
- 下部（Y=350〜450）: エネルギースライダー
- 下部（Y=230〜280）: パーツスロット4ボタン（横並び）
- 最下部（Y=80〜130）: ミッション・充電ボタン横並び
- 最下部（Y=15）: メニューへ戻るボタン

## ビジュアルフィードバック設計

1. **ミッション成功**: スケールパルス（1.0→1.3→1.0）+ 緑フラッシュ（0.2秒）
2. **ミッション失敗**: 赤フラッシュ（SpriteRenderer.color変更、0.3秒）
3. **シナジー発動時**: 黄金色フラッシュ（全パーツ、0.15秒）
4. **エネルギー充電**: 充電アイコンのスケールバウンス

## スコアシステム

- 基本スコア: 各ミッション固定（30〜150ポイント）
- シナジーボーナス乗算: ×1.0〜×1.8
- コンボ乗算: ×(1.0 + combo × 0.1)、最大×2.0
- ステージ目標スコア: 100/200/350/500/700

## ステージ別新ルール表

| ステージ | 新要素 |
|---------|-------|
| Stage 1 | 基本4パーツ、ミッション習得 |
| Stage 2 | シナジーシステム追加（同PartType2個でボーナス） |
| Stage 3 | エネルギー充電コスト増加、12パーツ解放 |
| Stage 4 | 対戦ミッション（敵との能力比較） |
| Stage 5 | 伝説パーツ（×1.8シナジー）、ボス戦 |
