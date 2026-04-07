# 開発ガイドライン (Development Guidelines)

## 基本方針

このプロジェクトは **非エンジニアがClaude Codeと協力してUnityゲームを量産する** ワークフローです。
コードはClaude Codeが生成するため、ガイドラインは主に以下を対象とします:

- Claude Codeが生成するC#コードの品質規約
- 非エンジニアとClaude Codeの協働ルール
- Git運用・進捗管理の標準手順

---

## C# コーディング規約（Claude Code生成コードの基準）

### 命名規則

| 対象 | 規則 | 例 |
|------|------|----|
| クラス名 | PascalCase | `BlockFlowGameManager` |
| メソッド名 | PascalCase | `StartGame()`, `OnBlockSwipe()` |
| フィールド（private） | _camelCase（アンダースコア接頭辞） | `_score`, `_isGameOver` |
| プロパティ（public） | PascalCase | `Score`, `IsGameOver` |
| 定数 | UPPER_SNAKE_CASE | `MAX_BLOCK_COUNT` |
| 名前空間 | `Game<ID>_<Title>`（classic）/ `Game<ID>v2_<Title>`（remake） | `namespace Game001_BlockFlow` / `namespace Game001v2_BlockFlow` |

```csharp
// ✅ 良い例（classic）
namespace Game001_BlockFlow
{
    public class BlockFlowGameManager : MonoBehaviour
    {
        private const int MAX_BLOCK_COUNT = 25;

        [SerializeField] private int _boardSize = 5;
        private bool _isGameOver = false;

        public int Score { get; private set; }

        public void StartGame()
        {
            Score = 0;
            _isGameOver = false;
        }
    }
}

// ❌ 悪い例
public class manager : MonoBehaviour
{
    public int s;
    bool gameover;
    public void start() { }
}
```

---

### ゲームごとの独立性（最重要ルール）

各ゲームのスクリプトは他のゲームのスクリプトに依存してはいけない。

```csharp
// ✅ 良い例: Common のみに依存（Common は namespace なし、using 不要）
using UnityEngine;

namespace Game001_BlockFlow
{
    public class BlockFlowGameManager : MonoBehaviour
    {
        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu(); // Common の共通機能を使う
        }
    }
}

// ❌ 悪い例: 他ゲームのスクリプトに依存
using Game002_MirrorMaze; // 絶対禁止
```

---

### MonoBehaviour の構造

Unity標準のライフサイクルに沿った順序で記述する。

```csharp
public class BlockFlowGameManager : MonoBehaviour
{
    // 1. 定数
    private const int BOARD_SIZE = 5;

    // 2. SerializeField（インスペクタ公開フィールド）
    [SerializeField] private BlockController _blockPrefab;
    [SerializeField] private TextMeshProUGUI _scoreText;

    // 3. プロパティ
    public int Score { get; private set; }

    // 4. Unityライフサイクル（Awake → Start → Update の順）
    private void Awake() { /* 初期化 */ }
    private void Start() { /* ゲーム開始時 */ }
    private void Update() { /* 毎フレーム */ }

    // 5. publicメソッド
    public void StartGame() { }

    // 6. privateメソッド
    private void UpdateScore(int delta) { }
}
```

---

### コメント規約

**コメントは「なぜ」を書く。「何をしているか」はコードを見ればわかる。**

```csharp
// ✅ 良い例: なぜそうするかを説明
// Unity の物理演算と競合するため、位置更新はFixedUpdateで行う
private void FixedUpdate()
{
    _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
}

// ❌ 悪い例: コードの繰り返し
// スコアを更新する
Score += delta;
_scoreText.text = Score.ToString();
```

**SerializeFieldには日本語コメントで用途を説明する（非エンジニアがインスペクタで確認できるように）**:

```csharp
[SerializeField, Tooltip("ゲームボードの横・縦のマス数")]
private int _boardSize = 5;

[SerializeField, Tooltip("クリア時に表示するパネル")]
private GameObject _clearPanel;
```

---

### エラーハンドリング

