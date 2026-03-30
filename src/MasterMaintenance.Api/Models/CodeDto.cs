using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>コードレスポンス DTO（コード種別名を含む）</summary>
public class CodeResponse
{
    public int Id { get; set; }
    public int CodeTypeId { get; set; }
    public string CodeTypeName { get; set; } = string.Empty;
    public string CodeTypeColor { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>コード作成リクエスト DTO</summary>
public class CreateCodeRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CodeTypeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>コード更新リクエスト DTO</summary>
public class UpdateCodeRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CodeTypeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
