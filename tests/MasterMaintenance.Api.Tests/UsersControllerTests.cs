using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Tests;

public class UsersControllerTests : IClassFixture<UsersControllerTests.CustomFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomFactory _factory;

    public UsersControllerTests(CustomFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public class CustomFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public CustomFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 既存の DbContext 登録を削除
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // 共有の SQLite インメモリ接続を使用
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                    options.ConfigureWarnings(w =>
                        w.Ignore(RelationalEventId.PendingModelChangesWarning));
                });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            return host;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _connection.Dispose();
        }
    }

    [Fact]
    public async Task GetUsers_ReturnsSeededData()
    {
        var response = await _client.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetUsers_FilterByRole_ReturnsFilteredResults()
    {
        // テスト用にユニークなユーザーを作成してからフィルタ
        var createReq = new CreateUserRequest
        {
            Id = "UADM", UserName = "管理テスト", Email = "admin-filter@example.com",
            Password = "Pass123!", Role = "admin", IsActive = true,
        };
        await _client.PostAsJsonAsync("/api/users", createReq);

        var response = await _client.GetAsync("/api/users?role=admin");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();
        Assert.NotNull(result);
        Assert.All(result.Items, u => Assert.Equal("admin", u.Role));
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task CreateUser_Returns201WithNewUser()
    {
        var request = new CreateUserRequest
        {
            Id = "U099",
            UserName = "テスト ユーザー",
            Email = "test-create@example.com",
            Password = "TestPass123!",
            Role = "viewer",
            IsActive = true,
        };

        var response = await _client.PostAsJsonAsync("/api/users", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal("U099", user.Id);
        Assert.Equal("テスト ユーザー", user.UserName);
        Assert.Equal("test-create@example.com", user.Email);
        Assert.Equal("viewer", user.Role);
    }

    [Fact]
    public async Task UpdateUser_Returns200WithUpdatedData()
    {
        // テスト用にユニークなユーザーを作成
        var createReq = new CreateUserRequest
        {
            Id = "UUPD", UserName = "更新前", Email = "update-before@example.com",
            Password = "Pass123!", Role = "viewer", IsActive = true,
        };
        await _client.PostAsJsonAsync("/api/users", createReq);

        var request = new UpdateUserRequest
        {
            UserName = "更新後ユーザー",
            Email = "update-after@example.com",
            Role = "editor",
            IsActive = false,
        };

        var response = await _client.PutAsJsonAsync("/api/users/UUPD", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal("更新後ユーザー", user.UserName);
        Assert.Equal("update-after@example.com", user.Email);
        Assert.Equal("editor", user.Role);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task DeleteUser_Returns204()
    {
        // テスト用にユニークなユーザーを作成
        var createReq = new CreateUserRequest
        {
            Id = "UDEL", UserName = "削除対象", Email = "delete-target@example.com",
            Password = "Pass123!", Role = "viewer", IsActive = true,
        };
        await _client.PostAsJsonAsync("/api/users", createReq);

        var response = await _client.DeleteAsync("/api/users/UDEL");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // 削除後に取得すると 404
        var getResponse = await _client.GetAsync("/api/users/UDEL");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithMissingFields_Returns400()
    {
        var request = new { };
        var response = await _client.PostAsJsonAsync("/api/users", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
