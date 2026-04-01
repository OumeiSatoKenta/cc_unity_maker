# Design: Game031 BounceKing

## namespace
`Game031_BounceKing`

## スクリプト構成

### BounceKingGameManager.cs
- ゲーム状態: `Playing / Clear / GameOver`
- ライフ管理（初期値3）
- スコア管理
- `OnBallLost()` → ライフ減少 → 0になったらGameOver
- `OnAllBlocksDestroyed()` → Clear
- 参照取得: `[SerializeField]` で BreakoutManager, BounceKingUI

### BreakoutManager.cs
- ブロック配置・管理（8列×5行、プレハブなし→直接生成）
- ボール生成・管理
- パドル管理
- 入力処理（マウスX座標 → パドル移動）
- `Mouse.current.position.ReadValue()` でマウス位置取得
- `Camera.main.ScreenToWorldPoint()` でワールド座標変換
- 残りブロック数カウント → 0でGameManager.OnAllBlocksDestroyed()
- ボールが画面下端（Y < -5.5）を下回ったらGameManager.OnBallLost()
- `using UnityEngine.InputSystem;`

### BallController.cs
- Rigidbody2D（isKinematic=false, gravityScale=0）
- CircleCollider2D
- PhysicsMaterial2D（bounciness=1, friction=0）
- 一定速度を維持（速度が変わったら補正）
- `_speed = 7f` 固定
- ボールが下端を超えたかチェックは BreakoutManager が行う

### Block.cs
- SpriteRenderer（色付き）
- BoxCollider2D
- HP管理（1～3、行によって異なる）
- `Hit()` → HP減少 → 0でDestroy + スコア加算コールバック
- `[SerializeField] private BounceKingGameManager _gameManager` （GetComponentInParentで取得）

### Paddle.cs
- BoxCollider2D（物理移動なし、transform直接移動）
- X移動範囲を制限（-3.8f ～ 3.8f）
- 入力は BreakoutManager から直接 transform.position を変更するため、このスクリプトは当たり判定設定のみ

実際は Paddle.cs は不要（BreakoutManager が Paddle GameObject の transform を操作するため）

### BounceKingUI.cs
- `[SerializeField]` でTextMeshProUGUI/Panel参照
- `UpdateScore(int)`, `UpdateLives(int)`, `ShowClearPanel()`, `ShowGameOverPanel()`, `HidePanels()`

## 入力処理フロー
```
BreakoutManager.Update()
  → Mouse.current.leftButton.isPressed (ドラッグ中)
  → ScreenToWorldPoint で X 座標取得
  → paddleTransform.position.x を更新（Y, Z固定）
```

## 物理設定
- Layer: Default
- Ball: Rigidbody2D (Dynamic, gravityScale=0) + CircleCollider2D + PhysicsMaterial2D(bounciness=1,friction=0)
- Paddle: BoxCollider2D（Rigidbody2D isKinematic=true）
- Block: BoxCollider2D（Rigidbody2D isKinematic=true）
- 壁: BoxCollider2D × 3（左壁、右壁、上壁）

## クリア・ゲームオーバー状態遷移
```
Playing
  → (全ブロック破壊) → Clear
  → (ライフ=0) → GameOver

OnBallLost():
  lives--
  if lives == 0: GameOver
  else: ボールをパドル上部に再配置してリセット
```

## SceneSetup 構成方針（Setup031_BounceKing.cs）
- MenuItem: "Assets/Setup/031 BounceKing"
- カメラ: 背景色(紺系)、orthographic size=5.5
- スプライト読み込み: Resources/Sprites/Game031_BounceKing/
- 壁3本(Left/Right/Top)をBoxCollider2Dで作成
- Paddle: SpriteRenderer + BoxCollider2D + Rigidbody2D(kinematic)
- BreakoutManager: SerializedObjectで _paddleTransform, _gameManager 配線
- GameManager: SerializedObjectで _breakoutManager, _ui 配線
- 全フィールド配線リスト:
  - BreakoutManager._paddleTransform → Paddle.transform
  - BreakoutManager._ballPrefabSprite → (インライン取得)
  - BreakoutManager._blockSprites[0..4] → 5色スプライト
  - BreakoutManager._gameManager → GameManager
  - GameManager._breakoutManager → BreakoutManager
  - GameManager._ui → UI
  - UI各フィールド → Canvas下のTextMeshPro/Panel
