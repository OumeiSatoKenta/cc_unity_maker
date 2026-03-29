---
name: generate-game-assets
description: Gemini CLIを使ってUnityゲーム用のスプライト画像（ブロック・背景・UI・アイコン等）を自動生成するスキル。「アセットを生成」「画像を作って」「スプライトを作成」等のリクエストでトリガーする。
allowed-tools: Bash, Read, Write, Edit, Glob, Grep
---

# ゲームアセット生成スキル

Gemini CLI の画像生成機能を使って、Unityゲーム用のスプライト画像を自動生成する。

## 前提条件

- Gemini CLI がインストール済み（`brew install gemini-cli`）
- 環境変数 `GEMINI_API_KEY` が設定済み、または `~/.gemini/settings.json` に API キーが記載済み

## 使い方

### 基本コマンド

```
ゲーム001のアセットを生成して
```

### 指定可能なオプション

- ゲームID（例: 001）
- 生成する画像の種類（ブロック、背景、UIボタン、アイコン等）
- スタイル指定（ピクセルアート、フラットデザイン、水彩風等）
- サイズ指定（デフォルト: 128x128）

## 生成フロー

### Step 1: ゲーム情報の取得

GameRegistry.json と Issue から対象ゲームの情報を取得する。

```bash
# GameRegistry.json からゲーム情報を取得
cat MiniGameCollection/Assets/Resources/GameRegistry.json | python3 -c "
import json, sys
data = json.load(sys.stdin)
game = next((g for g in data['games'] if g['id'] == 'TARGET_ID'), None)
print(json.dumps(game, ensure_ascii=False, indent=2))
"
```

### Step 2: アセットリストの設計

ゲームの内容に基づいて必要なアセットを決定する。

**共通アセット（全ゲーム）**:
- 盤面/背景テクスチャ

**ゲーム固有アセット（例: BlockFlow）**:
- 各色のブロックスプライト
- グリッド線テクスチャ

### Step 3: Gemini CLI で画像生成

各アセットについて Gemini CLI を呼び出す。

**呼び出しコマンド**:
```bash
GEMINI_API_KEY=${GEMINI_API_KEY} gemini -p "PROMPT" --yolo -o text 2>&1
```

**プロンプトのテンプレート**:
```
Generate a [SIZE] pixel game sprite for a Unity 2D puzzle game called "[GAME_TITLE]".
The sprite should be: [DESCRIPTION]
Style: [STYLE] (clean, with transparent background where appropriate)
Save the image as PNG to: [OUTPUT_PATH]
```

**出力先ディレクトリ**:
```
MiniGameCollection/Assets/Sprites/Game<ID>_<Title>/
```

### Step 4: Unity への組み込み

生成した画像をスクリプトで参照する。

1. テクスチャのインポート設定を確認（Sprite Mode, Pixels Per Unit）
2. 必要に応じて SceneSetup スクリプトを更新してスプライト参照を変更
3. BlockController 等のスクリプトで `Resources.Load<Sprite>()` または SerializeField で参照

## プロンプト設計ガイドライン

### 良いプロンプト例

```
Generate a 128x128 pixel game sprite of a red crystal gem block
for a puzzle game. The gem should have:
- A diamond/rhombus shape
- 3D shading with highlights on the upper-left
- Rich red color (#E03030)
- Transparent background
- Clean edges suitable for a 2D game
Save as PNG to MiniGameCollection/Assets/Sprites/Game001_BlockFlow/block_red.png
```

### スタイル指定のバリエーション

| スタイル | 説明 | 向いているゲーム |
|---------|------|----------------|
| pixel-art | ドット絵風 | レトロ系、カジュアル |
| flat | フラットデザイン | モダンUI、パズル |
| cartoon | カートゥーン調 | カジュアル、キッズ |
| watercolor | 水彩風 | アート系、禅 |
| neon | ネオン/グロー | アクション、リズム |

## 制約事項

- Gemini CLI はPillow（Python画像ライブラリ）で画像を描画する
- 写真リアルな画像ではなく、イラスト/ゲームスプライト向き
- 1回の呼び出しで1画像を生成（バッチ生成はループで対応）
- ワークスペース内のパスにのみ保存可能
- 無料APIの場合、レート制限あり（15 RPM）— 連続生成時は間隔を空ける

## ディレクトリ構造

```
MiniGameCollection/Assets/
├── Sprites/                          # ← アセット画像格納先
│   ├── Common/                       # 共通アセット（UIボタン等）
│   ├── Game001_BlockFlow/            # ゲーム固有アセット
│   │   ├── block_red.png
│   │   ├── block_blue.png
│   │   ├── block_green.png
│   │   ├── board_background.png
│   │   └── ...
│   └── Game002_MirrorMaze/
│       └── ...
```
