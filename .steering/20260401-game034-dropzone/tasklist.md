# タスクリスト: Game034_DropZone

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（background, apple, banana, paper, can, bottle, glass, zone_green, zone_gray, zone_blue）

## フェーズ2: C# スクリプト実装
- [x] DropZoneGameManager.cs
- [x] DropManager.cs（コアメカニクス: アイテム生成・ドラッグ・ゾーン判定）
- [x] DropItem.cs（アイテムの落下・データ保持）
- [x] DropZoneUI.cs（UI管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup034_DropZone.cs

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

- 実装完了日: 2026-04-01
- PR: https://github.com/OumeiSatoKenta/cc_unity_maker/pull/189 (マージ済み)

### 計画と実績の差分
- ゾーンラベルはSpriteRendererではなく、DropManager内でゾーン名表示なしで実装（ゾーンの色で十分識別可能）
- ドラッグ中もアイテムは落下し続ける仕様を採用（リリースで3倍加速）

### 学んだこと
- Invoke(nameof(SpawnNextItem), 0.4f) で次アイテム生成に遅延を入れるとテンポが良い
- コンボシステムは _combo フィールド1つで十分（正解で++、ミスで0リセット）
