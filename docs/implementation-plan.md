# 実装計画書

マスタメンテナンス UI プロトタイプを .NET Web API バックエンド付きのアプリケーションに拡張する。

## 技術スタック

| 項目 | 内容 |
|------|------|
| バックエンド | .NET 10 Web API |
| フロントエンド | 既存の静的 HTML（`wwwroot/` に配置、fetch で API 呼び出し） |
| DB | SQLite（本番は PostgreSQL。EF Core で抽象化） |
| ORM | Entity Framework Core |
| 認証 | JWT Bearer |
| 認可 | ロールベース（管理者 / 編集者 / 閲覧者） |
| パスワード | BCrypt（`BCrypt.Net-Next`） |
| テスト | xUnit + `WebApplicationFactory`（統合テスト中心） |

## ディレクトリ構成

```
c:\devroot\maintenance\
├── src/
│   └── MasterMaintenance.Api/
│       ├── Controllers/
│       ├── Models/
│       ├── Data/
│       ├── wwwroot/          ← 既存 HTML/CSS を移動
│       ├── Program.cs
│       └── MasterMaintenance.Api.csproj
├── tests/
│   └── MasterMaintenance.Api.Tests/
├── docs/
├── MasterMaintenance.sln
├── CLAUDE.md
└── .gitignore
```

## フェーズ

### Phase 1: プロジェクト基盤

- .NET Web API プロジェクト作成、ソリューション構成
- 既存 HTML/CSS を `wwwroot/` に移動、静的ファイル配信設定
- テストプロジェクト作成
- `dotnet run` でフロントがそのまま表示されることを確認

### Phase 2: DB & モデル

- EF Core + SQLite セットアップ
- エンティティ定義（User, Code）
- マイグレーション作成 & 初期シードデータ投入

### Phase 3: ユーザーマスタ API

- `UsersController`（CRUD + 検索 + ページネーション）
- 統合テスト
- フロントエンド改修（fetch で API 呼び出し）

### Phase 4: コードマスタ API

- `CodesController`（CRUD + 検索 + ページネーション）
- 統合テスト
- フロントエンド改修

### Phase 5: 認証

- ログイン API（JWT 発行）
- パスワードハッシュ（BCrypt）
- 認証ミドルウェア設定
- **セキュリティレビュー①**: JWT、パスワード処理、ミドルウェア設定

### Phase 6: 認可

- ロールベースの API アクセス制御（`[Authorize(Roles = "...")]`）
- フロントにログイン画面追加 & トークン管理
- ロールに応じた UI 制御（ボタン非表示など）

### Phase 7: 仕上げ

- CORS 設定
- 入力バリデーション
- エラーハンドリング統一
- **セキュリティレビュー②**: API 全体の最終チェック

## GitHub 運用

| 項目 | 方針 |
|------|------|
| ブランチ | `main` + トピックブランチ（`feature/xxx`, `fix/xxx`） |
| PR | トピックブランチ → `main` へマージ時に作成 |
| Issue | 実装項目ごとに起票 |

## エージェント構成

| エージェント | 役割 | タイミング |
|-------------|------|-----------|
| 進捗管理 | タスク管理・指示出し・進捗報告 | 常駐 |
| 実装担当 | コーディング全般 | 各タスクで起動 |
| セキュリティレビュアー | セキュリティ検査 | Phase 5 完了後 & Phase 7 |
