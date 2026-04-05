# タスクリスト: Game027v2_DotDodge

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Player, DotNormal, DotChaser, DotExpander, SafeZone）

## フェーズ2: C# スクリプト実装
- [x] DotDodgeGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] PlayerController.cs（ドラッグ追従、ニアミス検出、境界クランプ）
- [x] DotSpawner.cs（ドット生成・管理・5ステージ難易度対応）
- [x] DotDodgeUI.cs（HUD・各種パネル表示制御）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup027v2_DotDodge.cs（全オブジェクト配線・InstructionPanel・StageManager含む）

## フェーズ4: GameRegistry.json 更新
- [x] 027 remakeエントリーの implemented: true に変更

## 実装後の振り返り

**実装完了日**: 2026-04-05

**計画と実績の差分**:
- 計画通り全4フェーズを完了
- コードレビューで2件の[必須]バグを発見・修正（ExpanderスケールロジックとScreenShakeLoopの絶対座標バグ）
- DotBehaviorをDotSpawner.cs内のインナークラスとして実装（[推奨]指摘があったが動作上問題なし）

**発見・修正したバグ**:
1. **Expanderスケールバグ**: `_currentScale > _currentScale * 2f` は常にfalseで上限が効かなかった。`_initialScale`フィールドを追加し、`Mathf.Min(_currentScale * 1.2f, 2f)` + `Vector3.one * (_initialScale * _currentScale)` で修正
2. **ScreenShakeLoopドリフトバグ**: 毎フレーム現在位置の `.z` を読むと、シェイクがどんどん蓄積する。ループ前に `camZ` をキャッシュして常に `new Vector3(shakeX, shakeY, camZ)` を適用で修正

**学んだこと**:
- カメラシェイクは「ベース位置からのオフセット」を毎フレーム適用する。現在位置に加算し続けてはいけない
- Expanderのような「スケールを段階的に増やす」ロジックは、初期スケールを `Initialize()` 時にキャッシュすること
- `_activeSafeZones[0]` を削除する SafeZone ロジックは、追加したオブジェクトへの参照を保持して削除する方が安全

**次回への改善提案**:
- DotBehaviorは別ファイル（DotBehavior.cs）に分離した方がレビューしやすい
- カメラシェイク系処理は共通ユーティリティクラス化を検討
