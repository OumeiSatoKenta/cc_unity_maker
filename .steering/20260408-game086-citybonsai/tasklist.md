# タスクリスト: Game086v2_CityBonsai

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, trunk, branch_empty, house, shop, public, shrine, park, flower）

## フェーズ2: C# スクリプト実装
- [x] CityBonsaiGameManager.cs（StageManager・InstructionPanel統合、コンボ/スコア管理）
- [x] CityBonsaiManager.cs（コアメカニクス: 枝スロット・建物配置・剪定・ターン進行・季節・災害・ライバル）
- [x] CityBonsaiUI.cs（HUD・建物ボタン・剪定/ターンボタン・パネル群）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup086v2_CityBonsai.cs（全フィールド配線・InstructionPanel・StageManager含む）

## 実装後の振り返り

- **実装完了日**: 2026-04-08
- **計画と実績の差分**: 計画通り。スプライト→スクリプト→SceneSetupの順で実装完了。
- **コードレビューで修正した点**:
  - `_population` が負になりうるバグ（剪定・災害時）→ `Mathf.Max(0, ...)` でクランプ
  - 全スロット無効化で詰み状態になるバグ → `CheckGoals()` に有効スロット0チェック追加
  - `_season = (_turn / 1) % 4` の冗長コード → 簡略化
- **学んだこと**: simulation系は状態変数が多いため、境界値チェックを入念に行う必要がある
- **次回への改善**: スコアボーナス計算の指数増加問題（推奨指摘）は今回見送ったが、バランス調整時に検討
