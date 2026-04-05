# タスクリスト: Game030v2_FingerRacer

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成

## フェーズ2: C# スクリプト実装
- [x] FingerRacerGameManager.cs（StageManager・InstructionPanel統合）
- [x] CourseDrawer.cs（コース描画・チェックポイント・障害物管理）
- [x] CarController.cs（車の移動・ブースト・コースアウト判定）
- [x] FingerRacerUI.cs（HUD・各パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup030v2_FingerRacer.cs（全フィールド配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り

**実装完了日**: 2026-04-06

**計画と実績の差分**:
- RivalCarController の GameManager 逆方向ワイヤリング（`_rivalCarController` フィールド）が初期実装で欠落 → コードレビューで発見・修正
- `CheckpointMarker`/`SandAreaMarker` が `gameObject.name` 文字列比較を使用 → `GetComponent<CarController>()` に修正
- `CourseDrawer._carController` フィールドがファイル末尾に孤立宣言 → 先頭フィールド群に移動

**学んだこと**:
- 双方向ワイヤリング（A→B と B→A）はどちらか片方が漏れやすい。SceneSetup と GameManager の両方に存在するか必ず確認すること
- 文字列比較での衝突判定はサイレント故障リスクが高い。コンポーネント存在チェックを標準パターンにする

**次回への改善提案**:
- SceneSetup レビューチェックリストに「逆方向参照（B→A）の配線確認」を追加
- Trigger判定は全て `GetComponent<T>() != null` パターンを標準化する
