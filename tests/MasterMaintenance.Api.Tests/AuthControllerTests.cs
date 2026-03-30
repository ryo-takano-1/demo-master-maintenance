using System.Net;
using System.Net.Http.Json;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Tests;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var request = new LoginRequest
        {
            Email = "sato.taro@example.com",
            Password = "Password123!",
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var request = new LoginRequest
        {
            Email = "sato.taro@example.com",
            Password = "WrongPassword!",
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var request = new LoginRequest
        {
            Email = "nobody@example.com",
            Password = "Password123!",
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInactiveUser_Returns401()
    {
        // U005（渡辺 健二）は IsActive = false のシードデータ
        var request = new LoginRequest
        {
            Email = "watanabe.kenji@example.com",
            Password = "Password123!",
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
