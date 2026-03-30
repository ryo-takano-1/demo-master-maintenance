namespace MasterMaintenance.Api.Models;

/// <summary>
/// 操作ログのエンティティ。テーブル: AuditLogs
/// </summary>
public class AuditLog
{
    /// <summary>ログ ID（long, PK, 自動採番）</summary>
    public long Id { get; set; }

    /// <summary>操作者ユーザー ID（string, 必須, max:10）</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>操作種別（string, 必須, max:20, "Create" | "Update" | "Delete"）</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>対象テーブル名（string, 必須, max:50, 例: "Users"）</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>対象レコードの ID（string, 必須, max:50）</summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>変更内容（string, JSON 形式）</summary>
    public string Changes { get; set; } = string.Empty;

    /// <summary>操作日時（DateTime, UTC）</summary>
    public DateTime CreatedAt { get; set; }
}
