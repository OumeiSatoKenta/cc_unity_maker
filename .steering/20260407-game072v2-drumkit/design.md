# Design: Game072v2 DrumKit

## namespace
`Game072v2_DrumKit`

## スクリプト構成
- `DrumKitGameManager.cs` — StageManager/InstructionPanel統合、ゲーム状態管理
- `DrumPadManager.cs` — ドラムパッド配置・リング縮小アニメーション・判定処理
- `DrumKitUI.cs` — スコア/コンボ/ステージ/判定テキスト/クリアパネル表示

## ゲームメカニクス設計
- **ドラムパッド**: 6種（バスドラ, スネア, ハイハット, シンバル, タムH, タムL）
- **ガイドリング**: 各パッド上で大→小に縮小アニメーション（1.8 → 0.6のScale）
- **判定タイミング**: リングが判定サークルサイズ（scale=1.0）に達した瞬間が最適
  - Perfect: ±40ms、Great: ±80ms、Good: ±150ms
- **ノーツ生成**: BPMに合わせてビート毎にランダムパッドを選択

## レスポンシブ配置
```
camSize = 6f, aspect = 9/16
ゲーム領域: 上部HUD(Y=4.5〜6), パッド配置(Y=-2.5〜4.0), UI下部(Y=-3.0〜-6)
```

### パッド配置（6パッド）
2行3列レイアウト:
- 上段(Y=1.5): タムH, タムL, シンバル (pad 3,4,5)
- 下段(Y=-1.0): バスドラ, スネア, ハイハット (pad 0,1,2)
- X: -2.5, 0, +2.5

Dynamic calculation:
```csharp
float camSize = Camera.main.orthographicSize;
float camWidth = camSize * Camera.main.aspect;
float bottomMargin = 3.0f;
float topMargin = 1.5f;
// パッドを2行3列に配置
float rowLow = -camSize + bottomMargin + 1.2f;
float rowHigh = rowLow + 2.5f;
float colStep = camWidth * 2f / 3f * 0.85f;
```

## StageManager統合
- `OnStageChanged(int stage)` → `DrumPadManager.SetupStage(config, stage)`
- ステージ別パラメータ:
  | Stage | BPM | padCount | enableSimultaneous | enableFill | enableSyncopation |
  |-------|-----|----------|-------------------|-----------|-------------------|
  | 0 | 80 | 2 | false | false | false |
  | 1 | 100 | 3 | false | true | false |
  | 2 | 120 | 5 | false | true | false |
  | 3 | 140 | 6 | true | true | false |
  | 4 | 160 | 6 | true | true | true |

## InstructionPanel
- title: "DrumKit"
- description: "ドラムセットをリズムよくタップしよう"
- controls: "リングが縮んでパッドに重なったらタップ！"
- goal: "5ステージを全てクリアしてドラムマスターになろう"

## ビジュアルフィードバック
1. **ヒット時パッドスケールパルス**: 1.0 → 1.25 → 1.0（0.15秒）
2. **Miss時赤フラッシュ**: SpriteRenderer.color → (1,0.2,0.2,1) → (1,1,1,1)（0.2秒）
3. **Perfect時カラーフラッシュ**: パッドが黄色 → 元色（0.1秒）
4. **カメラシェイク**: Miss時に±0.08ランダムシェイク（0.15秒）
5. **判定テキスト**: パッドの上部にフェードアウト表示

## スコアシステム
- Perfect: 100pt × min(3.0, 1.0 + combo×0.1)
- Great: 60pt × min(2.0, 1.0 + combo×0.05)
- Good: 20pt（倍率なし）
- Miss: 0pt + コンボリセット + missCount++

## ゲームオーバー条件
missCount >= 10 でゲームオーバー

## SceneSetup設計
- BeatTiles(071v2)のパターンを踏襲
- `Setup072v2_DrumKit.cs`
- MenuItem: `"Assets/Setup/072v2 DrumKit"`
- 背景は暗めの茶/黒系（リズム/シミュレーション複合カテゴリ）
- ドラムパッドを2行3列配置（World Space SpriteRenderer）
- リング用SpriteRenderer（各パッドの子オブジェクト）

## コンポーネント依存関係
```
DrumKitGameManager
  ├── StageManager (child)
  ├── DrumPadManager (child)  
  └── DrumKitUI → (Canvas)
```

## Buggy Code防止
- `_isActive` ガードで二重処理防止
- Texture2D/Spriteは OnDestroy でクリーンアップ
- タッチ判定は Physics2D.OverlapPoint + Collider2D
