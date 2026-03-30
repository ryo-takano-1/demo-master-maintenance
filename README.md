# マスタメンテナンス

マスタデータ（ユーザー、コードなど）を管理する Web アプリケーション。

## 現在の状態

.NET Web API バックエンド構築中（Phase 4 完了）。ユーザーマスタ・コードマスタの CRUD API が稼働し、フロントエンドと API 連携済み。

## 画面一覧

| 画面 | ファイル | 概要 |
|------|----------|------|
| ユーザーマスタ | `index.html` | ユーザーの一覧・検索・登録・編集・削除 |
| コードマスタ | `codes.html` | コード値の一覧・検索・登録・編集・削除、コード種別の管理 |
| 操作ログ | `audit-logs.html` | 操作ログの一覧・検索・エクスポート（未実装） |

## 技術スタック

- **バックエンド**: .NET 10 Web API + Entity Framework Core + SQLite
- **フロントエンド**: HTML5 + Bootstrap 5.3.3（CDN） + vanilla JavaScript
- **テスト**: xUnit + WebApplicationFactory

## 開発方法

```bash
# 起動
cd src/MasterMaintenance.Api
dotnet run

# テスト
dotnet test

# ビルドのみ
dotnet build
```

`dotnet run` 後、`http://localhost:5062` でアクセス（初回起動時に DB 自動作成 + シードデータ投入）。

## API エンドポイント

| メソッド | パス | 概要 |
|---------|------|------|
| GET | `/api/users` | ユーザー一覧（検索・ページネーション対応） |
| GET/POST/PUT/DELETE | `/api/users/{id}` | ユーザー CRUD |
| GET | `/api/code-types` | コード種別一覧 |
| GET/POST/PUT/DELETE | `/api/code-types/{id}` | コード種別 CRUD |
| GET | `/api/codes` | コード一覧（検索・ページネーション対応） |
| GET/POST/PUT/DELETE | `/api/codes/{id}` | コード CRUD |
