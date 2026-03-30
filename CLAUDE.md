# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

マスタメンテナンス用の静的 HTML フロントエンド。ビルドシステムやバックエンドはなく、すべてのページはプレーン HTML + CDN 読み込みの依存ライブラリで構成された UI プロトタイプ。

## 技術スタック

- **HTML5** — 静的ページ、テンプレートエンジンなし
- **Bootstrap 5.3.3** — レイアウト、コンポーネント、モーダル（CDN経由）
- **Bootstrap Icons 1.11.3** — アイコンセット（CDN経由）
- **CSS** — カスタムスタイルは `css/style.css`
- **JavaScript** — 最小限のインライン `<script>`（サイドバー開閉のみ）

## 開発方法

ビルド・インストール・テストは不要。HTML ファイルをブラウザで直接開いてプレビュー。HTML/CSS を編集してリロード。

## アーキテクチャ

### 共有レイアウト（手動同期が必要）

パーシャルやインクルードの仕組みはなく、以下のセクションを全ページで同一内容で複製している：

1. **`<head>` CDNリンク** — Bootstrap CSS、Bootstrap Icons、`css/style.css`
2. **ヘッダーナビバー** — `<nav class="navbar ...">` サイドバー開閉ボタン、ブランドリンク、ユーザー情報、ログアウトボタン
3. **サイドバーナビ** — `<nav id="sidebar">` メニュー見出しとナビリンク（現在のページのリンクに `.active` を設定）
4. **ナビバースペーサー** — `<div style="height: var(--header-height);"></div>` を `</nav>` 直後に配置（ナビバーが `fixed-top` のため、コンテンツが隠れないようにするスペーサー）
5. **サイドバー開閉スクリプト** — `<body>` 末尾のインライン `<script>` で `collapsed`/`expanded` クラスをトグル
6. **Bootstrap JS バンドル** — Bootstrap 5 JS の CDN `<script>`、開閉スクリプトの直前に配置

**これらを変更する場合は、すべての HTML ファイルを更新すること**（`index.html`、`codes.html`）。

### ページ構造パターン

各ページは `#main-content` 内で以下のレイアウトに従う：

```
.page-header  →  タイトル + 「新規追加」ボタン
.search-area  →  検索フォーム
.table-card   →  .table-info-bar + テーブル + .pagination-area
```

各ページには編集・作成モーダルと削除確認モーダルがある。

### CSS変数

`:root`（`css/style.css`）で定義されるレイアウト変数：`--sidebar-width`、`--sidebar-bg`、`--sidebar-hover`、`--sidebar-active`、`--header-height`。サイドバー、ヘッダー、メインコンテンツのスタイルで参照される。

### レスポンシブ対応

`≤768px`（`css/style.css`）では、サイドバーがデフォルト非表示（`margin-left: -var(--sidebar-width)`）、メインコンテンツが全幅（`margin-left: 0`）、ページヘッダーが縦積みになる。モバイルではサイドバーの表示に `.show` クラスを使用（デスクトップの `.collapsed` トグルとは異なる）。

## ページ一覧

- `index.html` — ユーザーマスタ：ユーザーの CRUD、ID・名前・権限で検索
- `codes.html` — コードマスタ：コード／ルックアップ値の管理、種別・値・名前で検索

## 規約

- ユーザー向けテキストはすべて日本語。
- 必須フィールドは `<label>` に `.required` クラスを付与（CSS `::after` で赤いアスタリスクを表示）。
- ステータスバッジは `.badge-active` / `.badge-inactive` を使用（Bootstrap の色バリアントではない）。
- テーブルの操作ボタンは `.btn-action` でコンパクトサイズにする。
- モーダルは Bootstrap 5 の `data-bs-*` 属性で開閉制御。
- 新規ページ追加時はサイドバーのナビリンクを全ページに追加し、当該ページのリンクに `.active` を設定する。

## MCP サーバー

`.mcp.json` で以下が設定済み：context7（ドキュメント検索）、GitHub、Playwright（ブラウザ操作・テスト）。
