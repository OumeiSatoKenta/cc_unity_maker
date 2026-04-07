# タスクリスト: Game040v2_DashDungeon

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, floor, wall, player, enemy, spike, exit, ice, warp_a, warp_b）

## フェーズ2: C# スクリプト実装
- [x] DashDungeonGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] DashDungeonMechanic.cs（グリッド生成・プレイヤー移動・タイル判定・5ステージ対応）
- [x] DashDungeonUI.cs（HUD・HP表示・手数表示・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup040v2_DashDungeon.cs（InstructionPanel・StageManager配線・4方向ボタン配置）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

**実装完了日**: 2026-04-06

**計画と実績の差分**:
- 計画通り4フェーズすべて完了
- コードレビューで[必須]バグ4件を発見・修正: Camera.mainのnullガード、ClearGrid()でのStopAllCoroutines()、PlaceOneTile()のsentinel値(-1,-1)、HandleLanding()のwarp後再帰呼び出し

**学んだこと**:
- BFS最短手数計算はSimulateSlide()を再利用することで実装が簡潔になる
- Warpのような転送ギミックは、着地後の再判定（再帰HandleLanding）が必要
- Pillow生成時のy座標計算は2倍スケールとオフセットの組み合わせでバグが出やすい

**次回への改善提案**:
- PlaceOneTile()の失敗sentinel値はnullable Vector2Int?の方が意図が明確
- ステージデータはScriptableObjectで外部化すると難易度調整が容易
