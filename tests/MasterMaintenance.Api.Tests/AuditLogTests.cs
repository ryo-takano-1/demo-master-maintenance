using System.Net;
using System.Net.Http.Json;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Tests;

public class AuditLogTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _adminClient = null!;

    public AuditLogTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _adminClient = await _factory.CreateAuthenticatedClientAsync();
    }

    public Task DisposeAsync()
    {
        _adminClient.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CrudCreatesAuditLog()
    {
        // ユーザー作成
        var createReq = new CreateUserRequest
        {
            Id = "ULOG1",
            UserName = "ログテスト",
            Email = "audit-log-test@example.com",
            Password = "Pass123!",
            Role = "viewer",
            IsActive = true,
        };
        var createRes = await _adminClient.PostAsJsonAsync("/api/users", createReq);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);

        // 操作ログに記録されていることを確認
        var logsRes = await _adminClient.GetAsync("/api/audit-logs?tableName=Users");
        logsRes.EnsureSuccessStatusCode();

        var logs = await logsRes.Content.ReadFromJsonAsync<PagedResponse<AuditLogResponse>>();
        Assert.NotNull(logs);
        Assert.Contains(logs.Items, l =>
            l.Action == "Create" && l.TableName == "Users" && l.RecordId == "ULOG1");
    }

    [Fact]
    public async Task DateFilter_FiltersResults()
    {
        // テスト用にデータを作成
        var createReq = new CreateUserRequest
        {
            Id = "ULOG2",
            UserName = "フィルタテスト",
            Email = "audit-filter-test@example.com",
            Password = "Pass123!",
            Role = "viewer",
            IsActive = true,
        };
        await _adminClient.PostAsJsonAsync("/api/users", createReq);

        // 今日の日付でフィルタ
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var res = await _adminClient.GetAsync($"/api/audit-logs?from={today}&to={today}");
        res.EnsureSuccessStatusCode();

        var logs = await res.Content.ReadFromJsonAsync<PagedResponse<AuditLogResponse>>();
        Assert.NotNull(logs);
        Assert.True(logs.TotalCount >= 1);

        // 未来の日付でフィルタ（結果0件）
        var futureDate = DateTime.UtcNow.AddYears(10).ToString("yyyy-MM-dd");
        var futureRes = await _adminClient.GetAsync($"/api/audit-logs?from={futureDate}&to={futureDate}");
        futureRes.EnsureSuccessStatusCode();

        var futureLogs = await futureRes.Content.ReadFromJsonAsync<PagedResponse<AuditLogResponse>>();
        Assert.NotNull(futureLogs);
        Assert.Equal(0, futureLogs.TotalCount);
    }

    [Fact]
    public async Task Export_ReturnsCsv()
    {
        // テスト用にデータを作成
        var createReq = new CreateUserRequest
        {
            Id = "ULOG3",
            UserName = "エクスポートテスト",
            Email = "audit-export-test@example.com",
            Password = "Pass123!",
            Role = "viewer",
            IsActive = true,
        };
        await _adminClient.PostAsJsonAsync("/api/users", createReq);

        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var res = await _adminClient.GetAsync($"/api/audit-logs/export?from={today}&to={today}");
        res.EnsureSuccessStatusCode();

        Assert.Equal("text/csv", res.Content.Headers.ContentType?.MediaType);

        var content = await res.Content.ReadAsStringAsync();
        Assert.Contains("日時,操作者ID,操作,対象テーブル,レコードID,変更内容", content);
        Assert.Contains("Create", content);
    }

    [Fact]
    public async Task ViewerCannotAccessAuditLogs()
    {
        // viewer ユーザーで認証
        var viewerClient = await _factory.CreateAuthenticatedClientAsync("suzuki.ichiro@example.com");

        var res = await viewerClient.GetAsync("/api/audit-logs");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);

        viewerClient.Dispose();
    }
}
