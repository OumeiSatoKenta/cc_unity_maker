# 要件定義: Game003_GravitySwitch

## ゲーム概要

7×7グリッド盤面上のボールを、上下左右4方向の重力切り替えボタンで転がしてゴールに導くパズル。壁で止まり、ゴールに到達でクリア。

## コアループ

1. 4方向ボタン（上下左右）で重力方向を切り替える
2. ボールが重力方向に壁にぶつかるまで転がる
3. ボールがゴールに到達したらクリア

## クリア条件

- ボールがゴールセルに到達する

## ゲームオーバー条件

- なし（パズルなので何度でもやり直せる）

## 必要な GameObject 一覧

| 名前 | 役割 | 主なコンポーネント |
|------|------|-------------------|
| GameManager | ゲーム全体制御 | GravitySwitchGameManager |
| GravityBoard | 盤面管理 | GravityManager |
| BallPrefab | ボール | SpriteRenderer, BallController |
| WallPrefab | 壁セル | SpriteRenderer |
| GoalPrefab | ゴール | SpriteRenderer |
| FloorPrefab | 通常床 | SpriteRenderer |
| Canvas | UI全体 | Canvas, CanvasScaler |
| GravitySwitchUI | UI制御 | GravitySwitchUI |
| GravityButtons | 4方向ボタン群 | Button×4 |

## 操作仕様

- **4方向ボタン（UI）**: 上下左右の矢印ボタンをクリックして重力方向を切り替え
- ボールは重力方向に壁にぶつかるまでスライドする（1セルずつではなく一気に移動）

## ステージ構成

- 3ステージ（ループ）
  - ステージ1: シンプル、壁少なめ、3手程度でクリア
  - ステージ2: 壁が増え、5手程度
  - ステージ3: 複雑な経路、7手程度

## 盤面

- 7×7 グリッド
- セルサイズ: 1.0 ワールド単位

## 手数

- 重力切り替え1回 = 1手としてカウント、UI に表示（クリア条件には影響しない）
