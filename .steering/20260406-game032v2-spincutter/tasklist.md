# タスクリスト: Game032v2_SpinCutter

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（Background, Blade, Enemy, Obstacle）

## フェーズ2: C# スクリプト実装
- [x] SpinCutterGameManager.cs（StageManager・InstructionPanel統合）
- [x] BladeController.cs（回転・当たり判定・消滅）
- [x] SpinCutterMechanic.cs（敵生成・障害物・発射管理）
- [x] SpinCutterUI.cs（ステージ表示・スライダー・プレビュー・パネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup032v2_SpinCutter.cs（InstructionPanel・StageManager・スライダー配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

**実装完了日**: 2026-04-06

**計画と実績の差分**:
- 全フェーズ（1〜4）は前セッションで完了済みだった
- 今セッションはコードレビュー（Step 5-2）からの再開
- コードレビューで5件の[必須]指摘を修正してからPR作成

**発見されたバグと修正**:
1. スコア二重計算: OnBladeDiedのOnEnemyDefeated重複呼び出しを除去
2. BladeController.Die()でComboEffectコルーチンが残存 → StopAllCoroutines()追加
3. SpinCutterMechanicのGetComponentInParent<SpinCutterUI>()が常にnullを返す → SerializeFieldに変更、SceneSetup配線追加
4. CanvasScalerのmatchWidthOrHeight設定漏れ（両Canvas）
5. SpawnEnemiesのゼロ除算リスク（count==0時）
6. SceneLoader未使用（ReturnToMenu/RestartGame）

**学んだこと**:
- GetComponentInParentは同階層のコンポーネントには機能しない → 常にSerializeField使用
- コンボスコア計算は1箇所に集約すべき（OnAllEnemiesDefeatedのみで処理）

**次回への改善提案**:
- Mechanicクラスに_uiフィールドがある場合は必ずSerializeFieldにする規約を強化
