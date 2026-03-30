using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>
/// ログインリクエスト DTO。POST /api/auth/login
/// </summary>
public class LoginRequest
{
    /// <summary>メールアドレス（string, 必須, EmailAddress 形式）</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>パスワード（string, 必須, 平文で送信）</summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// ログインレスポンス DTO。POST /api/auth/login
/// </summary>
public class LoginResponse
{
    /// <summary>JWT トークン（string）</summary>
    public string Token { get; set; } = string.Empty;
}
