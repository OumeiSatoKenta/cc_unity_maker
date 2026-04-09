# タスクリスト: Game098v2_InfiniteLoop

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・部屋オブジェクト6種・フラッシュ・ロゴ）

## フェーズ2: C# スクリプト実装
- [x] InfiniteLoopGameManager.cs（StageManager・InstructionPanel統合、スコア・コンボ管理）
- [x] LoopManager.cs（ループ制御・変化要素管理・脱出条件判定・入力処理）
- [x] InfiniteLoopUI.cs（ステージ・スコア・ループ残数・メモ・パネル表示）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup098v2_InfiniteLoop.cs（InstructionPanel・StageManager・全フィールド配線含む）

## 実装後の振り返り

### 実装完了日
2026-04-09

### 計画と実績の差分
- 計画通り3スクリプト+SceneSetup+スプライト9枚を実装
- コンパイルエラー1件（camW変数名衝突）がSceneSetup内で発生したが即修正
- コードレビューで3つの[必須]指摘を受けて修正（絵文字除去・RestartGame再購読・TryEscape順序修正）

### 学んだこと
- SceneSetup内でif{}ブロックとその外で同名変数を使うと CS0136エラー → ブロック内変数に一意な名前をつける
- TryEscape失敗時はEscapeFlash開始前にループ上限チェックを行う（SetActive→StopAllCoroutinesの競合防止）
- フォントにNotoSansJP-Regular SDFを使用する場合は絵文字を使わない

### 次回への改善提案
- SceneSetup内のローカル変数名は全てプレフィックスを付けて衝突を防ぐ（`bg_`, `room_` など）
- TryEscape等のフロー変更を伴うメソッドは実装前にシーケンス図を設計.mdに記述する
