# タスクリスト: Game047_SpinBalance

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（board.png, piece.png, background.png）

## フェーズ2: C# スクリプト実装
- [x] SpinBalanceGameManager.cs
- [x] BalanceManager.cs（入力・盤面回転・コマ管理）
- [x] Piece.cs（コマ制御・落下検知）
- [x] SpinBalanceUI.cs

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup047_SpinBalance.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- **実装完了日**: 2026-04-01
- **計画と実績の差分**: ほぼ計画通り。FallZoneの検知方法をTag→gameObject.name比較に変更。
- **レビュー対応**: レースコンディション修正（StopGame()追加）、PhysicsMaterial2Dキャッシュ化、未使用_boardSpriteフィールド削除
- **学んだこと**: Unity Update()の実行順が保証されないため、複数MonoBehaviour間の状態同期には明示的なStop/Start制御が必要
- **次回への改善**: タグ依存のコードは避け、gameObject.name比較を標準パターンとする
