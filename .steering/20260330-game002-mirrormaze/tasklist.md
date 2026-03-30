# タスクリスト: Game002_MirrorMaze

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（board_background, cell_empty, mirror, mirror_slot, wall, laser_source, goal, laser_beam）

## フェーズ2: C# スクリプト実装
- [x] MirrorController.cs（鏡の角度・位置保持、反射計算、表示更新）
- [x] MazeManager.cs（盤面生成、入力処理、鏡移動、レーザー経路計算・描画、ステージデータ）
- [x] MirrorMazeGameManager.cs（ゲーム状態管理、手数カウント、クリア判定、ステージ遷移）
- [x] MirrorMazeUI.cs（手数・ステージ表示、クリアパネル、ボタン制御）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup002_MirrorMaze.cs（シーン自動構成）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り
（実装完了後に記入）
