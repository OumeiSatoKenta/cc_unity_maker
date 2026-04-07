# Design: Game083v2 StarChef

## namespace
`Game083v2_StarChef`

## スクリプト構成

### StarChefGameManager.cs
- ゲーム状態管理（Playing / StageClear / Clear / GameOver）
- StageManager・InstructionPanel統合
- CookingManager への指示とUI更新

### CookingManager.cs
- コアメカニクス: 素材選択 → 加熱タイミング → 提供フロー
- タイミングゲージアニメーション（往復するピン）
- ステージ別難易度パラメータ適用（ゲージ幅、制限時間、素材数）
- 失敗カウント管理（3回でゲームオーバー）
- コンボカウンター・スコア計算

### StarChefUI.cs
- ステージ表示「Stage X / 5」
- スコア・コンボ表示
- 加熱ゲージ表示（Sliderで実装）
- 料理評価フィードバック（★1〜★3表示）
- ステージクリアパネル・全クリアパネル・ゲームオーバーパネル

## レシピ・素材設計

### 素材種類（6種）
1. 星の粉（StardDust）- 黄色★
2. 月光ジュース（MoonJuice）- 青◎
3. 銀河ハーブ（GalaxyHerb）- 緑♦
4. 宇宙塩（SpaceSalt）- 白△
5. ネビュラソース（NebulaSauce）- 紫♠
6. 彗星クリーム（CometCream）- 白◇

### レシピ設計（簡略化・ゲームプレイ重視）
各ステージで素材の組み合わせ数が増える。
プレイヤーはボタンUI上の素材アイコンをタップして選択する。
選択した素材がレシピと一致すれば調理開始（加熱ゲージ表示）。

**ステージ1（3レシピ、2素材組み合わせ）:**
- 星のスープ: 星の粉 + 月光ジュース
- 銀河サラダ: 銀河ハーブ + 宇宙塩
- 宇宙プリン: 月光ジュース + 宇宙塩

**ステージ2（+3レシピ、3素材組み合わせ）:**
- 星雲ピザ: 星の粉 + 銀河ハーブ + ネビュラソース
- 月面パスタ: 月光ジュース + ネビュラソース + 宇宙塩
- 彗星デザート: 宇宙塩 + 彗星クリーム + 星の粉

**ステージ3〜5:** さらに組み合わせ複合、同じ素材で複数レシピ対応

## 入力処理フロー

1. プレイヤーが素材ボタンをタップ → CookingManagerで選択リストに追加
2. 選択素材がレシピに一致したら → 加熱ゲージ開始
3. ゲージが最適ゾーンに達したらタップ → 評価計算
4. 評価に応じてスコア加算・コンボ更新
5. 提供ボタン（または自動完成）でUI更新

## StageManager統合
- `OnStageChanged(int stage)` で `CookingManager.SetupStage(config)` を呼ぶ
- ステージ別パラメータ:
  - speedMultiplier: ゲージ速度（1.0〜2.0倍）
  - countMultiplier: 利用可能素材数（1=2種, 2=3種, 3=4種, 4=5種, 5=6種）
  - complexityFactor: ゲージ幅比率（0=広い=簡単, 1=狭い=難しい）

## InstructionPanel内容
- title: "StarChef"
- description: "宇宙の素材で銀河レシピを作ろう"
- controls: "素材をタップして選択、ゲージが光ったらタップ！"
- goal: "レストランを★5ランクに育てよう"

## ビジュアルフィードバック設計
1. **成功時ポップアニメーション**: 料理アイコンが1.0→1.3→1.0にスケール変化（0.2秒）
2. **失敗時赤フラッシュ**: 背景パネルが赤く点滅（0.3秒）
3. **コンボ表示**: コンボ数が増えるたびに文字が大きく弾む
4. **ゲージ最適ゾーン**: ゲージが最適ゾーン通過時にゾーンが光る

## スコアシステム
- 基本スコア: 100pt（最適）/ 60pt（良）/ 20pt（可）
- コンボ乗算: ×1.0（0-2連続）→ ×1.2（3-4連続）→ ×1.5（5-7連続）→ ×2.0（8+連続）
- ステージクリアボーナス: ステージ番号 × 200pt

## レスポンシブ配置
- Camera orthographicSize: 6f
- 上部マージン（HUD）: camSize - 1.2 = y > 4.8
- 下部マージン（UI）: bottomから2.8u = y < -3.2
- ゲーム領域（素材・調理エリア）: y の -3.0 〜 4.0

## SceneSetup構成
- Camera: orthographic, size=6
- Background Sprite Renderer
- Canvas (sortingOrder=10, ScreenSpaceOverlay, 1080x1920)
- GameManager ← StageManager, CookingManager, StarChefUI 配線
- InstructionPanel (フルスクリーンオーバーレイ)
- HUD (Stage表示, スコア, コンボ, 失敗カウント)
- ゲームエリア (加熱ゲージパネル, 素材ボタン4個)
- 結果パネル群 (StageClear, AllClear, GameOver)

## 配線が必要なフィールド (SceneSetupで設定)
- StarChefGameManager: _stageManager, _instructionPanel, _cookingManager, _ui
- CookingManager: (スプライト参照不要、動的生成)
- StarChefUI: 各Text/Slider/Button/Panel参照
