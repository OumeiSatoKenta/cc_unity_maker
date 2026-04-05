# タスクリスト: Game033_AimSniper

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（background, crosshair, target）

## フェーズ2: C# スクリプト実装
- [x] AimSniperGameManager.cs
- [x] SniperManager.cs（コアメカニクス: スコープ操作・射撃・ターゲット管理）
- [x] Target.cs（ターゲットの往復移動・被弾処理）
- [x] AimSniperUI.cs（UI管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup033_AimSniper.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-01
- PR: https://github.com/OumeiSatoKenta/cc_unity_maker/pull/187 (マージ済み)

### 計画と実績の差分
- 風向き要素は省略（Sサイズ相当のため）。UI上は非表示
- スコープ操作は isPressed ではなくマウス位置に常に追従（ドラッグ不要）に変更

### 学んだこと
- Issue の新テンプレート（操作仕様・クリア条件・UI要素一覧）があることで設計フェーズが高速化
- Perlin noise のスコープ揺れは SwayAmplitude=0.15f, Frequency=1.5f で自然な動きになる
