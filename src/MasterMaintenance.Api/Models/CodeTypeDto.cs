using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>コード種別レスポンス DTO</summary>
public class CodeTypeResponse
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>コード種別作成リクエスト DTO</summary>
public class CreateCodeTypeRequest
{
    [Required]
    [MaxLength(50)]
    [RegularExpression("^[A-Z_]+$", ErrorMessage = "Key は大文字英字とアンダースコアのみ使用できます。")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Color { get; set; } = "secondary";
}

/// <summary>コード種別更新リクエスト DTO</summary>
public class UpdateCodeTypeRequest
{
    [Required]
    [MaxLength(50)]
    [RegularExpression("^[A-Z_]+$", ErrorMessage = "Key は大文字英字とアンダースコアのみ使用できます。")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Color { get; set; } = "secondary";
}
