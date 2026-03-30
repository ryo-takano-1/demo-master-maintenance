# マスタメンテナンス

マスタデータ（ユーザー、コードなど）を管理する Web アプリケーション。

## 現在の状態

.NET Web API バックエンド構築中（Phase 7 完了）。CRUD API + JWT 認証 + ロールベース認可 + 操作ログが稼働中。

## 画面一覧

| 画面 | ファイル | 概要 |
|------|----------|------|
| ログイン | `login.html` | メール + パスワードで JWT 認証 |
| ユーザーマスタ | `index.html` | ユーザーの一覧・検索・登録・編集・削除 |
| コードマスタ | `codes.html` | コード値の一覧・検索・登録・編集・削除、コード種別の管理 |
| 操作ログ | `audit-logs.html` | 操作ログの一覧・検索・期間指定エクスポート（admin のみ） |

## 技術スタック

- **バックエンド**: .NET 10 Web API + Entity Framework Core + SQLite
- **フロントエンド**: HTML5 + Bootstrap 5.3.3（CDN） + vanilla JavaScript
- **認証**: JWT Bearer
- **テスト**: xUnit + WebApplicationFactory（30件）

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

### 初期ユーザー

| ID | 名前 | メール | 権限 | パスワード |
|----|------|--------|------|-----------|
| U001 | 佐藤 太郎 | sato.taro@example.com | admin | Password123! |
| U002 | 田中 花子 | tanaka.hanako@example.com | editor | Password123! |
| U003 | 鈴木 一郎 | suzuki.ichiro@example.com | viewer | Password123! |

## API エンドポイント

| メソッド | パス | 認可 | 概要 |
|---------|------|------|------|
| POST | `/api/auth/login` | 不要 | ログイン（JWT 発行） |
| GET | `/api/users` | 全ロール | ユーザー一覧 |
| POST/DELETE | `/api/users/{id}` | admin | ユーザー作成・削除 |
| PUT | `/api/users/{id}` | admin, editor | ユーザー更新 |
| GET | `/api/code-types` | 全ロール | コード種別一覧 |
| POST/PUT/DELETE | `/api/code-types/{id}` | admin, editor | コード種別 CUD |
| GET | `/api/codes` | 全ロール | コード一覧 |
| POST/PUT/DELETE | `/api/codes/{id}` | admin, editor | コード CUD |
| GET | `/api/audit-logs` | admin | 操作ログ一覧 |
| GET | `/api/audit-logs/export` | admin | 操作ログエクスポート（.log） |
