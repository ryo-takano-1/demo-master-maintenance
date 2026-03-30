using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>
/// ユーザーレスポンス DTO。PasswordHash を除外して返却する。
/// </summary>
public class UserResponse
{
    /// <summary>ユーザー ID（string）</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>ユーザー名（string）</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>メールアドレス（string）</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>ロール（string, "admin" | "editor" | "viewer"）</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>有効フラグ（bool）</summary>
    public bool IsActive { get; set; }

    /// <summary>作成日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時（DateTime, UTC）</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// ユーザー作成リクエスト DTO。POST /api/users
/// </summary>
public class CreateUserRequest
{
    /// <summary>ユーザー ID（string, 必須, max:10, 例: "U006"）</summary>
    [Required]
    [MaxLength(10)]
    public string Id { get; set; } = string.Empty;

    /// <summary>ユーザー名（string, 必須, max:100）</summary>
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>メールアドレス（string, 必須, max:256, EmailAddress 形式）</summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>パスワード（string, 必須, min:8, 平文で送信 → サーバー側で BCrypt 化）</summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>ロール（string, 必須, "admin" | "editor" | "viewer"）</summary>
    [Required]
    [RegularExpression("^(admin|editor|viewer)$", ErrorMessage = "Role は admin, editor, viewer のいずれかを指定してください。")]
    public string Role { get; set; } = string.Empty;

    /// <summary>有効フラグ（bool, 既定: true）</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// ユーザー更新リクエスト DTO。PUT /api/users/{id}
/// </summary>
public class UpdateUserRequest
{
    /// <summary>ユーザー名（string, 必須, max:100）</summary>
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>メールアドレス（string, 必須, max:256, EmailAddress 形式）</summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>パスワード（string?, min:8, 省略時は変更なし）</summary>
    [MinLength(8)]
    public string? Password { get; set; }

    /// <summary>ロール（string, 必須, "admin" | "editor" | "viewer", admin 以外はロール変更無視）</summary>
    [Required]
    [RegularExpression("^(admin|editor|viewer)$", ErrorMessage = "Role は admin, editor, viewer のいずれかを指定してください。")]
    public string Role { get; set; } = string.Empty;

    /// <summary>有効フラグ（bool, 既定: true）</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// ページネーション付きレスポンス DTO。全一覧 API で共通使用。
/// </summary>
/// <typeparam name="T">レスポンス項目の型</typeparam>
public class PagedResponse<T>
{
    /// <summary>データ一覧（List&lt;T&gt;）</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>総件数（int）</summary>
    public int TotalCount { get; set; }

    /// <summary>現在のページ番号（int, 1始まり）</summary>
    public int Page { get; set; }

    /// <summary>1ページあたりの件数（int）</summary>
    public int PageSize { get; set; }
}
