# 技術設計: Game101_ChainReactor

## namespace
`Game101_ChainReactor`

## スクリプト構成

### ChainReactorGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- スコア・コンボ・タップ数管理
- StageManager統合（OnStageChanged / OnAllStagesCleared）
- InstructionPanel統合（Show → OnDismissed → StartGame）
- `[SerializeField]` で `ReactorManager`, `ChainReactorUI`, `StageManager`, `InstructionPanel` を参照

### ReactorManager.cs
- コアメカニクス担当
- オーブの生成・配置（ランダム + 最低連鎖保証）
- 爆発処理（タップ → 爆発円生成 → 範囲内オーブ誘爆 → 連鎖）
- 移動オーブ処理（Stage 2+）
- シールドオーブ処理（Stage 3+）
- タイムリミット処理（Stage 4+）
- 入力処理: `Mouse.current.leftButton.wasPressedThisFrame` + ScreenToWorldPoint
- `[SerializeField]` で `_gameManager`, スプライト群を参照

### ChainReactorUI.cs
- タップ残数、スコア、ステージ表示
- コンボ表示（連鎖数 × 倍率）
- ステージクリアパネル、最終クリアパネル、ゲームオーバーパネル
- タイマー表示（Stage 4+）

## StageManager統合

`OnStageChanged(int stage)` で `ReactorManager.SetupStage()` を呼び出し:

| Stage | speedMul | countMul | complexity | 具体的変化 |
|-------|----------|----------|-----------|-----------|
| 0 | 1.0 | 1 | 0.0 | オーブ8個、タップ3、爆発半径1.8 |
| 1 | 1.0 | 1 | 0.2 | オーブ12個、タップ3、半径1.5、**移動オーブ20%** |
| 2 | 1.2 | 2 | 0.4 | オーブ15個、タップ2、半径1.5、**シールドオーブ15%** |
| 3 | 1.5 | 2 | 0.7 | オーブ18個、タップ2、半径1.3、**タイムリミット20秒** |
| 4 | 1.8 | 3 | 1.0 | オーブ22個、タップ2、半径1.2、全要素+**ボーナスオーブ** |

## InstructionPanel内容
- title: "チェインリアクター"
- description: "タップで爆発を起こし、連鎖反応で全オーブを消そう！"
- controls: "画面をタップして爆発を起こす。爆発範囲内のオーブは連鎖爆発する"
- goal: "限られたタップ数で全てのオーブを消せばクリア"

## ビジュアルフィードバック設計
1. **爆発エフェクト**: オーブ消滅時にスケール拡大(1.0→2.0) + 色が白→透明にフェード(0.3秒)
2. **連鎖カウント演出**: 連鎖発生ごとにUI上の連鎖数テキストがスケールパルス(1.0→1.5→1.0)
3. **シェイク**: 大連鎖(5個以上同時)でカメラシェイク
4. **コンボ表示**: 倍率テキストが色変化(白→金→虹)

## スコアシステム
- 基本: 1オーブ爆発 = 100点
- 連鎖ボーナス: n連鎖目 = n × 100 追加
- 1タップ倍率: 5個以上=x2, 10個以上=x3, 15個以上=x5
- 残タップボーナス: 残タップ数 × 500点（ステージクリア時）

## ステージ別新ルール表
- Stage 1: 静止オーブのみ。密集配置で連鎖を学ぶ。タップ3回
- Stage 2: **移動オーブ**追加。ゆっくり直線移動(speed=0.5)。タイミング判断が必要
- Stage 3: **シールドオーブ**追加。2回爆発を受けないと消えない。計画性が重要
- Stage 4: **タイムリミット20秒**追加。じっくり考える vs 急いで打つ
- Stage 5: 全要素 + **ボーナスオーブ**(取るとそのタップのスコア×3)

## 判断ポイントの実装設計
- **トリガー1**: 移動オーブが密集地帯に近づく瞬間 → タップの報酬: 連鎖+3〜5個 / 見逃しのペナルティ: 次の接近まで3〜5秒待ち
- **トリガー2**: シールドオーブの配置 → 先に弱い爆発で1回目を当てる(タップ消費) vs シールド無視して通常オーブ優先
- **トリガー3**: 残り1タップ時 → 確実に3個消す安全策(300点) vs 移動オーブ接近を待って8個狙い(800点+連鎖ボーナス、失敗リスクあり)

## オーブデータ構造
```
OrbData:
  GameObject obj
  SpriteRenderer renderer
  int type  // 0=通常, 1=移動, 2=シールド, 3=ボーナス
  int shieldHP  // シールド: 2→1→0で消滅
  Vector2 moveDir  // 移動オーブの方向
  float moveSpeed  // 移動速度
  bool isExploding  // 爆発中フラグ
```

## 爆発処理フロー
1. タップ位置に爆発円（半径: ステージ依存）を生成
2. 範囲内オーブを検索（Vector2.Distance < blastRadius）
3. 各オーブに対して:
   - 通常: 即爆発 → 新たな爆発円を生成（連鎖）
   - シールド: HP-1、HP=0なら爆発
   - ボーナス: 爆発 + スコア倍率適用
   - 移動: 即爆発
4. 連鎖は0.15秒間隔でウェーブ的に広がる（見た目の満足感）
5. 全連鎖終了後に結果判定

## SceneSetup 配線リスト

### ChainReactorGameManager
- `_reactorManager`: ReactorManager
- `_ui`: ChainReactorUI
- `_stageManager`: StageManager
- `_instructionPanel`: InstructionPanel

### ReactorManager
- `_gameManager`: ChainReactorGameManager
- `_orbSprite`: Sprite（通常オーブ）
- `_shieldOrbSprite`: Sprite（シールドオーブ）
- `_bonusOrbSprite`: Sprite（ボーナスオーブ）
- `_explosionSprite`: Sprite（爆発エフェクト）

### ChainReactorUI
- `_tapsText`: TextMeshProUGUI
- `_scoreText`: TextMeshProUGUI
- `_stageText`: TextMeshProUGUI
- `_chainText`: TextMeshProUGUI（連鎖数表示）
- `_timerText`: TextMeshProUGUI（Stage 4+ タイマー）
- `_orbCountText`: TextMeshProUGUI（残りオーブ数）
- `_stageClearPanel`: GameObject
- `_stageClearText`: TextMeshProUGUI
- `_nextStageButton`: Button
- `_clearPanel`: GameObject
- `_clearText`: TextMeshProUGUI
- `_gameOverPanel`: GameObject
- `_gameOverText`: TextMeshProUGUI
