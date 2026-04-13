# タスクリスト: Game090v2_StarshipCrew

## フェーズ1: スプライトアセット生成
- [x] Python + Pillow で高品質スプライト画像を生成（宇宙背景・クルーカード10種・ミッションボタン用アイコン）

## フェーズ2: C# スクリプト実装
- [x] StarshipCrewGameManager.cs（StageManager・InstructionPanel統合・スコア・コンボ管理）
- [x] CrewManager.cs（クルーデータ・選択・ミッション派遣・成功率計算・相性システム・装備）
- [x] StarshipCrewUI.cs（HUD・クルーカード・ミッションパネル・リザルト・クリアパネル）

## フェーズ3: SceneSetup Editor スクリプト
- [x] Setup090v2_StarshipCrew.cs（全フィールド配線・InstructionPanel・StageManager・スプライト読み込み）

## 実装後の振り返り

### 実装完了日
2026-04-09

### 計画と実績の差分
- 計画通り3スクリプト + SceneSetup + スプライト22枚を実装完了
- コードレビューで必須修正6件が発覚（difficultyMultiplier バグ、相性インデックス空間不一致、コルーチン二重起動、キャンセルボタン未配線等）

### 学んだこと
- `goodCompat`/`badCompat` に格納するインデックスは `_allCrew` 配列のインデックスだが、選択時は `_stageCrew` のインデックスを使うため比較空間が異なる → `GetAllCrewIndex()` で変換が必要
- `_selectedMissionIndex` をクリアする前に `difficultyMultiplier` を取得しないと常に配列末尾要素が使われる
- コルーチン二重起動はフラグまたはCoroutine参照で防止する

### 次回への改善提案
- SceneSetup の CB() 関数でキャンセルボタンのリスナーも同時に配線するテンプレートを用意すると漏れが防げる
- Unity MCP がタイムアウトする場合、コンパイル後に短い待機を挟んでから read_console すると取得できることが多い

### 事後検証（2026-04-09）
- ✅ コンパイル: エラーなし（0件）
- ✅ SceneSetup: Assets/Setup/090v2 StarshipCrew 実行成功
- ✅ PlayMode: ランタイムエラーなし、背景・スプライト表示確認済み
- ✅ GameRegistry.json: implemented: true に更新完了
