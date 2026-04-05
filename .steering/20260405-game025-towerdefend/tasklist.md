# タスクリスト: Game025v2 TowerDefend

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（背景・壁・敵4種・スタート・ゴール）

## フェーズ2: C# スクリプト実装
- [x] TowerDefendGameManager.cs（StageManager・InstructionPanel統合、ゲーム状態管理）
- [x] WaveManager.cs（Wave管理・敵生成・経路計算）
- [x] WallManager.cs（壁描画・グリッド管理・ダブルタップ消去）
- [x] Enemy.cs（敵の種類・移動・破壊処理）
- [x] TowerDefendUI.cs（インクバー・Wave・突破数・スコア・クリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup025v2_TowerDefend.cs（全配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更

## 実装後の振り返り

**実装完了日**: 2026-04-05

**計画と実績の差分**:
- WaveManager.Initialize()・SetPaths()・WallManager.SetGridOrigin() はruntime dependency injection（StartGame/OnStageChanged から呼ぶ）が必要だった
- StageManager の `_stageConfigs` フィールドは存在しない（`_configs` がprivate、SceneSetupでは `_totalStages` のみ設定）
- BFSのグリッド境界チェック・Enemy path[0] アクセスのnullガードが設計段階で漏れていた

**学んだこと**:
- StageManager SerializedObject配線は `_totalStages` のみ、ステージ設定はデフォルト or OnStageChangedで処理
- Enemy.Initialize() の path が空の可能性に備えて path[0] 前に Count > 0 チェック必須
- WallManager Update() 先頭で `_mainCam == null` チェックが必要

**次回への改善提案**:
- 設計書に「SceneSetupでは設定できないフィールド（runtime injection）」を明記
- StageManager の設定方法を設計段階で確認する
