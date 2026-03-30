namespace MasterMaintenance.Api.Models;

/// <summary>
/// 操作ログレスポンス DTO。GET /api/audit-logs
/// </summary>
public class AuditLogResponse
{
    /// <summary>ログ ID（long）</summary>
    public long Id { get; set; }

    /// <summary>操作者ユーザー ID（string）</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>操作種別（string, "Create" | "Update" | "Delete"）</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>対象テーブル名（string）</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>対象レコードの ID（string）</summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>変更内容（string, JSON 形式）</summary>
    public string Changes { get; set; } = string.Empty;

    /// <summary>操作日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }
}
