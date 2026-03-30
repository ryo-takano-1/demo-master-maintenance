namespace MasterMaintenance.Api.Models;

public class User
{
    public string Id { get; set; } = string.Empty;       // PK（U001 形式）
    public string UserName { get; set; } = string.Empty;  // 必須
    public string Email { get; set; } = string.Empty;     // 必須
    public string PasswordHash { get; set; } = string.Empty; // BCrypt ハッシュ
    public string Role { get; set; } = string.Empty;      // admin / editor / viewer
    public bool IsActive { get; set; }                     // 有効/無効
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
