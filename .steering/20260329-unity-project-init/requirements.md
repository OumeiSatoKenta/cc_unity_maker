# 要求内容: Unityプロジェクト初期構築

## 背景
全100ゲームを格納する単一Unityプロジェクトの骨格を `unity/` に作成する。
Unity Hub で開いてそのまま使える状態にする。

## 作成物

### Unityプロジェクト構造
- `unity/Packages/manifest.json` — Unity 6 標準パッケージ依存
- `unity/ProjectSettings/ProjectVersion.txt` — Unity 6000.x バージョン指定
- `unity/Assets/Scenes/` — シーン格納ディレクトリ
- `unity/Assets/Scripts/Common/` — 共通スクリプト格納ディレクトリ
- `unity/Assets/Scripts/TopMenu/` — TopMenu スクリプト格納ディレクトリ
- `unity/Assets/Editor/SceneSetup/` — Editorスクリプト格納ディレクトリ
- `unity/Assets/Resources/` — GameRegistry.json 等のランタイムデータ格納

### Unity用 .gitignore
- `unity/.gitignore` — Library/, Temp/, Obj/ 等の除外設定

## 制約
- .unity シーンファイルと .meta ファイルは Unity Editor が自動生成するため手動作成しない
- ProjectSettings の .asset ファイルも Unity Editor 生成に任せる
- Git で空ディレクトリを保持するために .gitkeep を配置する
