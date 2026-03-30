# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

マスタメンテナンス Web アプリケーション。ユーザー・コード・コード種別のマスタデータを管理し、操作ログを記録する。全 Phase 完了済み（CRUD API + JWT 認証 + ロールベース認可 + 操作ログ + セキュリティレビュー対応）。詳細な実装計画は `docs/implementation-plan.md` を参照。

## 技術スタック

- **バックエンド**: .NET 10 Web API + EF Core + SQLite（`app.db`、初回起動時に自動作成＋シード投入）
- **フロントエンド**: HTML5 + Bootstrap 5.3.3 + Bootstrap Icons 1.11.3（CDN）+ vanilla JS（インライン）、カスタム CSS（`css/style.css`）
- **認証**: JWT Bearer（60分有効、BCrypt パスワードハッシュ）
- **認可**: ロールベース（admin / editor / viewer）
- **テスト**: xUnit + WebApplicationFactory + インメモリ SQLite（32件）

## 開発コマンド

```bash
# 起動（http://localhost:5216）
cd src/MasterMaintenance.Api
dotnet run

# テスト全件実行
dotnet test

# ビルドのみ
dotnet build

# 特定テストクラスの実行
dotnet test --filter "FullyQualifiedName~UsersControllerTests"

# 特定テストメソッドの実行
dotnet test --filter "FullyQualifiedName~UsersControllerTests.CreateUser_WithAdminRole_ReturnsCreated"
```

## アーキテクチャ

### バックエンド構成（`src/MasterMaintenance.Api/`）

- **Controllers/**: API コントローラー（Auth, Users, Codes, CodeTypes, AuditLogs）
- **Models/**: エンティティ + DTO（リクエスト / レスポンス分離）。`PagedResponse<T>` で統一的なページネーション
- **Data/AppDbContext.cs**: EF Core DbContext。`OnModelCreating` でシードデータ（ユーザー5件、コード種別4件、コード8件）を投入
- **Migrations/**: EF Core マイグレーション（InitialCreate, AddCodeTypeColor）

### フロントエンド（`src/MasterMaintenance.Api/wwwroot/`）

JS は全ページにインラインで記述（外部 JS ファイルなし）。各ページ共通の処理パターン：
1. **認証チェック**: localStorage の JWT トークン検証 → 未認証なら `login.html` へリダイレクト
2. **`authFetch()` ラッパー**: Authorization ヘッダー付与、401 時に自動リダイレクト
3. **ロールベース UI 制御**: ロールに応じてボタンの表示/非表示を切り替え
4. **CRUD + ページネーション**: テーブル描画、モーダルでのフォーム入力、削除確認

### 共有レイアウト（手動同期が必要）

パーシャルやインクルードの仕組みはない。ナビバー、サイドバー、認証スクリプト、`authFetch()` は全 HTML ファイルに複製されている。**これらを変更する場合は、`login.html`・`index.html`・`codes.html`・`audit-logs.html` すべてを更新すること。**

### ページ構造パターン

各ページは `#main-content` 内で `.page-header` → `.search-area` → `.table-card` のレイアウト。各ページに編集モーダルと削除確認モーダルがある。

### 認可モデル

| 操作 | admin | editor | viewer |
|------|-------|--------|--------|
| 全データの閲覧 | ○ | ○ | ○ |
| ユーザー作成・削除 | ○ | × | × |
| ユーザー更新（ロール変更含む） | ○ | △（ロール変更不可） | × |
| コード / コード種別 CUD | ○ | ○ | × |
| 操作ログ閲覧・エクスポート | ○ | × | × |

### テスト構成（`tests/MasterMaintenance.Api.Tests/`）

`TestWebApplicationFactory` でインメモリ SQLite を使用。テストクラスは `IAsyncLifetime` で DB を初期化。コントローラーごとにテストファイルが分かれている。

## 規約

- ユーザー向けテキストはすべて日本語
- 必須フィールドは `<label>` に `.required` クラス
- ステータスバッジは `.badge-active` / `.badge-inactive`
- テーブルの操作ボタンは `.btn-action`
- モーダルは Bootstrap 5 の `data-bs-*` 属性で制御
- 新規ページ追加時はサイドバーのナビリンクを全ページに追加
- CUD 操作時は `AuditLog` を記録（変更内容を JSON で保存）
- DateTime は常に UTC

## MCP サーバー

`.mcp.json` で設定済み：context7、GitHub、Playwright。
