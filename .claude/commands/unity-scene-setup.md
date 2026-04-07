---
description: Unity Editor で SceneSetup メニューを自動実行してシーンを構成するスキル。「SceneSetupを実行」「シーンを作り直して」「Assets/Setup を実行」「scene setup [ゲームID]」などのリクエスト、またはadd-featureのステップ5-4として呼び出されたときに使用する。
---

# Unity SceneSetup 実行スキル

Unity MCP 経由で SceneSetup Editor メニューを実行し、シーンを自動構成するスキル。

**引数:** ゲームID（必須。例: `056`、`042v2`）

---

## 実行手順

### 0. Unity MCP セッション確認（必須・最初に実行）

`mcp__coplaydev-mcp__manage_editor(action="status")` を呼び出す。

- **エラーが返った場合 → 即座に終了する:**
  ```
  ⚠️ Unity MCP セッションなし: SceneSetup 実行をスキップします。
  ```
  以降のステップは**一切実行しない**。

- **正常に応答が返った場合 → ステップ1へ進む。**

### 1. メニューパスの決定

引数のゲームIDから GameRegistry.json を参照してメニューパスを特定する:
- `collection: "remake"` → `Assets/Setup/[ID]v2 [Title]`
- `collection: "classic"` → `Assets/Setup/[ID] [Title]`

GameRegistry.json が参照できない場合は、引数をそのままIDとして使う（例: `056` → パスを両方試みる）。

### 2. Play Mode を停止してから実行

`mcp__coplaydev-mcp__manage_editor` / `mcp__coplaydev-mcp__execute_menu_item` / `mcp__coplaydev-mcp__read_console` ツールを使用する:
```
mcp__coplaydev-mcp__manage_editor(action="stop")
mcp__coplaydev-mcp__execute_menu_item(menu_path="Assets/Setup/[ID] [Title]")
mcp__coplaydev-mcp__read_console(types=["error"], count=20)
```

### 3. 判定と対応

**エラーがない場合:**
- 「✅ SceneSetup 完了: シーン構成済み」と報告して完了。

**エラーがある場合:**
- エラーの原因を特定（SceneSetup スクリプトのバグか、参照切れか）。
- 修正して再実行する（最大3回）。
- 解消しない場合はエラー詳細をユーザーに報告。

### 4. 結果レポート

```
## SceneSetup 実行結果
- 対象: Assets/Setup/[ID] [Title]
- 状態: ✅ 成功 / ❌ 失敗
- エラー: （あれば内容）
```

---

## MCP 接続がない場合

ステップ0で検出済みのため、このセクションは参照不要。
