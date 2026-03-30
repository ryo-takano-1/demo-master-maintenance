using System.Net;
using System.Net.Http.Json;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Tests;

public class AuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthorizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // --- viewer ---

    [Fact]
    public async Task Viewer_CanGetUsers()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync("suzuki.ichiro@example.com");

        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotPostUsers()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync("suzuki.ichiro@example.com");

        var request = new CreateUserRequest
        {
            Id = "VWER", UserName = "Viewer Test", Email = "viewer-test@example.com",
            Password = "Pass123!", Role = "viewer", IsActive = true,
        };

        var response = await client.PostAsJsonAsync("/api/users", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- editor ---

    [Fact]
    public async Task Editor_CanPostCodes()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync("tanaka.hanako@example.com");

        var request = new CreateCodeRequest
        {
            CodeTypeId = 1, Value = "DEPT_ED", Name = "Editor部門",
            DisplayOrder = 99, IsActive = true,
        };

        var response = await client.PostAsJsonAsync("/api/codes", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Editor_CannotPostUsers()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync("tanaka.hanako@example.com");

        var request = new CreateUserRequest
        {
            Id = "EDTR", UserName = "Editor Test", Email = "editor-test@example.com",
            Password = "Pass123!", Role = "viewer", IsActive = true,
        };

        var response = await client.PostAsJsonAsync("/api/users", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- admin ---

    [Fact]
    public async Task Admin_CanPerformAllOperations()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync("sato.taro@example.com");

        // GET users
        var getResponse = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // POST users
        var createRequest = new CreateUserRequest
        {
            Id = "ADMN", UserName = "Admin Test", Email = "admin-all-test@example.com",
            Password = "Pass123!", Role = "viewer", IsActive = true,
        };
        var postResponse = await client.PostAsJsonAsync("/api/users", createRequest);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        // PUT users
        var updateRequest = new UpdateUserRequest
        {
            UserName = "Admin Updated", Email = "admin-all-test@example.com",
            Role = "editor", IsActive = true,
        };
        var putResponse = await client.PutAsJsonAsync("/api/users/ADMN", updateRequest);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        // DELETE users
        var deleteResponse = await client.DeleteAsync("/api/users/ADMN");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
