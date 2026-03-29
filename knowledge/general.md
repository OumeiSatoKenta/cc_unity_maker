# 汎用ルール・共通教訓

## Unity 6 + 新 Input System

### EventSystem は InputSystemUIInputModule を使う
- `StandaloneInputModule` は旧 Input System 用。Unity 6 で Input System Package を有効にしている場合は `InputSystemUIInputModule` を使う。
- `using UnityEngine.InputSystem.UI;` が必要。

### OnMouseDown / OnMouseUp は使えない
- 新 Input System のみのモードでは `OnMouseDown` / `OnMouseUp` コールバックが発火しない。
- 代替: `Update()` で `Mouse.current.leftButton.wasPressedThisFrame` + `Physics2D.OverlapPoint` を使う。
- 入力処理は個別オブジェクトではなく Manager クラスに一元化すべき（複数オブジェクトが同時にドラッグ状態になる問題を防ぐ）。

### Input.mousePosition は使えない
- `Input.mousePosition` は旧 API。`Mouse.current.position.ReadValue()` または `Pointer.current.position.ReadValue()` を使う。

## Unity Editor スクリプト（SceneSetup）

### Sprite.Create() はプレハブに保存できない
- `Sprite.Create()` で生成したスプライトはランタイムオブジェクトのため、プレハブに SerializeField で保持できない。
- 対策: テクスチャを PNG でファイル保存 → `AssetDatabase.ImportAsset` → `TextureImporter` で Sprite 設定 → `AssetDatabase.LoadAssetAtPath<Sprite>()` でアセットとして読み込む。

### Play モード中は EditorSceneManager.NewScene が使えない
- `EditorApplication.isPlaying` チェックを入れてガードする。

### SceneSetup で Build Settings にシーンを追加する場合
- `EditorBuildSettings.scenes` を直接操作する。
- Play モード中の変更は保持されない場合がある。

## Unity MCP

### --project-path を指定しないと意図しないプロジェクトに接続される
- `.mcp.json` の `unity-mcp` に `--project-path` を明示的に指定すること。
- 複数の Unity プロジェクトが開いている場合、どのプロジェクトに接続されるか不定。

## Gemini CLI

### 無料枠のデイリークォータ
- 無料枠は 1日20リクエスト程度（モデルにより異なる）。
- アセット生成はテスト含めてすぐ消費される。複数画像を一度に生成する場合は有料プランを検討。
- 代替: Pillow（Python）で直接画像を描画すれば API 消費なし。

### Gemini CLI はワークスペース外に書き込めない
- `--project-path` のワークスペース内か、`~/.gemini/tmp/` にしかファイルを保存できない。

## GitHub

### GitHub Projects のスコープ
- `gh project` コマンドには `read:project` と `project` スコープが必要。
- `gh auth refresh -s read:project,project -h github.com` で追加。

### Issue 一括登録時のレート制限
- GitHub API の 504 タイムアウトが発生する場合がある。
- 冪等設計（既存チェック + スキップ）にすることで、再実行で続きから処理可能。