```csharp
// ✅ 良い例: Debug.LogError で問題を明確にする
private void LoadGameData()
{
    var registry = Resources.Load<TextAsset>("GameRegistry");
    if (registry == null)
    {
        Debug.LogError("[GameRegistry] GameRegistry.json が Resources/ に見つかりません");
        return;
    }
    // 処理続行
}

// ❌ 悪い例: エラーを無視する
private void LoadGameData()
{
    var registry = Resources.Load<TextAsset>("GameRegistry");
    // null チェックなし → NullReferenceException が発生
}
```

---

## ワークフローガイド（非エンジニア向け）

### 新しいゲームを作るフロー

```
Step 1: GitHub Projects でゲームを選ぶ
  → ステータス「未着手」× 工数「S」でフィルター
  → 1つ選んでステータスを「作業中」に変更

Step 2: Claude Code に依頼する（remakeモード推奨）
  > 「ゲーム001 BlockFlow を作って（remakeモード）」
  → GameRegistry.json の remake エントリの implemented を true に更新
  → Game001v2_BlockFlow/ フォルダとスクリプト群を自動生成・push まで完了

Step 3: ゲームアセット（画像）を生成する
  > 「ゲーム001のアセットを生成して」
  → Gemini CLI で各スプライト画像を自動生成
  → MiniGameCollection/Assets/Sprites/Game001v2_BlockFlow/ に保存

Step 4: Unity Editor で確認する
  → Unity Hub でプロジェクトを開く（初回のみ）
  → Assets > Setup > 001v2 BlockFlow を実行（remake版）
  → Play ボタンを押して動作確認

Step 5: 問題があれば Claude Code に伝える
  > 「ブロックが消えないバグを直して」
  > 「スコアをもっと大きく表示して」
  > 「ブロックの画像をもっとキラキラさせて」

Step 6: 完成したら GitHub Projects を更新
  → ステータスを「完成」に変更
  → Issue をクローズ
```

---

### Claude Code への依頼の書き方

**基本形**: 「ゲーム\<ID\> \<タイトル\> を作って」

| 依頼内容 | 例 |
|---------|-----|
| 新規ゲーム作成 | `「ゲーム001 BlockFlow を作って」` |
| バグ修正 | `「ブロックが消えないバグを直して」` |
| 調整・改善 | `「落下速度を2倍にして」` |
| 仕様確認 | `「001 BlockFlow の仕様を教えて」` |

**困ったときは**: 状況をそのまま伝えてOK
> 「Unity を開いたら赤いエラーがたくさん出た」
> 「Play ボタンを押しても何も動かない」

---

## Git 運用ルール

### ブランチ戦略

```
main
  └─ feature/game-<ID>-<title>    # 各ゲームの実装ブランチ
  └─ fix/<内容>                   # バグ修正
  └─ chore/<内容>                 # GameRegistry更新等の雑務
```

**例**:
- `feature/game-001-block-flow`
- `feature/game-021-blade-dash`
- `fix/game-001-block-disappear-bug`

---

### コミットメッセージ規約

**フォーマット**: `<type>(<scope>): <内容>`

| Type | 用途 | 例 |
|------|------|----|
| `feat` | 新しいゲームの追加 | `feat(game-001): BlockFlow を追加` |
| `fix` | バグ修正 | `fix(game-001): ブロック消去ロジックを修正` |
| `chore` | GameRegistry 更新 | `chore: GameRegistry に 001-010 を追加` |
| `docs` | ドキュメント | `docs: GETTING_STARTED を更新` |
| `style` | コードフォーマット | `style(game-001): インデントを統一` |

---

### PR（プルリクエスト）プロセス

1ゲーム = 1PRを基本とする。

**PRタイトル**: `feat: [001] BlockFlow を実装`

**PRチェックリスト**:
```markdown
## 動作確認
- [ ] Unity Editor でコンパイルエラーなし
- [ ] SceneSetup スクリプトでシーン構成完了
- [ ] Play ボタンでゲームが起動する
- [ ] コアメカニクスが動作する
- [ ] 「メニューへ戻る」でTopMenuに戻れる
- [ ] Console にエラーログなし

## GitHub Projects
- [ ] ステータスを「完成」に更新
- [ ] Issue をクローズ
- [ ] GameRegistry.json に implemented: true で追加済み

## 関連 Issue
Closes #<Issue番号>
```

