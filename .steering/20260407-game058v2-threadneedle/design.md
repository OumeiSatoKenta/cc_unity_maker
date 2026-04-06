# Design: Game058v2_ThreadNeedle

## namespace
`Game058v2_ThreadNeedle`

## スクリプト構成

### ThreadNeedleGameManager.cs
- 役割: ゲーム全体の状態管理、スコア・コンボ管理
- StageManager統合、InstructionPanel統合
- GameState: Idle / Playing / StageClear / GameClear / GameOver
- フィールド:
  - `[SerializeField] StageManager _stageManager`
  - `[SerializeField] InstructionPanel _instructionPanel`
  - `[SerializeField] NeedleController _needleController`
  - `[SerializeField] ThreadNeedleUI _ui`
- Start(): InstructionPanel.Show() → OnDismissed += StartGame
- StartGame(): StageManager購読 → StartFromBeginning()
- OnStageChanged(int): NeedleController.SetupStage(config, stageNumber)
- OnAllStagesCleared(): GameClear表示
- AddScore(int baseScore, bool isCenter): コンボ・乗算適用
- OnMiss(): ミスカウント+1、3回でGameOver
- OnStageClear(): StageClear表示

### NeedleController.cs
- 役割: 針の揺れアニメーション制御、糸射出判定、ラウンド管理
- 針オブジェクトをSin波で左右揺れ
- SetupStage(StageConfig config, int stageNumber): 揺れ速度・針穴サイズ・回転有無・2穴モードを設定
- Update(): 針揺れ計算 + 回転（ステージ3+）
- OnTap(): 糸射出→判定実行
  - 針穴中心からの距離でCENTER/SUCCESS/MISS判定
  - CENTER閾値: 穴サイズの20%以内
  - SUCCESS閾値: 穴サイズの50%以内
- コルーチン: 糸飛翔アニメーション（0.2秒で伸びる）
- ラウンド管理: 成功でroundCount++、目標ラウンドでStageClear
- **レスポンシブ配置**: Camera.main.orthographicSizeから針位置を動的計算
  - 針Y位置: camSize * 0.2（上側）
  - 糸発射位置: -camSize * 0.5（下側）

### ThreadNeedleUI.cs
- 役割: UI表示管理
- Init(ThreadNeedleGameManager gm)
- UpdateScore(int score)
- UpdateCombo(int combo, float multiplier)
- UpdateMiss(int missCount)
- UpdateRound(int current, int total)
- OnStageChanged(int stageNumber)
- ShowJudgement(string text, Color color): CENTER!/SUCCESS/MISS表示（1秒後消える）
- ShowStageClear(int score)
- ShowGameClear(int totalScore)
- ShowGameOver(int score)

## 針の揺れ設計
```
angle = Mathf.Sin(Time.time * swingSpeed) * maxAngle
needle.transform.rotation = Quaternion.Euler(0, 0, angle)
```
- ステージ別パラメータ:
  - Stage1: swingSpeed=1.0, maxAngle=30, holeSize=0.8, rotation=false
  - Stage2: swingSpeed=1.5, maxAngle=40, holeSize=0.6, rotation=false
  - Stage3: swingSpeed=2.0, maxAngle=35, holeSize=0.45, rotation=true(rotSpeed=45)
  - Stage4: swingSpeed=2.5, maxAngle=40, holeSize=0.35, rotation=false, dualHole=true
  - Stage5: swingSpeed=3.0, maxAngle=50, holeSize=0.25, rotation=true, irregular=true

## InstructionPanel内容
- title: "ThreadNeedle"
- description: "揺れる針穴に糸を通そう！"
- controls: "針穴が正面に来たらタップして糸を射出"
- goal: "全ラウンドの針穴に糸を通してステージクリア！"

## ビジュアルフィードバック設計
1. **成功時ポップ**: 針穴スケール 1.0→1.4→1.0（0.2秒）+ CENTER時は黄金色フラッシュ
2. **ミス時赤フラッシュ**: SpriteRenderer.colorを赤に0.1秒→白に戻す + カメラシェイク
3. **糸飛翔アニメーション**: 糸がY方向に伸びながら針穴に到達（0.15秒）
4. **判定テキスト**: CENTER! (黄), SUCCESS (緑), MISS (赤) がポップアップして消える

## スコアシステム
- CENTER判定: 200pt
- SUCCESS判定: 100pt
- ×コンボ乗算（5連続=×1.5, 10連続=×2.0）
- ミスでコンボリセット
- ステージクリアボーナス: +500pt

## ステージ別新ルール表
- Stage 1: 基本ルールのみ（遅い規則的揺れ）チュートリアル的
- Stage 2: 揺れ速度1.5倍 + 揺れ幅拡大（パターン崩し）
- Stage 3: 針穴が回転（縦横判定）追加
- Stage 4: 2連続穴（同一タップで2穴通過必要、穴を通るたびに次の穴が出現）
- Stage 5: 全要素+不規則揺れ（SinとCosを合成した複合波形）

## SceneSetup構成方針（Setup058v2_ThreadNeedle.cs）
- MenuItem: "Assets/Setup/058v2 ThreadNeedle"
- Camera: orthographicSize=6, background=#1A1A2E（ダーク）
- 針オブジェクト: Needle GameObject（SpriteRenderer）
  - NeedleHole: 子GameObject（判定用BoxCollider2D）
- ThreadLaunchPoint: 下部固定（Y=-3）
- GameManager + 子にStageManager
- Canvas + InstructionPanel配線
- StageManagerのStageConfigは自動初期化（デフォルト5ステージ）

## レスポンシブ配置
```csharp
float camSize = Camera.main.orthographicSize; // 6.0
float camWidth = camSize * Camera.main.aspect;
// 針: Y = camSize * 0.3 (上方)
// 糸発射点: Y = -camSize * 0.5 (下方)
// HUD: anchorMax.y=1, Y offset=-40
// ボタン: anchorMin.y=0, Y offset=60-80
```

## 判断ポイントの実装設計
- 針穴のSin波ゼロ交差点（最も中央）がCHANCE時間
- プレイヤーが「今」のタイミングか「次の周期」を待つかの選択
- CENTER判定半径 = holeSize * 0.2
- SUCCESS判定半径 = holeSize * 0.5
- ミス判定 = 上記以外

## Buggy Code防止
- Physics2D比較はgameObject.name使用
- `_isActive`ガードで多重Update防止
- Texture2D/Sprite は OnDestroy でクリーンアップ
- ワールド座標はorthographicSizeから動的計算（固定値禁止）
