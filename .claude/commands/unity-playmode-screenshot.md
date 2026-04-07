---
description: Unity Editor で PlayMode を起動してゲーム画面をスクリーンショット撮影し、表示崩れがないか検証するスキル。「画面確認」「スクショを撮って」「PlayModeでテスト」「画面崩れを確認」「playmode screenshot」などのリクエスト、またはadd-featureのステップ5-5として呼び出されたときに使用する。
---

# Unity PlayMode スクリーンショット検証スキル

Unity MCP 経由で PlayMode を起動し、ゲーム画面をスクリーンショット撮影して表示崩れを検証するスキル。

**引数:** ゲームID（省略可。指定するとレポートにゲーム名を明記する）

---

## 実行手順

### 0. Unity MCP セッション確認（必須・最初に実行）

`mcp__coplaydev-mcp__manage_editor(action="status")` を呼び出す。

- **エラーが返った場合 → 即座に終了する:**
  ```
  ⚠️ Unity MCP セッションなし: PlayMode 検証をスキップします。
  ```
  以降のステップは**一切実行しない**。

- **正常に応答が返った場合 → ステップ1へ進む。**

### 1. Play Mode を開始

`mcp__coplaydev-mcp__manage_editor` ツールを使用する:
```
mcp__coplaydev-mcp__manage_editor(action="play")
```

### 2. 画面が安定するまで待ち、エラーを確認

`mcp__coplaydev-mcp__read_console` ツールを使用する:
```
mcp__coplaydev-mcp__read_console(types=["error", "warning"], count=30)
```

ランタイムエラーが出ている場合は即座に Play Mode を停止し、エラー内容を報告して終了する。

### 3. スクリーンショット取得

**重要:** `capture_source="game_view"` では `Screen Space - Overlay` モードの Canvas UI が含まれない場合がある。必ず両方撮影して比較する。

`mcp__coplaydev-mcp__manage_camera` ツールを使用する:
```
mcp__coplaydev-mcp__manage_camera(action="screenshot", capture_source="game_view", include_image=true)
mcp__coplaydev-mcp__manage_camera(action="screenshot", capture_source="scene_view", include_image=true)
```

game_view がほぼ単色（カメラ背景色のみ）で UI が見えない場合は、**エディタ上では正常に表示されている可能性がある**。その場合は scene_view の画像とコンソールエラーの有無で判断する。UI の最終確認はユーザーにエディタの Game View を直接確認してもらう。

### 4. Play Mode を停止

`mcp__coplaydev-mcp__manage_editor` ツールを使用する:
```
mcp__coplaydev-mcp__manage_editor(action="stop")
```

### 5. 画像を目視確認し判定

取得した画像を以下の観点でチェックする:

| 観点 | チェック内容 |
|------|------------|
| UI 配置 | スコア・ライフ・ステージ表示が画面内に収まっているか |
| テキスト | 文字化け・欠け・重なりがないか |
| スプライト | 背景・オブジェクトが意図した位置に表示されているか |
| 重大崩れ | 黒画面・白飛び・UI が画面外に出ていないか |

### 6. 判定基準

| 判定 | 条件 | 対応 |
|------|------|------|
| ✅ 問題なし | 上記すべてクリア | 次のステップへ |
| ⚠️ 軽微な崩れ | レイアウトのズレ等、ゲームプレイに影響しない | Issue コメントに記録して続行 |
| ❌ 重大な崩れ | 黒画面・UI が全く表示されない等 | SceneSetup スクリプトを修正して `unity-scene-setup` からやり直す |

game_view が単色でも scene_view にスプライトが見えており、コンソールエラーがなければ ⚠️（MCP撮影限界）として続行し、ユーザーにエディタ確認を依頼する。

### 7. 結果レポート

```
## PlayMode 画面検証結果
- 状態: ✅ 問題なし / ⚠️ 軽微な崩れ / ❌ 重大な崩れ
- ランタイムエラー: N件
- 確認した観点:
  - UI 配置: ✅/⚠️/❌（game_view でUI非表示の場合はエディタ確認要）
  - テキスト: ✅/⚠️/❌
  - スプライト: ✅/⚠️/❌
- スクリーンショット: （game_view・scene_view 両方を表示）
- 所見: （気になった点があれば記述）
```

---

## MCP 接続がない場合

ステップ0で検出済みのため、このセクションは参照不要。
