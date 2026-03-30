using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>ログインリクエスト用 DTO</summary>
public class LoginRequest
{
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

/// <summary>ログインレスポンス用 DTO</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}
