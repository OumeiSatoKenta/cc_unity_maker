# タスクリスト: Game078v2_EchoBack

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（背景・鍵盤・マーカー等）

## フェーズ2: C# スクリプト実装
- [x] EchoBackGameManager.cs（StageManager・InstructionPanel統合）
- [x] EchoManager.cs（コアメカニクス・5ステージ難易度対応・音程判定・リプレイ）
- [x] EchoBackUI.cs（ステージ表示・コンボ・判定テキスト・進捗ドット）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup078v2_EchoBack.cs（InstructionPanel・StageManager配線・鍵盤UI構成含む）

## 実装後の振り返り

- 実装完了日: 2026-04-07
- 差分:
  - 計画通り3スクリプト + SceneSetup + スプライト8枚を実装完了
  - [必須]修正: `_perfectCount`リセット漏れ、スコア累積バグ、Stage5テンポ変化のタイミング計算、`_patternTimes`未使用デッドコード、`TriggerGameOverDelay`の`_isActive`ガード漏れ
  - `StopAllCoroutinesOn`プレースホルダーを`_pulseCoroutine`トラッキングに修正
  - リプレイボタンの`↺`文字をフォント非対応のため`>>`に変更
- 学んだこと:
  - 手続き的AudioClipの生成はサイン波を直接書き込むことで音源ファイル不要
  - リズム系ゲームのタイミング判定では、テンポ変化を`_patternTimes`に事前計算して埋め込む必要がある
  - Canvas UI Buttonを鍵盤として使う設計はレスポンシブ配置と当たり判定の両立に有効
- 次回への改善提案:
  - 特殊文字（矢印記号等）はフォントの対応状況を事前に確認する
