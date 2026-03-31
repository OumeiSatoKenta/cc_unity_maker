# 設計: Game030_FingerRacer

## スクリプト構成

全クラスは `namespace Game030_FingerRacer` で包む。

### FingerRacerGameManager.cs（オーケストレーター）
- ゲーム状態管理: Drawing / Racing / Clear / GameOver
- タイマー管理（Racing フェーズのみカウントダウン）
- RaceManager と FingerRacerUI への SerializeField 参照
- コールバック: OnRaceComplete(), OnTimeOut()
- `GetComponentInParent` は不使用（GameManager がルート）

### RaceManager.cs（コアメカニクス）
- **入力処理を一元管理**（Mouse.current.leftButton）
- Drawing フェーズ: ドラッグ中にパス点を List<Vector3> に追記
- パス点サンプリング: 前の点から 0.1ユニット以上離れた場合のみ追加（密度制御）
- 指を離した時: パス長チェック → 5ユニット未満なら描き直し → 問題なければ StartRacing()
- LineRenderer でパスをリアルタイム描画
- Racing フェーズ: 車を List<Vector3> に沿って移動
  - `_progress`（0〜1の float）でパス上の位置を管理
  - 隣接点間の角度差でカーブ係数を計算 → 速度調整
  - 車の向きを移動方向に合わせる（transform.up = 移動方向）
  - ゴール（progress >= 1）で GameManager.OnRaceComplete()
- `StartDrawing()` / `StartRacing()` / `StopGame()` の公開メソッド
- `Awake()` で `GetComponentInParent<FingerRacerGameManager>()`

### FingerRacerUI.cs（UI表示）
- フェーズ表示テキスト（"コースを描いてください" / "レース中！"）
- 残り時間表示（Racing フェーズのみ表示）
- クリアパネル（タイム表示）
- ゲームオーバーパネル（リトライボタン）
- メニューボタン
- `Awake()` で `GetComponentInParent<FingerRacerGameManager>()`

## 状態遷移
```
Drawing  --> [指を離す & パス長OK] --> Racing: RaceManager.StartRacing() → UI.ShowRacingPhase()
Racing   --> [ゴール到達]          --> Clear:   GameManager.OnRaceComplete() → UI.ShowClearPanel(time)
Racing   --> [時間切れ]            --> GameOver: GameManager.OnTimeOut() → UI.ShowGameOverPanel()
Clear/GameOver --> [リトライ]      --> Drawing: GameManager.RestartGame()
```

## 入力処理フロー（RaceManager.Update）
```
Drawing フェーズ:
  if mouse.leftButton.isPressed:
    worldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue())
    if dist(worldPos, lastPoint) > 0.1f:
      _pathPoints.Add(worldPos)
      UpdateLineRenderer()
  if mouse.leftButton.wasReleasedThisFrame:
    CheckPathAndStartRace()

Racing フェーズ:
  入力なし（車が自動走行）
```

## カーブ速度計算
```csharp
// 前後の点とのなす角（0=直線、180=U ターン）
float angle = Vector2.Angle(dir1, dir2);
float curveFactor = 1f - Mathf.Clamp01(angle / 180f) * 0.5f; // 0.5〜1.0
speed = baseSpeed * curveFactor;
```

## SceneSetup 構成方針
- カメラ: orthographic, size=5, 背景色=薄緑（草地: #7EC850）
- Canvas: ScreenSpaceOverlay, ScaleWithScreenSize 1920x1080
- Hierarchy: GameManager > RaceManager（child）、GameManager > FingerRacerUI（child）
- Car: SpriteRenderer, RaceManager.carTransform に SerializedObject で配線
- LineRenderer: RaceManager に AddComponent、color=灰色路面
- SerializedObject でフィールド接続

## 配線が必要な SerializeField 一覧
### RaceManager
- `_carTransform`: Car GameObject の Transform
- `_finishLineTransform`: FinishLine GameObject の Transform
- `_baseSpeed`: 3.0f
- `_minPathLength`: 5.0f
- `_timeLimit`: 30.0f（GameManager 側で管理するため参照用）

### FingerRacerGameManager
- `_raceManager`: RaceManager component
- `_ui`: FingerRacerUI component
- `_timeLimit`: 30.0f

### FingerRacerUI
- `_phaseText`: TMP フェーズ表示
- `_timeText`: TMP タイム表示
- `_clearPanel`: GameObject
- `_clearText`: TMP
- `_gameOverPanel`: GameObject
- `_gameOverText`: TMP
- `_retryButton`: Button
- `_menuButton`: Button