---

## 動作確認チェックリスト（各ゲーム共通）

### Unity 動作確認

```
必須確認:
- [ ] Play ボタンで起動する（クラッシュしない）
- [ ] コアメカニクスが動く
- [ ] クリア/ゲームオーバーが発動する
- [ ] ゲームリスタートができる
- [ ] 「メニューへ戻る」でTopMenuに戻れる
- [ ] Console にエラーログが出ない

TopMenu確認（新ゲーム追加時）:
- [ ] カテゴリタブに新しいゲームカードが表示される
- [ ] カードをタップするとゲームシーンに遷移する
```

---

## 開発環境セットアップ

### 必要なツール

| ツール | バージョン | インストール方法 |
|--------|-----------|-----------------|
| Unity Hub | 最新版 | https://unity.com/ja/download |
| Unity | 6000.x LTS | Unity Hub から |
| Git | 最新版 | https://git-scm.com/ |
| GitHub CLI (gh) | 最新版 | `brew install gh` (Mac) |
| Claude Code | 最新版 | `npm install -g @anthropic-ai/claude-code` |

### 初回セットアップ手順

```bash
# 1. リポジトリをクローン
git clone https://github.com/OumeiSatoKenta/cc_unity_maker.git
cd cc_unity_maker

# 2. GitHub CLI の認証
gh auth login

# 3. Claude Code の起動
claude

# 4. Unity Hub でプロジェクトを開く
# Unity Hub > Add project > cc_unity_maker/MiniGameCollection/ を選択

# 5. 日本語フォントをセットアップ（初回のみ）
# Unity Editor で Assets > Setup > Generate Japanese Font を実行

# 6. TopMenuシーンを構成（初回のみ）
# Unity Editor で Assets > Setup > TopMenu を実行

# 7. 最初のゲームを作る
# Claude Code に「ゲーム001 BlockFlow を作って」と伝える
```

詳細手順は `docs/GETTING_STARTED.md` を参照。

### Unity Editor 推奨設定

#### PlayMode 移行の高速化

`Edit > Project Settings > Editor > Enter Play Mode Options` を開き、以下を設定する:

| 設定項目 | 推奨値 | 備考 |
|---------|--------|------|
| Enter Play Mode Options | ON | このチェックを入れると個別設定が有効になる |
| Reload Domain | OFF | 静的変数がリセットされないため注意（下記参照） |
| Reload Scene | OFF | シーン再読み込みをスキップ |

> **効果**: Domain Reload を OFF にすることで、PlayMode 移行が数秒→ほぼ即時になる。スクリプト数が多いほど効果大。

> **注意**: `Reload Domain OFF` にすると `static` フィールドや `SceneLoader.CurrentCollection` のような静的プロパティが前のPlayModeの値を引き継ぐ。不審な挙動が出た場合は一時的に ON に戻して確認すること。

---

## Claude Code が守るべきコード生成ルール

> このセクションはClaude Codeへの指示です。コード生成時に必ず遵守してください。

1. **名前空間を必ず付与する**: classic版は `namespace Game<ID>_<Title>`、remake版は `namespace Game<ID>v2_<Title>` を付ける
2. **Null チェックを徹底する**: `Resources.Load` 等の結果は必ず null チェック
3. **Tooltip を付ける**: `[SerializeField]` には `[Tooltip("説明")]` を併記
4. **Unity 6 API を使う**: 非推奨の旧APIを使わない（例: `FindObjectOfType` → `FindFirstObjectByType`）
5. **SceneSetup で全設定を完結させる**: 非エンジニアがインスペクタを手動設定する場面をゼロにする
6. **GameRegistry.json を必ず更新する**: classic実装時は classicエントリの `implemented: true` を確認（既に true）、remake実装時は remakeエントリの `implemented` を `true` に更新する
7. **ゲーム間依存を作らない**: `using Game002_*` は絶対に書かない
8. **コミットはゲーム単位で行う**: 複数ゲームを1コミットにまとめない
9. **同一GameObjectにTMPとImageを共存させない**: `TextMeshProUGUI` が付いたGameObjectに `Image` コンポーネントを追加すると描画が競合する。背景が必要な場合は親子構造にし、親に `Image`、子に `TextMeshProUGUI` を配置する
10. **NotoSansJP で絵文字を使わない**: `NotoSansJP-Regular SDF` は絵文字グリフを含まないため、絵文字文字（🎯等）をテキストに含めると警告が出る。テキスト代替（「★」「●」等）を使用する
11. **コルーチン内のTransform参照にnullチェックを入れる**: 複数フレームにまたがるコルーチンで `Transform` を操作する場合、オブジェクトが途中で破棄される可能性がある。ループの先頭で `if (t == null) yield break;` を入れること

