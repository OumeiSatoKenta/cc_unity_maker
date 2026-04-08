# 設計書: Game089v2_IslandHop

## namespace
`Game089v2_IslandHop`

## スクリプト構成

### IslandHopGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- StageManager・InstructionPanel統合
- コンボカウンター・スコア乗算管理
- GameManagerへの参照: `[SerializeField] IslandManager _islandManager`
- GameManagerへの参照: `[SerializeField] IslandHopUI _ui`

### IslandManager.cs
- 島データ管理（最大5島）
- 施設スロット管理（各島に3〜7スロット）
- 資源管理（Wood/Stone/Food/Gold）
- 施設建設処理・シナジー判定
- 訪問客リクエスト管理（ステージ3以降）
- 天候イベント（ステージ4以降）
- **入力処理を一元管理**（Mouse.current）
- SetupStage(StageManager.StageConfig config, int stageIndex) でパラメータ適用

### IslandHopUI.cs
- ステージ表示「Stage X / 5」
- スコア表示（コンボ乗算込み）
- 資源量表示（木材/石/食料/金貨）
- ステージクリアパネル（「次のステージへ」）
- 全クリアパネル
- ゲームオーバーパネル
- 施設選択パネル（建設可能施設一覧）

## 盤面・ステージデータ設計

### 島の表現
```
Island {
    int id
    string name
    Vector3 worldPosition
    FacilitySlot[] slots (3〜7個)
    IslandType type (Forest/Rocky/Tropical/Volcanic/Coral)
    Sprite sprite
    bool isUnlocked
}

FacilitySlot {
    int slotIndex
    Vector3 position
    FacilityType facilityType (None / Cottage / Pier / Garden / Restaurant / Observation / Spa / Marina / Hotel / Lighthouse / Casino / Aquarium)
    bool isOccupied
}
```

### 資源システム
- Wood（木材）: 基本資源、Forestタイプの島で多産
- Stone（石）: 建設材料、Rockyタイプで多産
- Food（食料）: 訪問客満足度に影響
- Gold（金貨）: 特殊施設の建設に使用

### 施設種類と効果
| 施設 | コスト(Wood/Stone) | スコア | シナジー相手 |
|------|-----------------|--------|------------|
| コテージ | 2/1 | 15pt | 花壇 |
| 桟橋 | 1/2 | 10pt | レストラン |
| 花壇 | 1/0 | 10pt | コテージ・スパ |
| レストラン | 3/2 | 30pt | 桟橋・ホテル |
| 展望台 | 2/3 | 25pt | スパ |
| スパ | 3/2 | 30pt | 花壇・展望台 |
| マリーナ | 2/4 | 35pt | 桟橋 |
| ホテル | 5/5 | 50pt | レストラン |
| 灯台 | 3/4 | 20pt | 桟橋・マリーナ |
| カジノ | 8/6 | 80pt | ホテル |
| 水族館 | 5/5 | 60pt | マリーナ・桟橋 |

## 入力処理フロー
1. Mouse.current.leftButton.wasPressedThisFrame 検知
2. Mouse.current.position.ReadValue() でスクリーン座標取得
3. Camera.main.ScreenToWorldPoint() でワールド座標変換
4. Physics2D.OverlapPoint() でヒット判定
   - 島アイコンにヒット → 島を選択
   - スロットにヒット → 施設建設ダイアログ表示
   - 資源アイコンにヒット → 資源収穫
5. using UnityEngine.InputSystem;

## SceneSetup 構成方針

### Setup089v2_IslandHop.cs
- MenuItem: `[MenuItem("Assets/Setup/089v2 IslandHop")]`
- カメラ背景: 海の青（Color(0.1f, 0.3f, 0.6f)）
- orthographicSize: 6f
- 島を画面内に配置（ステージ1は中央1島、後のステージで増加）

## StageManager統合
- OnStageChanged購読でIslandManager.SetupStage()呼び出し
- ステージ遷移時に島数・利用可能施設・目標スコアを更新
- OnAllStagesCleared で最終クリア画面表示

## InstructionPanel内容
- title: "IslandHop"
- description: "島を開拓してリゾートアイランドを作ろう"
- controls: "島をタップして選択、建設スロットをタップして施設を配置"
- goal: "複数の島を開発して最高のリゾートを完成させよう"

## ビジュアルフィードバック設計
1. **施設建設成功**: スケールパルス（1.0→1.3→1.0、0.25秒）+ 金色フラッシュ
2. **シナジーコンボ発動**: 接続線エフェクト（2施設間を光の線が走る）+ サイズ強調（×1.2）
3. **資源収穫**: 小さなスプライトが弧を描いてUIに飛ぶ演出
4. **ゲームオーバー（資源枯渇）**: 赤いフラッシュ + カメラシェイク

## スコアシステム
- 施設建設: 施設固有スコア × コンボ乗算
- コンボ: 連続建設（1=×1.0, 2=×1.3, 3=×1.6, 4+=×2.0）
- シナジーボーナス: 隣接ペア1組=+20pt, 2組=+50pt, 3組=+100pt
- 目標スコア: Stage1=50pt, Stage2=120pt, Stage3=200pt, Stage4=300pt, Stage5=420pt

## ステージ別新ルール表
- **Stage 1**: 基本（1島、3施設種類、資源収集と建設のみ）
- **Stage 2**: 島間輸送追加（2島目解放、資源を別の島へ送れる「輸送ボタン」）
- **Stage 3**: 訪問客システム（UI下部に客の要望アイコン表示、対応施設建設で+ボーナス）
- **Stage 4**: 天候イベント（タイマーで嵐発生予告、防壁未設置=−30pt）
- **Stage 5**: リゾートランキング（累積スコアが420pt以上でクリア、隣接コンボ×1.5に強化）

## 判断ポイントの実装設計
- **資源配分**: 現在の資源量が「新島開拓閾値」以上になったタイミングでUIに「開拓できます！」通知
- 開拓選択 = 資源−10消費、即座に新島解放（高リターン）
- 施設建設 = 資源−2〜8消費、スコア直接獲得（安定）
- **施設選択UI**: 建設時に「収益型」「満足度型」2列で視覚的に選択促進

## レスポンシブ配置
- camSize = 6f, aspect ≈ 9/16
- **上部（HUD）**: y=4.5〜5.5 → ステージ/スコア表示
- **中央（ゲーム）**: y=0〜4.0 → 島マップ
- **下部（UI）**: Canvas下部 y=-3.5〜-5.5 → 施設選択・資源表示

### 島の配置（ワールド座標）
| ステージ | 島1 | 島2 | 島3 | 島4 | 島5 |
|---------|-----|-----|-----|-----|-----|
| 1 | (0, 1) | - | - | - | - |
| 2 | (-2, 1) | (2, 1) | - | - | - |
| 3 | (-3, 1.5) | (0, 2) | (3, 1.5) | - | - |
| 4 | (-3, 2) | (-1, 0.5) | (1, 0.5) | (3, 2) | - |
| 5 | (-3.5, 2) | (-1.5, 0) | (0, 2.5) | (1.5, 0) | (3.5, 2) |

（camSize=6f, 下部3u確保 → 上部プレイ領域 y=-3 to +5 のうち、島は y=0〜2.5 に収める）
