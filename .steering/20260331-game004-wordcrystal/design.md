# 設計: Game004_WordCrystal

## 名前空間

全スクリプトに `namespace Game004_WordCrystal` を付与する。

## スクリプト構成

| クラス | ファイル | 責務 |
|--------|----------|------|
| WordCrystalGameManager | WordCrystalGameManager.cs | ゲーム状態管理、ミスカウント、ステージ遷移 |
| CrystalManager | CrystalManager.cs | 盤面生成、入力処理（一元管理）、文字配置、正解判定 |
| CrystalController | CrystalController.cs | クリスタル1つのデータ保持（文字、砕き状態）、表示更新 |
| WordCrystalUI | WordCrystalUI.cs | 目標単語スロット表示、ミス表示、クリア/ゲームオーバーパネル |

## 入力処理フロー（CrystalManager に一元管理）

1. Mouse.current.leftButton.wasPressedThisFrame でクリック検出
2. Camera.ScreenToWorldPoint → Physics2D.OverlapPoint でクリスタルヒットテスト
3. 砕き済みならスキップ
4. クリスタルを砕いて文字を表示
5. 目標単語の次の文字と一致 → スロットに追加、次へ
6. 不一致 → ミスカウント加算、GameManagerに通知

## SceneSetup 構成方針

Setup001_BlockFlow パターンに準拠。
