# cc_unity_maker

Unity ミニゲームコレクション（100本）を Claude Code で自動実装するプロジェクト。

## ゲーム自動実装の実行方法

### loop コマンドで自動実行

```
/loop 5h 状態ファイル(~/.claude/projects/-Users-satoken-workspace-claude-code-cc-unity-maker/game_schedule_state.json)からnext_idを読み、レートリミットに達するまで以下をループ実行: ゼロパディングして /implement-game [ID] を実行 → 完了後 next_id を +1 更新 → 次のゲームへ。next_idが101以上になったら全完了。レートリミットエラー時はnext_idを保存して停止。
```

### 手動で1本ずつ実行

```
/implement-game 014
```

### 状態ファイル

パス: `~/.claude/projects/-Users-satoken-workspace-claude-code-cc-unity-maker/game_schedule_state.json`

```json
{
  "next_id": 2,
  "total": 100
}
```

## プロジェクト構成

```
MiniGameCollection/Assets/
├── Scripts/Game[ID]_[Title]/          # ゲームスクリプト
├── Editor/SceneSetup/                 # SceneSetup Editorスクリプト
├── Resources/Sprites/Game[ID]_[Title]/ # スプライトアセット
└── Resources/GameRegistry.json        # ゲーム一覧
```
