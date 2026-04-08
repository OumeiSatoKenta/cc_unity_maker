# Design: Game088v2_AlchemyPet

## スクリプト構成

### namespace: Game088v2_AlchemyPet

| クラス | 担当 | 参照取得 |
|--------|------|---------|
| `AlchemyPetGameManager` | ゲーム状態・スコア・コンボ管理 | SerializeField |
| `AlchemyManager` | コアメカニクス（素材管理・合成・育成） | GetComponentInParent or SerializeField |
| `AlchemyPetUI` | UI表示・パネル制御 | SerializeField |

## 盤面・ステージデータ設計

### 素材定義
```
素材ID 0-3:  基本素材（火・水・土・風）　　　　 ← Stage1〜
素材ID 4-7:  二次素材（炎・氷・砂・嵐）　　　　 ← Stage2〜
素材ID 8-11: 三次素材（岩・雷・毒・光）　　　　 ← Stage3〜
素材ID 12-13: 危険素材（爆薬・禁断の粉）　　　　 ← Stage4〜
素材ID 14-15: 伝説素材（星屑・ドラゴンの鱗）　　 ← Stage5〜
```

### ペット定義（25種）
- 基本ペット（3種）: 火+水=サラマンダー、土+風=フェニックス、火+土=ゴーレム
- 標準ペット（5種）: 各2素材組み合わせ
- 高度ペット（7種）: 3素材組み合わせ
- レアペット（7種）: 危険素材込み組み合わせ
- 伝説ペット（3種）: コンボ状態+特殊素材のみ誕生

### ステージ別パラメータ
| Stage | speedMultiplier | countMultiplier | complexityFactor | スロット数 | 目標ペット | 使用可能素材数 |
|-------|----------------|----------------|-----------------|----------|-----------|-------------|
| 1     | 1.0            | 1              | 0.0             | 2        | 2         | 4           |
| 2     | 1.0            | 1              | 0.25            | 2        | 5         | 8           |
| 3     | 1.2            | 2              | 0.5             | 3        | 8         | 12          |
| 4     | 1.5            | 2              | 0.75            | 3        | 12        | 14          |
| 5     | 2.0            | 3              | 1.0             | 3        | 15        | 16          |

complexityFactor: 0.25未満 → 危険素材なし、0.5以上 → 3スロット解禁、0.75以上 → 危険素材登場、1.0 → 伝説ペット条件追加

## 入力処理フロー

```
素材ボタンタップ
  → AlchemyManager.SelectMaterial(materialId)
  → 選択中スロットに素材をセット
  → UI更新（スロット表示）

錬金ボタンタップ
  → AlchemyManager.TryCombine()
  → 素材組み合わせをHashCodeで検索
  → 成功: 新ペット誕生エフェクト → OnPetDiscovered()
  → 失敗（未知）: 爆発エフェクト → OnExplosion()
  → UI更新（図鑑・失敗カウント）

餌やりボタン（育成中ペットがいるとき）
  → AlchemyManager.FeedPet()
  → ペット成長 → 素材ドロップ（ランダム）
  → UI更新
```

## SceneSetup 構成方針

### ファイル: `Setup088v2_AlchemyPet.cs`
### MenuItem: `Assets/Setup/088v2 AlchemyPet`

**ヒエラルキー構成**:
```
AlchemyPetGameManager
  └ StageManager
  └ AlchemyManager
Canvas (sortOrder=10)
  └ InstructionPanel (フルスクリーンオーバーレイ)
  └ HUD
    └ StageText (左上)
    └ ScoreText (右上)
    └ ComboText (中央上)
    └ MissText (左上段)
  └ InventoryPanel (下部左)
    └ 素材ボタン群（最大16個）
  └ AlchemyArea
    └ Slot0, Slot1[, Slot2] (素材スロット)
    └ CombineButton (錬金釜ボタン)
  └ PetArea
    └ PetDisplay (現在のペット)
    └ FeedButton (餌やりボタン)
  └ BookButton (図鑑ボタン)
  └ PetBookPanel (図鑑パネル)
  └ StageClearPanel
  └ GameOverPanel
  └ BackButton (左下)
  └ HelpButton (右下)
```

## StageManager 統合

```csharp
_stageManager.OnStageChanged += OnStageChanged;
_stageManager.OnAllStagesCleared += OnAllStagesCleared;
_stageManager.StartFromBeginning();

void OnStageChanged(int stageIndex) {
    var config = _stageManager.GetCurrentStageConfig();
    _alchemyManager.SetupStage(config, stageIndex);
    _ui.UpdateStage(stageIndex + 1, 5);
}
```

## InstructionPanel 内容

- title: "AlchemyPet"
- description: "錬金術でユニークなペットを生み出そう"
- controls: "素材ボタンで選択して投入\n錬金ボタンで合成実行"
- goal: "素材を組み合わせてペット図鑑をコンプリートしよう"

## ビジュアルフィードバック設計

1. **ペット誕生エフェクト**: スケールパルス（0 → 1.5 → 1.0、0.4秒）+ 黄金色フラッシュ
2. **爆発エフェクト**: 錬金釜を赤フラッシュ + カメラシェイク（0.2秒）+ 失敗カウント点滅

## スコアシステム

- 新ペット発見: +20pt
- レアペット発見: +50pt（危険素材使用成功）
- 伝説ペット発見: +100pt
- 育成完了（マックスレベル）: +30pt
- 新レシピ発見: +15pt
- コンボ乗算: ×1.0 → ×1.3 → ×1.6 → ×2.0（連続成功）

## ステージ別新ルール表

| Stage | 新要素 |
|-------|--------|
| Stage 1 | 基本2素材合成（チュートリアル的）|
| Stage 2 | 育成システム追加（ペットに餌やりで素材ドロップ）|
| Stage 3 | 3スロット解禁（3素材合成可能）|
| Stage 4 | 危険素材登場（失敗確率UP、成功時レア確定）|
| Stage 5 | 伝説ペット（コンボ×3以上状態でのみ誕生）|

## 判断ポイントの実装設計

- **素材配分判断**: インベントリ残量が少ない素材を既知レシピに使うか未知実験に使うか
- **爆発リスク**: 危険素材（Stage4+）使用時は失敗確率30%アップ → 残り失敗回数少ない時は回避推奨
- **報酬/ペナルティ**: 危険素材成功 → レアペット確定+50pt、失敗 → 失敗カウント1消費

## Buggy Code 防止事項

- Physics2Dタグ比較: `gameObject.name` または `gameObject.tag`を使用
- 複数Update(): `_isActive` ガードで入力処理を制御
- Texture2D/Sprite: `OnDestroy()` でクリーンアップ
- 素材ボタンは`_isActive`がtrueの時のみ有効

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize; // 6f
float camWidth = camSize * Camera.main.aspect;
// ゲームワールド（背景のみ、UI重なりなし）
// 全ゲームロジックはCanvas UI内のボタン・パネルで処理
// ペット表示: Canvas内のImage (ゲームオブジェクトではない)
```

## 設計の核心

AlchemyPetはシミュレーション/育成ゲームなのでワールド座標への動的配置は最小限。
主にCanvas UI（インベントリパネル・合成エリア・ペット表示）で完結する設計とする。
背景スプライトのみワールド座標に配置。
