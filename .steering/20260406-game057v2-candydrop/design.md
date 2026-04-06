# Design: Game057v2_CandyDrop

## namespace
`Game057v2_CandyDrop`

## スクリプト構成

### CandyDropGameManager.cs
- ゲーム状態管理（Idle / Playing / StageClear / GameClear / GameOver）
- StageManager / InstructionPanel 統合
- スコア・コンボ管理
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] CandySpawner _spawner`
- `[SerializeField] TowerChecker _towerChecker`
- `[SerializeField] CandyDropUI _ui`

### CandySpawner.cs
- キャンディの生成・落下制御
- 落下中キャンディの左右移動（タッチ入力処理）
- `SetupStage(StageManager.StageConfig config, int stageNumber)` で難易度パラメータ適用
- キャンディ形状: Circle / Square / Triangle / Star (PhysicsShape2D or collider設定)
- 物理演算: Rigidbody2D + Collider2D
- 溶けるキャンディ（Stage4+）: 5秒後にDestroyする
- 巨大キャンディ（Stage5）: スケール1.5倍・重量増加
- レスポンシブ配置:
  ```csharp
  float camSize = Camera.main.orthographicSize;
  float camWidth = camSize * Camera.main.aspect;
  float groundY = -camSize + 1.5f;  // 下部UI上に土台
  float spawnY = camSize - 0.5f;    // 上端から生成
  float halfWidth = camWidth - 0.5f; // 画面内に収める
  ```

### TowerChecker.cs
- 積み上がったキャンディの最高高さを監視
- 目標ラインとの比較でクリア判定
- 崩壊検出（高さが急激に下がった場合ゲームオーバー）
- 物理的安定判定

### CandyDropUI.cs
- スコア表示
- ステージ表示「Stage X / 5」
- 高さゲージ（現在高さ / 目標高さ）
- 次のキャンディプレビュー
- コンボ表示
- ステージクリアパネル・ゲームオーバーパネル・最終クリアパネル

## InstructionPanel内容
```
title: "CandyDrop"
description: "落下するキャンディを積み上げよう！"
controls: "ドラッグで位置を決めてタップで落とす"
goal: "目標ラインまでキャンディを積み上げよう"
```

## ステージ別パラメータ
| Stage | speedMultiplier | countMultiplier | complexityFactor |
|-------|----------------|----------------|-----------------|
| 1     | 0.8            | 1.0            | 0.0 (丸・四角) |
| 2     | 0.9            | 1.0            | 0.3 (三角追加) |
| 3     | 1.0            | 1.0            | 0.6 (星型+振動台) |
| 4     | 1.0            | 1.0            | 0.8 (溶けるキャンディ) |
| 5     | 1.1            | 1.0            | 1.0 (巨大+風) |

## ビジュアルフィードバック設計
1. **キャンディ着地時パルス**: 着地したキャンディが scale 1.0→1.2→1.0 に0.2秒でアニメーション
2. **色ボーナス発生時フラッシュ**: 同色3個隣接時にSpriteRendererを白→元色に0.3秒フラッシュ
3. **崩壊時カメラシェイク**: ゲームオーバー時に0.4秒カメラシェイク
4. **コンボ発生時スケール**: UIテキストが1.5倍に拡大→縮小

## スコアシステム
- 着地成功: 50pt
- 色ボーナス（同色3隣接）: 300pt
- コンボ（連続安定着地）: コンボ数 × 20pt、5個ごとに倍率+0.5
- クリアボーナス: 1000pt
- 効率ボーナス: 最少個数クリア → ×2.0

## SceneSetup 構成方針
- GameManager, CandySpawner, TowerChecker を順次生成
- 土台（Ground）: SpriteRenderer + BoxCollider2D
- 壁（左右）: 落ちたキャンディをキャッチするために左右に透明の壁
- 目標ラインインジケーター: LineRenderer or 細いSpriteオブジェクト
- Canvas: ステージ/スコア/コンボ/クリアパネル/ゲームオーバーパネル
- InstructionPanel + StageManager の生成と配線

## 台の振動（Stage3以降）
- TowerChecker に振動制御を持たせる
- `StartCoroutine(OscillateGround())` でsinウェーブで左右に動かす
- Stage3+: amplitide=0.3, period=2.0s

## 溶けるキャンディ（Stage4以降）
- CandyController コンポーネントで溶け時間カウント
- `complexityFactor > 0.7` のとき、一定確率(30%)で溶けるキャンディを生成
- 残り時間に応じてtransparency変化（見た目フィードバック）

## 横風（Stage5）
- `complexityFactor >= 1.0` のとき有効
- すべてのキャンディRigidbody2Dに定期的にforce.xを加える
- UIに風方向インジケーターを表示
