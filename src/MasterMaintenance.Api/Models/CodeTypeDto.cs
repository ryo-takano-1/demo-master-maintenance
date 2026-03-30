using System.ComponentModel.DataAnnotations;

namespace MasterMaintenance.Api.Models;

/// <summary>
/// コード種別レスポンス DTO。GET /api/code-types
/// </summary>
public class CodeTypeResponse
{
    /// <summary>コード種別 ID（int）</summary>
    public int Id { get; set; }

    /// <summary>種別キー（string, 例: "DEPT"）</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>種別名（string, 例: "部門"）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>バッジ色（string, Bootstrap 色クラス名）</summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>作成日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時（DateTime, UTC）</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// コード種別作成リクエスト DTO。POST /api/code-types
/// </summary>
public class CreateCodeTypeRequest
{
    /// <summary>種別キー（string, 必須, max:50, 大文字英字+アンダースコアのみ, 例: "DEPT"）</summary>
    [Required]
    [MaxLength(50)]
    [RegularExpression("^[A-Z_]+$", ErrorMessage = "Key は大文字英字とアンダースコアのみ使用できます。")]
    public string Key { get; set; } = string.Empty;

    /// <summary>種別名（string, 必須, max:100）</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>バッジ色（string, 必須, max:20, Bootstrap 色クラス名, 既定: "secondary"）</summary>
    [Required]
    [MaxLength(20)]
    public string Color { get; set; } = "secondary";
}

/// <summary>
/// コード種別更新リクエスト DTO。PUT /api/code-types/{id}
/// </summary>
public class UpdateCodeTypeRequest
{
    /// <summary>種別キー（string, 必須, max:50, 大文字英字+アンダースコアのみ）</summary>
    [Required]
    [MaxLength(50)]
    [RegularExpression("^[A-Z_]+$", ErrorMessage = "Key は大文字英字とアンダースコアのみ使用できます。")]
    public string Key { get; set; } = string.Empty;

    /// <summary>種別名（string, 必須, max:100）</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>バッジ色（string, 必須, max:20, Bootstrap 色クラス名, 既定: "secondary"）</summary>
    [Required]
    [MaxLength(20)]
    public string Color { get; set; } = "secondary";
}
