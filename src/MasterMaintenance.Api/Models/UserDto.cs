using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>レスポンス用 DTO（PasswordHash を含まない）</summary>
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>作成リクエスト用 DTO</summary>
public class CreateUserRequest
{
    [Required] public string Id { get; set; } = string.Empty;
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>更新リクエスト用 DTO</summary>
public class UpdateUserRequest
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    [Required] public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>ページネーション付きレスポンス</summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
