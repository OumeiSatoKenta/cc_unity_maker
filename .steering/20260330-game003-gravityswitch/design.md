# 設計: Game003_GravitySwitch

## 名前空間

全スクリプトに `namespace Game003_GravitySwitch` を付与する。

## スクリプト構成

| クラス | ファイル | 責務 |
|--------|----------|------|
| GravitySwitchGameManager | GravitySwitchGameManager.cs | ゲーム状態管理、手数カウント、クリア判定、ステージ遷移 |
| GravityManager | GravityManager.cs | 盤面生成、重力切り替え処理、ボール移動計算、ステージデータ |
| BallController | BallController.cs | ボール1つのデータ保持（グリッド位置）、表示更新 |
| GravitySwitchUI | GravitySwitchUI.cs | 手数・ステージ表示、4方向ボタン、クリアパネル |

## クラス間依存関係

```
GravitySwitchGameManager
  ├── GravityManager（盤面操作を委譲）
  └── GravitySwitchUI（UI更新を委譲）

GravityManager
  ├── BallController（ボールの位置・表示を管理）
  └── GravitySwitchGameManager（重力切替イベント通知: OnGravityChanged）
```

## 盤面・ステージデータ設計

- グリッド: 7×7、CellType enum（Floor, Wall, Goal）
- ステージデータ: StageData struct
  - ballStartPos: Vector2Int（ボール初期位置）
  - goalPos: Vector2Int（ゴール位置）
  - walls: List\<Vector2Int\>（壁の座標リスト）
- 3ステージ分のハードコードデータ

## 重力移動ロジック

1. 重力方向（Vector2Int）を受け取る
2. ボールの現在位置から重力方向に1セルずつ進む
3. 次のセルが壁 or 盤面外なら停止
4. 次のセルがゴールなら停止してクリア判定
5. ボールの位置を更新し、ワールド座標に反映

## 入力処理（GravitySwitchUI に4方向ボタン）

- UIボタン経由でGravityManagerに方向を通知
- GravityManager.ApplyGravity(Vector2Int direction) を呼ぶ
- 結果をGameManagerに通知

## SceneSetup 構成方針

Setup001_BlockFlow / Setup002_MirrorMaze パターンに準拠:
1. NewScene → カメラ設定（orthographic, size 5, 暗い背景色）
2. 盤面背景スプライト配置
3. プレハブ群を生成（Ball, Wall, Goal, Floor）
4. GameManager + GravityManager 階層を構築
5. Canvas + UI要素（手数テキスト、ステージテキスト、4方向ボタン、メニューボタン、クリアパネル）
6. GravitySwitchUI の SerializeField を設定
7. EventSystem（InputSystemUIInputModule）
8. シーン保存 + BuildSettings 追加

## GameRegistry.json 更新

Resources/GameRegistry.json の Game003 エントリを implemented: true に変更する。
