# タスクリスト: Game004v2_WordCrystal

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（background, crystal_normal, crystal_hidden, crystal_bonus, crystal_poison, letter_tile）

## フェーズ2: C# スクリプト実装
- [x] CrystalObject.cs（クリスタルオブジェクト、タイプ管理）
- [x] LetterTile.cs（文字タイル）
- [x] WordManager.cs（コアメカニクス・クリスタル配置・単語判定・5ステージ難易度対応）
- [x] WordCrystalGameManager.cs（StageManager・InstructionPanel統合・スコア管理）
- [x] WordCrystalUI.cs（タイマー・スコア・スロット・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup004v2_WordCrystal.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリ）

## 実装後の振り返り

- 実装完了日: 2026-04-03
- PR: #264 (マージ済み)
- コードレビューで発見・修正した主要バグ:
  - WordCrystalUI が GetComponentInParent で WordManager を取得できない問題 → SerializeField に変更
  - RemoveSlotAt で同文字が複数ある場合に誤ったタイルを復元するバグ → _slotTiles[] 配列でインデックス対応を保持
  - Camera.main が毎フレーム検索される問題 → Awake でキャッシュ
  - SpawnCrystals で normalCount が負になりうる問題 → Mathf.Max(0, ...) で保護
  - コルーチン中にオブジェクト破棄で MissingReferenceException → null チェック追加
