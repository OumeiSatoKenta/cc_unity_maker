# タスクリスト: Game085v2_MechPet

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow でスプライト画像生成（ロボットパーツ・背景・UI）

## フェーズ2: C# スクリプト実装
- [x] MechPetGameManager.cs（StageManager・InstructionPanel統合）
- [x] MechPetManager.cs（パーツ管理・シナジー計算・ミッション処理）
- [x] MechPetUI.cs（UI表示・スロット・パネル管理）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup085v2_MechPet.cs（フル配線）

## 実装後の振り返り

**実装完了日:** 2026-04-07

**計画と実績の差分:**
- コードレビューで5件の重要な修正が見つかった（StageTargetScore初期値、Camera.mainのnullチェック、RunMissionの_isActive順序、coroutineのyield break、StopCoroutineの参照方法）
- コンテキスト上限に達したため、セッションをまたいで修正・PR作成を行った

**学んだこと:**
- `StageTargetScore`のような「設定前にアクセス可能なプロパティ」は初期値を安全な値（int.MaxValue）にする必要がある
- coroutineで非同期処理後にgameObject.activeInHierarchyをチェックしないと、SetActive(false)後も処理が続く
- `_isActive = true`とコールバック呼び出しの順序がステージクリア判定に影響する

**次回への改善提案:**
- ゲームオーバー条件がないシミュレーション系は、ミッション失敗のペナルティをより明確にするとゲーム性が高まる
- シナジーシステムの視覚フィードバック（パーツのハイライト等）を加えるとわかりやすい
