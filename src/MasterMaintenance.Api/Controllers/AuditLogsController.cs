using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "admin")]
public class AuditLogsController(AppDbContext db) : ControllerBase
{
    /// <summary>一覧取得（検索 + ページネーション）</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> GetAuditLogs(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? action,
        [FromQuery] string? tableName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = db.AuditLogs.AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
        {
            // to の日付の終わりまで含める
            var toEnd = to.Value.Date.AddDays(1);
            query = query.Where(a => a.CreatedAt < toEnd);
        }

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(tableName))
            query = query.Where(a => a.TableName == tableName);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => ToResponse(a))
            .ToListAsync();

        return Ok(new PagedResponse<AuditLogResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    /// <summary>ログファイルエクスポート</summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = db.AuditLogs.AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
        {
            var toEnd = to.Value.Date.AddDays(1);
            query = query.Where(a => a.CreatedAt < toEnd);
        }

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("日時,操作者ID,操作,対象テーブル,レコードID,変更内容");

        foreach (var log in logs)
        {
            var changes = log.Changes.Replace("\"", "\"\"");
            sb.AppendLine($"{log.CreatedAt:yyyy-MM-dd HH:mm:ss},{log.UserId},{log.Action},{log.TableName},{log.RecordId},\"{changes}\"");
        }

        var fromStr = from?.ToString("yyyyMMdd") ?? "all";
        var toStr = to?.ToString("yyyyMMdd") ?? "all";
        var fileName = $"audit-log_{fromStr}_{toStr}.log";

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
    }

    private static AuditLogResponse ToResponse(AuditLog a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        Action = a.Action,
        TableName = a.TableName,
        RecordId = a.RecordId,
        Changes = a.Changes,
        CreatedAt = a.CreatedAt,
    };
}
