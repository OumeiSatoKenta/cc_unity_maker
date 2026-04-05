# タスクリスト: Game032_SpinCutter

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（background, blade, enemy）

## フェーズ2: C# スクリプト実装
- [x] SpinCutterGameManager.cs
- [x] SpinCutterManager.cs（コアメカニクス: スライダー制御・発射・敵管理）
- [x] BladeController.cs（刃の軌道運動・衝突検出）
- [x] Enemy.cs（敵の状態管理）
- [x] SpinCutterUI.cs（UI管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup032_SpinCutter.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-01
- PR: https://github.com/OumeiSatoKenta/cc_unity_maker/pull/185 (マージ済み)

### 計画と実績の差分

- `BladeController` に `ResetBlade()` メソッドを定義したが未使用（SpawnBlade 側で Destroy + null 代入で代替）
- `OrbitPreview` の `lineRenderer.loop = true` 設定 + `positionCount = 65` (始点=終点) で正しく円が閉じる

### コードレビューで発見した修正点

1. **BladeController コールバック順序**: `gameObject.SetActive(false)` の前に `_onFinished?.Invoke()` を呼ぶよう修正（OnDisable 追加時の副作用防止）
2. **SpawnBlade の null 代入**: `Destroy(_blade.gameObject); _blade = null;` と明示的に null を代入（将来の MissingReferenceException 防止）

### 次回への改善提案

- `_totalEnemies` と `EnemyPositions.Length` の乖離リスク → `StartStage()` 内で `_totalEnemies = EnemyPositions.Length` を自動同期するパターンを標準化する
- `Physics2D.OverlapCircleAll` の GC アロケーション → `OverlapCircleNonAlloc` + 事前確保配列で改善可能（ミニゲーム規模では実害は小さい）
