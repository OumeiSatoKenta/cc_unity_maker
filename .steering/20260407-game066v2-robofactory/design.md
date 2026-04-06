# Design: Game066v2 RoboFactory

## スクリプト構成

### namespace: `Game066v2_RoboFactory`

| クラス | ファイル | 役割 |
|-------|---------|------|
| RoboFactoryGameManager | RoboFactoryGameManager.cs | 薄いオーケストレータ。InstructionPanel・StageManager統合 |
| FactoryManager | FactoryManager.cs | コアメカニクス。ロボット管理・資源収集・建設・研究 |
| RoboFactoryUI | RoboFactoryUI.cs | UI管理。資源表示・ボタン・パネル |

## 盤面・ステージデータ設計

### 資源システム
- 3種類: 鉱石 (Ore)、エネルギー (Energy)、パーツ (Parts)
- 各ステージでロボットが自動収集 (autoCollect coroutine)

### ロボット種類
- WorkerBot (作業ロボ): 万能だが低効率。初期から1体
- MinerBot (採掘ロボ): 鉱石収集特化。ステージ2解放
- BuilderBot (建設ロボ): 自動建設。ステージ3解放
- RepairBot (修理ロボ): 故障修理。ステージ3解放
- PowerBot (発電ロボ): エネルギー生成。ステージ4解放
- AIBot (AIロボ): 全効率3倍。ステージ5解放

### 建物種類（3×3グリッド・最大9マス）
- House: 基本居住施設 → 都市レベル+1
- Factory: 製造施設 → パーツ生産向上
- PowerPlant: 発電所 → エネルギー生産向上
- Lab: 研究施設 → 研究速度向上
- MiningDrill: 採掘場 → 鉱石収集向上
- AICore: AIコア → ステージ5解放、AI効率向上

### 5ステージ設定 (StageConfig)
| Stage | targetLevel | speedMult | countMult | complexFactor |
|-------|------------|-----------|-----------|---------------|
| 1 | 5 | 1.0 | 1.0 | 1.0 |
| 2 | 15 | 1.5 | 1.5 | 1.2 |
| 3 | 30 | 2.0 | 2.0 | 1.5 |
| 4 | 50 | 2.5 | 2.5 | 2.0 |
| 5 | 100 | 3.0 | 3.0 | 3.0 |

## 入力処理フロー

- 全ボタン操作はUIボタンのOnClickで処理（UIイベント駆動）
- タップ修理（ステージ3以降）はCanvas上のRepairNotificationパネルのボタンで

## SceneSetup 構成方針

- Setup066v2_RoboFactory.cs
- MenuItem: "Assets/Setup/066v2 RoboFactory"
- Camera: orthographic size 6, dark background (#1A1A2E)
- 3×3グリッドは世界座標で動的生成
- Canvas: ScreenSpaceOverlay, 1080×1920

## StageManager 統合

- `SetConfigs()` で5ステージ設定を渡す
- `OnStageChanged` → `FactoryManager.SetupStage(config, stageIndex)`
  - 資源収集速度リセット
  - 新ロボット種類解放
  - 故障・エネルギー管理ON/OFF
- `OnAllStagesCleared` → 最終クリアパネル表示

## InstructionPanel 内容

- title: "RoboFactory"
- description: "ロボットを作って都市を建設しよう"
- controls: "ボタンでロボット製造・建設・研究を指示"
- goal: "都市レベル目標を達成してステージクリア"

## ビジュアルフィードバック設計

1. **建設完了時**: 建物スプライトのスケールパルス (1.0 → 1.3 → 1.0、0.2秒) + 黄色フラッシュ
2. **ロボット故障時**: 該当ロボットアイコンの赤点滅 + 警告パネルのシェイク
3. **コンボ時**: コンボテキストのスケールアップ + 色グラデーション変化
4. **資源不足時**: 関連ボタンの赤フラッシュ

## スコアシステム

- 都市レベルアップごとにスコア加算
- コンボ乗算: 連続建設成功で 1x→1.5x→2x→3x
- 30秒無建設でコンボリセット
- AIロボ稼働中は全スコア×2

## ステージ別新ルール表

| Stage | 新要素 |
|-------|--------|
| 1 | 作業ロボ1体、手動建設指示のみ |
| 2 | 採掘ロボ解放、自動資源収集coroutine起動、技術ツリー表示 |
| 3 | 建設ロボ解放（自動建設）、ロボット故障イベントcoroutine |
| 4 | エネルギー管理ON、PowerBot解放、エネルギー不足→ロボット停止 |
| 5 | AIロボ解放（3倍効率）、全要素複合チャレンジ |

## 判断ポイントの実装設計

- トリガー: 資源が建設/製造/研究いずれかのコスト以上に達した瞬間
- 各選択の報酬: ロボット製造→収集速度UP、建設→都市LvUP、研究→新機能解放
- ペナルティ: 故障修理を怠ると収集効率-50%、エネルギー不足で全ロボット停止

## レスポンシブ配置

```csharp
float camSize = Camera.main.orthographicSize;  // 6.0
float camWidth = camSize * Camera.main.aspect; // ~3.37
float topMargin = 1.5f;  // HUD領域
float bottomMargin = 3.0f;  // UIボタン領域
float availableHeight = camSize * 2f - topMargin - bottomMargin;  // 7.5
float cellSize = Mathf.Min(availableHeight / 3f, camWidth * 2f / 3f, 1.8f);
// 3×3グリッドを中央に配置
```

## Buggy Code 防止チェック

- Physics2D tag比較: `gameObject.name` 使用（比較不要の場合はlayer除外）
- 各Managerの `Update()` に `_isActive` ガード
- 動的生成Texture2D/Sprite は `OnDestroy()` でクリーンアップ
- 固定座標禁止: 全配置をcamera.orthographicSizeから動的計算
