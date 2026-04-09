# Design: Game099v2 TouchMemory (Remake)

## namespace
`Game099v2_TouchMemory`

## スクリプト構成

### TouchMemoryGameManager.cs
- ゲーム状態管理（Playing / StageClear / AllClear / GameOver）
- StageManager・InstructionPanel統合
- スコア・コンボ管理
- TouchMemoryManager への SetupStage 呼び出し
- `[SerializeField] StageManager _stageManager`
- `[SerializeField] InstructionPanel _instructionPanel`
- `[SerializeField] TouchMemoryManager _touchMemoryManager`
- `[SerializeField] TouchMemoryUI _ui`
- コンボカウンター、スコア乗算実装

### TouchMemoryManager.cs
- コアメカニクス（パターン生成・再生・入力受付・判定）
- パネル動的生成・配置（カメラ orthographicSize から動的計算）
- 入力処理: Mouse.current.leftButton.wasPressedThisFrame + Physics2D.OverlapPoint
- ステージ設定: `SetupStage(StageManager.StageConfig config, int stageIndex)`
- ラウンド進行: `StartRound(int patternLength)`
- パターン再生: コルーチンで順番に点灯 → 入力受付フェーズへ
- 入力判定: 正解/不正解でGameManagerに通知
- `_isActive` ガードで入力フェーズ外のタップを無視
- 動的生成リソース（Texture2D, Sprite等）は OnDestroy() でクリーンアップ
- ビジュアルフィードバック:
  - 点灯時: スケールパルス（1.0→1.3→1.0, 0.2秒）+ 明るい色
  - 正解タップ時: グリーンフラッシュ（0.15秒）
  - 不正解タップ時: 赤フラッシュ + カメラシェイク（0.3秒）

### TouchMemoryUI.cs
- HUD表示: ステージ、スコア、ラウンド
- コンボ表示（一時的）
- Stage Clear / Game Over / All Clear パネル管理
- ボタンイベント処理

## 盤面・ステージデータ設計

### StageManager.StageConfig 活用
```
Stage 1: speedMultiplier=1.0f, panelCount=4, startPatternLength=2, roundCount=5, colorChange=false, reverseEven=false
Stage 2: speedMultiplier=0.8f (速い), panelCount=4, startPatternLength=3, roundCount=5, colorChange=false, reverseEven=false
Stage 3: speedMultiplier=0.8f, panelCount=6, startPatternLength=3, roundCount=5, colorChange=false, reverseEven=false
Stage 4: speedMultiplier=0.7f, panelCount=6, startPatternLength=4, roundCount=5, colorChange=true, reverseEven=false
Stage 5: speedMultiplier=0.6f, panelCount=9, startPatternLength=4, roundCount=5, colorChange=true, reverseEven=true
```

StageConfig.customParams で追加パラメータを渡す:
- `panelCount`: int（パネル数）
- `startPatternLength`: int（そのステージの最初のパターン長）
- `roundCount`: int（ラウンド数）
- `colorChange`: bool（毎ラウンドパネル色が変わるか）
- `reverseEven`: bool（偶数ラウンドは逆順入力か）

## パネル配置（レスポンシブ）

```csharp
float camSize = Camera.main.orthographicSize;  // 6f
float camWidth = camSize * Camera.main.aspect;
float topMargin = 1.2f;   // HUD領域
float bottomMargin = 2.8f; // Canvas UI領域
float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
// パネルグリッド: Mathf.CeilToInt(Mathf.Sqrt(panelCount)) で列数計算
// セルサイズ: Min(availableHeight / rows, camWidth * 2f / cols, maxCell)
```

4パネル: 2x2グリッド
6パネル: 2x3 or 3x2グリッド
9パネル: 3x3グリッド

## 入力処理フロー
1. パターン再生コルーチン実行（_isActive=false で入力無視）
2. 再生完了後 _isActive=true、_inputPhase=true
3. Mouse.current.leftButton.wasPressedThisFrame で毎フレームチェック
4. Physics2D.OverlapPoint(mouseWorldPos) でパネル判定
5. 正しい順序: 正解エフェクト、全入力完了でラウンドクリア通知
6. 誤った順序: 不正解エフェクト、GameManager.OnMissed() 通知

## SceneSetup 構成方針

### ファイル: `Setup099v2_TouchMemory.cs`
- MenuItem: `"Assets/Setup/099v2 TouchMemory"`
- カメラ設定: backgroundColor = #1A0A2E（濃い紫）、orthographicSize=6
- スプライト読み込み: `Assets/Resources/Sprites/Game099v2_TouchMemory/`
- GameManager GameObject に TouchMemoryGameManager + StageManager + StageConfig x5
- TouchMemoryManager は別の GameManager 子オブジェクト
- TouchMemoryUI は Canvas の子

### StageManager 配線（GameManagerの子）
- 5つの StageConfig を設定:
  ```
  Stage 1: speedMultiplier=1.0, customData="4,2,5,false,false"
  Stage 2: speedMultiplier=0.8, customData="4,3,5,false,false"
  Stage 3: speedMultiplier=0.8, customData="6,3,5,false,false"
  Stage 4: speedMultiplier=0.7, customData="6,4,5,true,false"
  Stage 5: speedMultiplier=0.6, customData="9,4,5,true,true"
  ```
- customData を TouchMemoryManager.ParseStageData() でパース

### InstructionPanel 配線
- title: "TouchMemory"
- description: "光るパターンを記憶して再現しよう"
- controls: "光った順番にパネルをタップ"
- goal: "できるだけ多くのラウンドをクリアしよう"

### UI 構成
- HUD上部: Stage X/5（左上）、Score（中央上）、Round（右上）
- コンボテキスト（中央、一時的表示）
- ステージクリアパネル（中央パネル、「次のステージへ」ボタン）
- 全クリアパネル（中央パネル）
- ゲームオーバーパネル（中央パネル、「再挑戦」ボタン）
- メニューへ戻るボタン（最下部 Y=10~20）
- ?ボタン（右下）

## ビジュアルフィードバック設計
1. **点灯演出（パターン再生）**: パネルをスケールパルス（1.0→1.3→1.0, 0.15秒）+ emit色（白/明るい色）
2. **正解タップ**: グリーンフラッシュ（Color(0.3f,1f,0.3f)→元色、0.15秒）
3. **不正解タップ**: 赤フラッシュ（Color(1f,0.2f,0.2f)、0.2秒）+ カメラシェイク（振幅0.1f、0.3秒）
4. **コンボ表示**: コンボ数テキストをスケールアニメーションで表示（1.5秒後非表示）

## スコアシステム
- 基本ラウンドスコア: `100 × (stageIndex + 1) × patternLength`
- コンボ乗算: 連続正解 n 回で ×(1.0 + n × 0.1)（最大×2.0）
- 即答ボーナス（2秒以内に全入力完了）: +50%
- コンボリセット: ミス時に0リセット

## Buggy Code 防止チェックリスト
- Physics2D.OverlapPoint でパネル判定（タグ比較不使用）
- _isActive ガードで入力フェーズ外タップ無視
- パネル生成のSpriteは AssetDatabase 経由で保存（SceneSetup内）
- OnDestroy() で生成済みリソースクリーンアップ
- ワールド座標は Camera.main.orthographicSize から動的計算（固定値禁止）
- 下部マージン 2.8f でCanvas UIと重複防止
