# タスクリスト: Game043v2_BallSort3D

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（background, tube, ball_r/g/b/y/m, lid, lock_icon）

## フェーズ2: C# スクリプト実装
- [x] BallSort3DGameManager.cs（StageManager・InstructionPanel統合）
- [x] BallSort3DMechanic.cs（コアメカニクス・5ステージ難易度・Undo・デッドロック検出）
- [x] BallSort3DUI.cs（ステージ・スコア・手数・コンボ・タイマー表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup043v2_BallSort3D.cs（InstructionPanel・StageManager・全UI配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- 計画と実績の差分: BallSort3DMechanic.cs が設計より複雑になり約720行に。ロックボール・蓋付きチューブ・回転チューブの3種のステージ固有メカニクスが追加された
- 学んだこと:
  - デッドロック検出はcover/isRotatedチューブがある場合は早期return falseが必要（ステージ初期化時の誤検出防止）
  - PlacePulse/SelectPulseのコルーチン内はnullガード必須（Destroy後にアクセスするとMissingReferenceException）
  - UndoLastMoveでrotateIconObj再生成前に旧アイコンをDestroyしないと重複表示になる
  - GetComponent every frameはUpdate内で禁止、SerializeFieldで参照を保持すること
- 次回への改善提案: Stage5で空きチューブが1本のみの場合にデッドロックしやすいため、パズル生成時に解けることを検証するアルゴリズムを検討する
