# タスクリスト: 共通スクリプト実装

## 実装タスク

- [x] `SceneLoader.cs` を作成する
- [x] `GameData.cs` を作成する（JSON デシリアライズ用データクラス）
- [x] `GameRegistry.cs` を作成する（シングルトン・JSON読み込み・フィルター）
- [x] `BackToMenuButton.cs` を作成する
- [x] `GameRegistry.json` の初期データ（全100ゲーム・implemented: false）を作成する

## レビューセクション

**完了日**: 2026-03-29
**実績**: 全タスク完了 + 検証フィードバック反映

**作成物**:
- `SceneLoader.cs` — シーン遷移（null/空文字チェック追加）
- `GameData.cs` — JSON デシリアライズ用データクラス
- `GameRegistry.cs` — シングルトン（パース失敗時フォールバック追加）
- `BackToMenuButton.cs` — メニュー戻るボタン
- `GameRegistry.json` — 全100ゲーム初期データ（implemented: false）

**検証で修正した点**:
- GameRegistry: `_registryData` の宣言時初期化 + JsonUtility.FromJson の null チェック追加
- SceneLoader: `LoadGame()` に null/空文字ガード追加
- development-guidelines.md: `using Common;` の記述を「namespace なし」に修正

**次**: `/add-feature TopMenuシーン`
