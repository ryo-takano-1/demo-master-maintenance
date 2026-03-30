using System.Net;
using System.Net.Http.Json;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Tests;

public class CodeTypesControllerTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;

    public CodeTypesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task GetCodeTypes_ReturnsSeededData()
    {
        var response = await _client.GetAsync("/api/code-types");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<CodeTypeResponse>>();
        Assert.NotNull(result);
        Assert.True(result.Count >= 4);
    }

    [Fact]
    public async Task CreateCodeType_Returns201()
    {
        var request = new CreateCodeTypeRequest
        {
            Key = "PRIORITY",
            Name = "優先度",
        };

        var response = await _client.PostAsJsonAsync("/api/code-types", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var codeType = await response.Content.ReadFromJsonAsync<CodeTypeResponse>();
        Assert.NotNull(codeType);
        Assert.Equal("PRIORITY", codeType.Key);
        Assert.Equal("優先度", codeType.Name);
    }

    [Fact]
    public async Task UpdateCodeType_Returns200()
    {
        // テスト用にコード種別を作成
        var createReq = new CreateCodeTypeRequest { Key = "UPD_TEST", Name = "更新前" };
        var createRes = await _client.PostAsJsonAsync("/api/code-types", createReq);
        var created = await createRes.Content.ReadFromJsonAsync<CodeTypeResponse>();

        var request = new UpdateCodeTypeRequest
        {
            Key = "UPD_TEST",
            Name = "更新後",
        };

        var response = await _client.PutAsJsonAsync($"/api/code-types/{created!.Id}", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var codeType = await response.Content.ReadFromJsonAsync<CodeTypeResponse>();
        Assert.NotNull(codeType);
        Assert.Equal("更新後", codeType.Name);
    }

    [Fact]
    public async Task DeleteCodeType_Returns204()
    {
        // 使用されていないコード種別を作成
        var createReq = new CreateCodeTypeRequest { Key = "DEL_TEST", Name = "削除対象" };
        var createRes = await _client.PostAsJsonAsync("/api/code-types", createReq);
        var created = await createRes.Content.ReadFromJsonAsync<CodeTypeResponse>();

        var response = await _client.DeleteAsync($"/api/code-types/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCodeType_InUse_Returns409()
    {
        // シードデータの DEPT（Id=1）は Codes で使用中
        var response = await _client.DeleteAsync("/api/code-types/1");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
