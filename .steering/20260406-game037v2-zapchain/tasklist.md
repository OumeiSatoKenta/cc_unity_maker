# タスクリスト: Game037v2_ZapChain

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（ノード・障害物・背景・接続線）

## フェーズ2: C# スクリプト実装
- [x] NodeObject.cs（ノードタイプ・移動・時限・ビジュアルフィードバック）
- [x] ZapMechanic.cs（ノード生成・入力処理・チェーン・5ステージ難易度）
- [x] ZapChainGameManager.cs（StageManager・InstructionPanel統合・スコア）
- [x] ZapChainUI.cs（HUD・クリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup037v2_ZapChain.cs（InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-06
- **計画と実績の差分**: `using Common;` が他ゲームでは不要（同一アセンブリなので直接参照可能）な点を見落とし、コンパイルエラーが発生。`SceneLoader.LoadMenu()` → `SceneLoader.BackToMenu()` のメソッド名相違も修正。
- **学んだこと**: Common名前空間のクラス（InstructionPanel, StageManager, SceneLoader）は `using Common;` 不要。Unityプロジェクト内のすべてのクラスはグローバルにアクセス可能。
- **次回への改善提案**: Commonクラス使用時に `using Common;` を書かない。SceneLoaderは `BackToMenu()` メソッドを使用する。
