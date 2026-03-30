using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>
/// コードレスポンス DTO。コード種別名・バッジ色を含む。
/// </summary>
public class CodeResponse
{
    /// <summary>コード ID（int）</summary>
    public int Id { get; set; }

    /// <summary>コード種別 ID（int）</summary>
    public int CodeTypeId { get; set; }

    /// <summary>コード種別名（string）</summary>
    public string CodeTypeName { get; set; } = string.Empty;

    /// <summary>コード種別バッジ色（string, Bootstrap 色クラス名）</summary>
    public string CodeTypeColor { get; set; } = string.Empty;

    /// <summary>コード値（string）</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>コード名（string）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>表示順（int）</summary>
    public int DisplayOrder { get; set; }

    /// <summary>有効フラグ（bool）</summary>
    public bool IsActive { get; set; }

    /// <summary>作成日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時（DateTime, UTC）</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// コード作成リクエスト DTO。POST /api/codes
/// </summary>
public class CreateCodeRequest
{
    /// <summary>コード種別 ID（int, 必須, 1以上）</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int CodeTypeId { get; set; }

    /// <summary>コード値（string, 必須, max:50, 例: "DEPT_01"）</summary>
    [Required]
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;

    /// <summary>コード名（string, 必須, max:100, 例: "営業部"）</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>表示順（int, 必須, 1以上）</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int DisplayOrder { get; set; }

    /// <summary>有効フラグ（bool, 既定: true）</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// コード更新リクエスト DTO。PUT /api/codes/{id}
/// </summary>
public class UpdateCodeRequest
{
    /// <summary>コード種別 ID（int, 必須, 1以上）</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int CodeTypeId { get; set; }

    /// <summary>コード値（string, 必須, max:50）</summary>
    [Required]
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;

    /// <summary>コード名（string, 必須, max:100）</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>表示順（int, 必須, 1以上）</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int DisplayOrder { get; set; }

    /// <summary>有効フラグ（bool, 既定: true）</summary>
    public bool IsActive { get; set; } = true;
}
