# タスクリスト: UIレスポンシブ対応

## フェーズ1: 問題の特定と修正

- [x] CollectionSelect画面: 固定anchor+固定サイズのカード → VerticalLayoutGroup+LayoutElement方式に変更
- [x] CollectionSelect画面: matchWidthOrHeight=0.5f 追加
- [x] CollectionSelect画面: テキストにenableAutoSizing追加
- [x] TopMenu画面: referenceResolutionを1080x1920に統一（1920x1080から変更）
- [x] TopMenu画面: matchWidthOrHeight=0.5f 追加
- [x] TopMenu画面: タブコンテナをScrollRectでラップ（横スクロール対応）
- [x] TopMenu画面: タイトルをアンカーストレッチ+autoSizingに変更

## フェーズ2: ドキュメント

- [x] development-guidelines.md に「UI レスポンシブ設計ガイドライン」セクション追加

## 実装後の振り返り

### 実装完了日
2026-04-03

### 問題の根本原因
- CollectionSelect: カード3枚が固定anchorで中央に絶対配置されていた。解像度が変わるとカード同士が重なる
- TopMenu: referenceResolutionがCollectionSelectと不統一（横1920x1080 vs 縦1080x1920）。タブ8個が固定幅で、狭い画面ではみ出す

### 修正方針
- 固定anchor → 相対anchor/LayoutGroup に統一
- referenceResolution → 全シーン1080x1920統一 + matchWidthOrHeight=0.5f
- テキスト → enableAutoSizingで解像度適応
- タブ → ScrollRectで横スクロール対応
