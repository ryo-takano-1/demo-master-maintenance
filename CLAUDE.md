# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

マスタメンテナンス Web アプリケーション。現在は静的 HTML の UI プロトタイプ段階で、.NET Web API バックエンドを構築中。詳細な実装計画は `docs/implementation-plan.md` を参照。

## 技術スタック

- **フロントエンド**: HTML5 + Bootstrap 5.3.3 + Bootstrap Icons 1.11.3（CDN）、カスタム CSS（`css/style.css`）
- **バックエンド（構築中）**: .NET 10 Web API、EF Core、SQLite

## 開発方法

現段階ではビルド不要。HTML ファイルをブラウザで直接開いてプレビュー。

## アーキテクチャ

### 共有レイアウト（手動同期が必要）

パーシャルやインクルードの仕組みはない。ヘッダー、サイドバー、フッタースクリプトは全 HTML ファイルに複製されている。**これらを変更する場合は、すべての HTML ファイルを更新すること。**

### ページ構造パターン

各ページは `#main-content` 内で `.page-header` → `.search-area` → `.table-card` のレイアウト。各ページに編集モーダルと削除確認モーダルがある。

## ページ一覧

- `index.html` — ユーザーマスタ
- `codes.html` — コードマスタ

## 規約

- ユーザー向けテキストはすべて日本語
- 必須フィールドは `<label>` に `.required` クラス
- ステータスバッジは `.badge-active` / `.badge-inactive`
- テーブルの操作ボタンは `.btn-action`
- モーダルは Bootstrap 5 の `data-bs-*` 属性で制御
- 新規ページ追加時はサイドバーのナビリンクを全ページに追加

## MCP サーバー

`.mcp.json` で設定済み：context7、GitHub、Playwright。
