# タスクリスト: Game094_GravityPainter

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（cell.png, background.png）

## フェーズ2: C# スクリプト実装
- [x] GravityPainterGameManager.cs
- [x] PaintManager.cs（コアメカニクス・グリッド管理）
- [x] GravityPainterUI.cs

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup094_GravityPainter.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-01
- Color==比較の浮動小数点問題をintインデックス管理で回避 → 今後の標準パターンにすべき
- 全塗り済みでゲームが終わらない問題: painted==falseでもOnPaintDropped呼ぶ設計に変更
- GravityPainterUI の未使用SerializeField削除でコードを整理
- コンパイル・SceneSetup実行エラーなし
