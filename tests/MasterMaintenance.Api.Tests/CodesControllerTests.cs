using System.Net;
using System.Net.Http.Json;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Tests;

public class CodesControllerTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;

    public CodesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task GetCodes_ReturnsSeededData()
    {
        var response = await _client.GetAsync("/api/codes");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CodeResponse>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetCodes_FilterByCodeTypeId_ReturnsFilteredResults()
    {
        // コード種別 1（部門）でフィルタ
        var response = await _client.GetAsync("/api/codes?codeTypeId=1");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CodeResponse>>();
        Assert.NotNull(result);
        Assert.All(result.Items, c => Assert.Equal(1, c.CodeTypeId));
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task CreateCode_Returns201()
    {
        var request = new CreateCodeRequest
        {
            CodeTypeId = 1,
            Value = "DEPT_99",
            Name = "テスト部門",
            DisplayOrder = 99,
            IsActive = true,
        };

        var response = await _client.PostAsJsonAsync("/api/codes", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var code = await response.Content.ReadFromJsonAsync<CodeResponse>();
        Assert.NotNull(code);
        Assert.Equal("DEPT_99", code.Value);
        Assert.Equal("テスト部門", code.Name);
        Assert.Equal("部門", code.CodeTypeName);
    }

    [Fact]
    public async Task UpdateCode_Returns200()
    {
        // テスト用にコードを作成
        var createReq = new CreateCodeRequest
        {
            CodeTypeId = 2,
            Value = "ROLE_UPD",
            Name = "更新前",
            DisplayOrder = 10,
            IsActive = true,
        };
        var createRes = await _client.PostAsJsonAsync("/api/codes", createReq);
        var created = await createRes.Content.ReadFromJsonAsync<CodeResponse>();

        var request = new UpdateCodeRequest
        {
            CodeTypeId = 2,
            Value = "ROLE_UPD",
            Name = "更新後",
            DisplayOrder = 11,
            IsActive = false,
        };

        var response = await _client.PutAsJsonAsync($"/api/codes/{created!.Id}", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var code = await response.Content.ReadFromJsonAsync<CodeResponse>();
        Assert.NotNull(code);
        Assert.Equal("更新後", code.Name);
        Assert.Equal(11, code.DisplayOrder);
        Assert.False(code.IsActive);
    }

    [Fact]
    public async Task DeleteCode_Returns204()
    {
        // テスト用にコードを作成
        var createReq = new CreateCodeRequest
        {
            CodeTypeId = 3,
            Value = "STATUS_DEL",
            Name = "削除対象",
            DisplayOrder = 99,
            IsActive = true,
        };
        var createRes = await _client.PostAsJsonAsync("/api/codes", createReq);
        var created = await createRes.Content.ReadFromJsonAsync<CodeResponse>();

        var response = await _client.DeleteAsync($"/api/codes/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // 削除後に取得すると 404
        var getResponse = await _client.GetAsync($"/api/codes/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCode_WithMissingFields_Returns400()
    {
        var request = new { };
        var response = await _client.PostAsJsonAsync("/api/codes", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
