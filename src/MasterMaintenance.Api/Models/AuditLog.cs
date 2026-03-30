namespace MasterMaintenance.Api.Models;

public class AuditLog
{
    public long Id { get; set; }                           // PK, 自動採番
    public string UserId { get; set; } = string.Empty;     // 操作者
    public string Action { get; set; } = string.Empty;     // Create / Update / Delete
    public string TableName { get; set; } = string.Empty;  // 対象テーブル名
    public string RecordId { get; set; } = string.Empty;   // 対象レコードの ID
    public string Changes { get; set; } = string.Empty;    // 変更内容（JSON）
    public DateTime CreatedAt { get; set; }                // 操作日時
}
