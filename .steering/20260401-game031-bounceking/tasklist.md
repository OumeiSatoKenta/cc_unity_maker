# タスクリスト: Game031_BounceKing

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（ball, paddle, block×5色, background）

## フェーズ2: C# スクリプト実装
- [x] BounceKingGameManager.cs
- [x] BreakoutManager.cs（コアメカニクス: ブロック配置・ボール・パドル・入力）
- [x] BallController.cs（ボール個体制御）
- [x] Block.cs（ブロック個体制御）
- [x] BounceKingUI.cs（UI管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup031_BounceKing.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-01
- PR: https://github.com/OumeiSatoKenta/cc_unity_maker/pull/182 (マージ済み)

### 計画と実績の差分
- `Paddle.cs` は不要と判断し BreakoutManager で直接 transform 操作した（計画通り）
- `BallController` から `SetMaterial()` メソッドを追加し、Rigidbody2D 設定を一元化

### 発見した重要バグ（コードレビューで発見）
1. **`Destroy` 遅延問題**: `Block.Hit()` で `_onDestroyed?.Invoke` 後に `Destroy` すると、Unity の遅延 Destroy により `Count == 0` チェックが永遠に通らない。解決: コールバックに `this` を渡し、BreakoutManager 側で `_blocks.Remove(destroyedBlock)` を即時実行。
2. **リスタート時ボール残存**: `StartGame` の冒頭で既存ボールを Destroy するガードが必要。

### 次回への改善提案
- `[Tooltip]` は最初から各 `[SerializeField]` に付けること（規約準拠）
- ブロック全消滅判定は常にコールバック側でリスト操作→カウントチェックするパターンを標準化する
