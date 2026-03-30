namespace MasterMaintenance.Api.Models;

/// <summary>
/// ユーザーマスタのエンティティ。テーブル: Users
/// </summary>
public class User
{
    /// <summary>ユーザー ID（string, PK, max:10, 例: "U001"）</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>ユーザー名（string, 必須, max:100）</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>メールアドレス（string, 必須, max:256, unique）</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>パスワードハッシュ（string, 必須, BCrypt）</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>ロール（string, 必須, max:20, "admin" | "editor" | "viewer"）</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>有効フラグ（bool）</summary>
    public bool IsActive { get; set; }

    /// <summary>作成日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時（DateTime, UTC）</summary>
    public DateTime UpdatedAt { get; set; }
}
