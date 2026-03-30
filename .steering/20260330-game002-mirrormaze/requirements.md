# 要件定義: Game002_MirrorMaze

## ゲーム概要

レーザー発射装置とゴール受光器が配置されたグリッド盤面に、鏡パーツをドラッグで移動・タップで回転させてレーザーをゴールまで誘導する反射パズル。

## コアループ

1. 鏡をタップして45度回転（/ ↔ \ の切り替え）
2. 鏡をスワイプして隣接セルに移動
3. レーザーの反射経路がリアルタイムで更新される
4. レーザーがゴールに到達したらクリア

## クリア条件

- レーザーがゴール受光器に到達する

## ゲームオーバー条件

- なし（パズルなので何度でもやり直せる）

## 必要な GameObject 一覧

| 名前 | 役割 | 主なコンポーネント |
|------|------|-------------------|
| GameManager | ゲーム全体制御 | MirrorMazeGameManager |
| MazeBoard | 盤面管理・入力処理 | MazeManager |
| MirrorPrefab | 鏡1つ | SpriteRenderer, BoxCollider2D, MirrorController |
| WallPrefab | 壁セル | SpriteRenderer |
| LaserSourcePrefab | レーザー発射器 | SpriteRenderer |
| GoalPrefab | ゴール受光器 | SpriteRenderer |
| LaserBeamPrefab | レーザー線分1つ | SpriteRenderer |
| EmptyCellPrefab | 空セル | SpriteRenderer |
| MirrorSlotPrefab | 鏡配置可能スロット | SpriteRenderer |
| Canvas | UI全体 | Canvas, CanvasScaler |
| MirrorMazeUI | UI制御 | MirrorMazeUI |

## 操作仕様

- **タップ（クリック）**: 鏡を45度回転
- **スワイプ（ドラッグ）**: 鏡を上下左右1マス移動（移動先が空またはスロットの場合のみ）
- **判定基準**: ドラッグ量30px未満 → タップ、30px以上 → スワイプ

## ステージ構成

- 3ステージ（ループ）
  - ステージ1: 鏡2つ、シンプルな直角反射
  - ステージ2: 鏡4つ、壁を迂回して2回反射
  - ステージ3: 鏡3つ、3回反射で複雑な経路

## 盤面

- 7×7 グリッド
- セルサイズ: 1.0 ワールド単位
