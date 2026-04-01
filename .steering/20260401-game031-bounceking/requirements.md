# Requirements: Game031 BounceKing

## ゲーム概要
- ID: 031
- タイトル: BounceKing
- カテゴリ: action / S サイズ
- シーン名: 031_BounceKing
- 説明: ボールをバウンドさせてブロックを全破壊するブレイクアウト

## コアループ
1. パドルを左右にドラッグして動かす（マウスドラッグ）
2. ボールを跳ね返してブロックに当てる
3. 全ブロック破壊でステージクリア

## クリア条件
- 画面上のブロックを全て破壊する

## ゲームオーバー条件
- ボールが画面下端を超えたとき（ライフ制: 3機）
- 全ライフ消失でゲームオーバー

## 操作仕様
- マウス左ドラッグで パドルを左右に移動
- Mouse.current.position.ReadValue() で現在位置取得
- パドルのX座標をマウスのX座標に追従させる

## 必要な GameObject 一覧
| 名前 | 役割 | スクリプト |
|------|------|-----------|
| GameManager | ゲーム状態管理 | BounceKingGameManager |
| BreakoutManager | ボール・ブロック・パドル管理 | BreakoutManager |
| Paddle | パドル表示・衝突 | Paddle |
| Ball(clone) | ボール（動的生成） | BallController |
| Block(clone) | ブロック（動的生成） | Block |
| Canvas | UI | BounceKingUI |
| Camera | カメラ | - |

## ステージ構成
- ブロック: 8列 × 5行 = 40個
- ブロックの色: 行ごとに4色（赤・橙・黄・緑・青）
- パドル: 画面下部（Y = -4.0）
- 壁: 左右上端 にEdgeCollider2D（壁なし→BoxCollider）
- ボール: ゲーム開始時に1個生成、パドル上部からスタート
