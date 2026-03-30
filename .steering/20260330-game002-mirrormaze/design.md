# 設計: Game002_MirrorMaze

## 名前空間

全スクリプトに `namespace Game002_MirrorMaze` を付与する。

## スクリプト構成

| クラス | ファイル | 責務 |
|--------|----------|------|
| MirrorMazeGameManager | MirrorMazeGameManager.cs | ゲーム状態管理、手数カウント、クリア判定、ステージ遷移 |
| MazeManager | MazeManager.cs | 盤面生成、入力処理（一元管理）、鏡移動、レーザー経路計算・描画 |
| MirrorController | MirrorController.cs | 鏡1つのデータ保持（角度・位置）、反射方向計算、表示更新 |
| MirrorMazeUI | MirrorMazeUI.cs | 手数・ステージ表示、クリアパネル、ボタン制御 |

## クラス間依存関係

```
MirrorMazeGameManager
  ├── MazeManager（盤面操作を委譲）
  └── MirrorMazeUI（UI更新を委譲）

MazeManager
  ├── MirrorController[]（鏡の配列を管理）
  └── MirrorMazeGameManager（操作イベント通知: OnMirrorMoved, OnMirrorRotated）
```

## 盤面・ステージデータ設計

- グリッド: 7×7、CellType enum（Empty, Wall, LaserSource, Goal, Mirror, MirrorSlot）
- ステージデータ: StageData struct
  - laserPos: Vector2Int（レーザー発射器のグリッド座標）
  - laserDir: Vector2Int（初期方向、例: (1,0) = 右）
  - goalPos: Vector2Int（ゴールのグリッド座標）
  - walls: List\<Vector2Int\>（壁の座標リスト）
  - mirrorSlots: List\<Vector2Int\>（鏡配置可能スロットの座標リスト）
  - mirrors: List\<MirrorPlacement\>（初期配置鏡の位置・角度ペア）
- 3ステージ分のハードコードデータ

## レーザー経路計算

1. レーザー発射位置・方向からスタート
2. 1セルずつ進行方向に進む
3. 鏡に当たったら MirrorController.Reflect(Vector2Int inDir): Vector2Int で方向転換
   - / 鏡(type=0): (dx,dy)→(dy,dx) 例: 右(1,0)→上(0,1)
   - \ 鏡(type=1): (dx,dy)→(-dy,-dx) 例: 右(1,0)→下(0,-1)
4. 壁に当たるか盤面外で終了
5. ゴールに到達したら goalReached = true
6. 各セグメントを LaserBeamPrefab で描画（中点配置、回転、スケール調整）

## 入力処理フロー（MazeManager に一元管理）

1. Mouse.current.leftButton.wasPressedThisFrame でクリック開始検出
2. Physics2D.OverlapPoint で鏡をヒットテスト
3. wasReleasedThisFrame でリリース検出
4. ドラッグ量 < 30px → タップ → Rotate45()
5. ドラッグ量 >= 30px → スワイプ → TryMoveMirror()
6. 操作後 GameManager に通知 → レーザー更新 → クリア判定

## SceneSetup 構成方針

Setup001_BlockFlow.cs のパターンに準拠:
1. NewScene → カメラ設定（orthographic, size 5, 暗い背景色）
2. 盤面背景スプライト配置
3. プレハブ群を生成（Mirror, Wall, LaserSource, Goal, LaserBeam, EmptyCell, MirrorSlot）
4. GameManager + MazeManager 階層を構築、SerializedObject でプレハブ参照を設定
5. Canvas + UI 要素（手数テキスト、ステージテキスト、メニューボタン、クリアパネル）
6. MirrorMazeUI の SerializeField を設定
7. EventSystem（InputSystemUIInputModule）
8. シーン保存 + BuildSettings 追加
