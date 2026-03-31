# 設計: Game029_MeteorShield

## スクリプト構成

全クラスは `namespace Game029_MeteorShield` で包む。

### MeteorShieldGameManager.cs（オーケストレーター）
- ゲーム状態管理: Playing / Clear / GameOver
- HP管理、タイマー管理
- ShieldManagerとMeteorShieldUIへの参照保持
- コールバック: OnMeteorHitStar(), OnTimeUp()

### ShieldManager.cs（コアメカニクス）
- Awake()で `GetComponentInParent<MeteorShieldGameManager>()` によりGameManagerを取得
- シールドのドラッグ移動（入力処理を一元管理）
- 隕石のスポーン・落下制御
- 隕石とシールドの衝突判定（手動距離ベース）
- 隕石と星の衝突判定
- 難易度スケーリング: `Mathf.Lerp(initialValue, maxValue, elapsed / clearTime)`
- シールドと衝突した隕石は即時Destroyする（弾き返し物理なし）

### MeteorShieldUI.cs（UI表示）
- HP表示（テキスト）
- 残り時間表示
- クリアパネル / ゲームオーバーパネル
- リスタート・メニューボタン

## 状態遷移
```
Playing --> [60秒経過] --> Clear: OnTimeUp() → ShieldManager.StopGame() → UI.ShowClearPanel()
Playing --> [HP=0]     --> GameOver: OnMeteorHitStar()でHP減算 → HP==0で ShieldManager.StopGame() → UI.ShowGameOverPanel()
```

## 入力処理フロー
1. Mouse.current.leftButton.isPressed でドラッグ検出
2. マウスX座標をワールド座標に変換
3. シールドのX座標をClampして移動（Y固定）

## 隕石スポーンロジック
- 画面上部のランダムX座標にスポーン
- 一定間隔で生成（時間経過で間隔短縮）
- 直線落下（Y方向のみ）
- 画面外に出たら自動破棄

## 衝突判定
- シールド: 隕石のY座標がシールドY付近 && X座標がシールド幅内 → 弾き返し（破壊）
- 星: 隕石のY座標が星Y付近 && X座標が星の範囲内 → HP-1、隕石破壊

## SceneSetup構成方針
- カメラ: orthographic, size=5, 背景色=深宇宙（暗い青黒）
- Canvas: ScreenSpaceOverlay, ScaleWithScreenSize 1920x1080
- Hierarchy: GameManager > ShieldManager
- スプライトPrefab: meteor, shield, star をPrefabUtility.SaveAsPrefabAsset
- SerializedObjectでフィールド接続
