# タスクリスト: Game005v2_PipeConnect

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像を生成（pipe_straight, pipe_elbow, pipe_tjunction, pipe_source, pipe_exit, pipe_locked, pipe_valve_open, pipe_valve_closed, background）

## フェーズ2: C# スクリプト実装
- [x] PipeCell.cs（パイプセルコンポーネント、PipeType enum、接続方向計算、回転アニメーション）
- [x] PipeManager.cs（グリッド管理、入力処理、水流BFS、5ステージレイアウト生成、レスポンシブ配置）
- [x] PipeConnectGameManager.cs（ゲーム状態管理、StageManager・InstructionPanel統合、スコア・タイマー）
- [x] PipeConnectUI.cs（HUD表示、クリア/ゲームオーバーパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup005v2_PipeConnect.cs（InstructionPanel・StageManager・PipeCellPrefab配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（collection: "remake" のエントリ）

## 実装後の振り返り

**実装完了日**: 2026-04-03

**計画と実績の差分**:
- ステアリングファイルは既に存在していたため新規作成不要だった（過去セッションの作業）
- コードレビューで2件の必須修正を実施: FlowCoroutine二重起動防止フラグ追加、SetupStage引数の整理

**学んだこと**:
- BFSの方向インデックスとグリッド座標系の対応は必ず一致確認が必要
- コルーチンを複数箇所から起動できる設計では二重起動防止フラグが必須

**次回への改善提案**:
- ステージレイアウトをScriptableObject化すれば実装後の調整が容易になる
