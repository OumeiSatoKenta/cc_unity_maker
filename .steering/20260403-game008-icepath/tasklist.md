# タスクリスト: Game008v2_IcePath

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（player, ice, rock, crack, hole, redirect, friction, visited, background）

## フェーズ2: C# スクリプト実装
- [x] IcePathGameManager.cs（StageManager・InstructionPanel統合、Undo機能）
- [x] IceBoardManager.cs（盤面ロジック・5ステージ対応・スワイプ入力・レスポンシブ配置）
- [x] IcePathUI.cs（ステージ・スコア・手数・残りマス表示・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup008v2_IcePath.cs（全フィールド配線・InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（008 remake エントリ）

## 実装後の振り返り
（実装完了後に記入）
