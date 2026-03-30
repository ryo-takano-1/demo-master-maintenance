namespace MasterMaintenance.Api.Models;

/// <summary>操作ログレスポンス DTO</summary>
public class AuditLogResponse
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