---

## UI レスポンシブ設計ガイドライン

### 基本原則

モバイル（縦画面）からデスクトップ（横画面）まで、解像度変更時にUI要素が重ならないようにする。

### CanvasScaler 設定（全シーン共通）

```csharp
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1080, 1920); // 縦基準で統一
scaler.matchWidthOrHeight = 0.5f; // 縦横バランスよくスケール
```

- `referenceResolution` は全シーンで `1080x1920` に統一する（横向き `1920x1080` にしない）
- `matchWidthOrHeight = 0.5f` で縦横どちらが狭くなっても破綻しにくくする

### レイアウト手法

| 手法 | 使い所 |
|------|--------|
| **アンカーストレッチ** | テキスト・パネルなど親の幅に追従すべき要素 |
| **LayoutGroup + LayoutElement** | カード列・ボタン列など均等配置 |
| **ScrollRect** | 要素数が画面に収まらない可能性がある場合（タブ一覧等） |
| **enableAutoSizing** | テキストが解像度によって切れる可能性がある場合 |

### 固定サイズ・固定アンカーの禁止

```csharp
// ❌ 固定アンカー+固定サイズ（解像度変更で重なる）
rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.65f);
rect.sizeDelta = new Vector2(700, 180);

// ✅ 相対アンカー（親のサイズに追従）
rect.anchorMin = new Vector2(0.08f, 0.55f);
rect.anchorMax = new Vector2(0.92f, 0.75f);
rect.offsetMin = Vector2.zero;
rect.offsetMax = Vector2.zero;
```

ただし `LayoutElement` で高さを指定し、幅をLayoutGroupに委ねるパターンはOK:

```csharp
// ✅ LayoutGroup内での高さ指定（幅は親が制御）
var le = card.AddComponent<LayoutElement>();
le.preferredHeight = 180;
le.minHeight = 120;
le.flexibleWidth = 1;
```

### テキストの自動サイズ

解像度変更でテキストが切れるのを防ぐため、可変長テキストには `enableAutoSizing` を使用する:

```csharp
tmp.enableAutoSizing = true;
tmp.fontSizeMin = 18;  // 最小フォントサイズ
tmp.fontSizeMax = 48;  // 最大フォントサイズ（デザイン上の理想値）
```

### タブ・ボタン列のスクロール対応

要素数が画面幅を超える可能性がある場合は `ScrollRect` でラップする:

```csharp
// 横スクロール可能なタブコンテナ
var scrollRect = tabScrollObj.AddComponent<ScrollRect>();
scrollRect.horizontal = true;
scrollRect.vertical = false;
scrollRect.movementType = ScrollRect.MovementType.Elastic;

// 中のコンテナはContentSizeFitterで自動伸縮
var fitter = tabContainerObj.AddComponent<ContentSizeFitter>();
fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
```

### ボタンの最小タップ領域

モバイル操作性のため、ボタンの `sizeDelta` の最小値は `(150, 55)` とする。

### 適用済み画面一覧

| 画面 | SceneSetupファイル | 対応状況 |
|------|-------------------|----------|
| CollectionSelect | `SetupCollectionSelect.cs` | VerticalLayoutGroup + autoSizing + matchWidthOrHeight |
| TopMenu | `SetupTopMenu.cs` | ScrollRect タブ + アンカーストレッチタイトル + matchWidthOrHeight |
| 各ゲーム画面（classic） | `Setup[ID]_[Title].cs` | 個別対応（HUD は基本アンカーベース） |
| 各ゲーム画面（remake） | `Setup[ID]v2_[Title].cs` | 個別対応（StageManager/InstructionPanel統合） |
