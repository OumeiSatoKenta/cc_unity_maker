# タスクリスト: Game031v2_BounceKing

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト生成（Background, Paddle, Ball, BlockNormal, BlockHard, BlockBoss, ItemExpand, ItemMultiBall, ItemShrink）

## フェーズ2: C# スクリプト実装
- [x] Block.cs（個別ブロック: 耐久値・色・ヒット処理・破壊フラッシュ）
- [x] ItemController.cs（アイテム落下・取得・効果発動）
- [x] PaddleController.cs（ドラッグ操作・移動範囲制限・反射角度）
- [x] BallController.cs（物理移動・反射・壁/パドル/ブロック衝突）
- [x] BlockManager.cs（ブロック配置・ステージ別パラメータ・全破壊通知）
- [x] BounceKingUI.cs（スコア・ライフ・ステージ・コンボ・各パネル）
- [x] BounceKingGameManager.cs（ゲーム状態管理・StageManager・InstructionPanel統合）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup031v2_BounceKing.cs（全GameObject生成・配線・InstructionPanel・StageManager配線含む）

## フェーズ4: GameRegistry.json 更新
- [x] implemented: true に変更（remakeエントリー）

## 実装後の振り返り
（実装完了後に記入）
