using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

/// <summary>
/// 操作ログの閲覧・エクスポートを提供する API コントローラー。
/// </summary>
/// <remarks>
/// <para>認証: JWT Bearer 必須（全エンドポイント）</para>
/// <para>認可: admin ロールのみ</para>
/// <para>対応画面: audit-logs.html（操作ログ）</para>
/// </remarks>
[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "admin")]
public class AuditLogsController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// 操作ログ一覧を検索条件付きで取得する。
    /// </summary>
    /// <param name="from">開始日時（DateTime?, 以降を取得、省略可）</param>
    /// <param name="to">終了日時（DateTime?, 当日末まで含む、省略可）</param>
    /// <param name="action">操作種別（string?, 完全一致、省略可, "Create" | "Update" | "Delete"）</param>
    /// <param name="tableName">対象テーブル名（string?, 完全一致、省略可）</param>
    /// <param name="page">ページ番号（int, 1始まり、既定: 1）</param>
    /// <param name="pageSize">1ページあたりの件数（int, 既定: 20）</param>
    /// <returns>ActionResult&lt;PagedResponse&lt;AuditLogResponse&gt;&gt; — ページネーション付き操作ログ一覧</returns>
    /// <response code="200">検索結果を返す</response>
    /// <response code="403">admin ロール以外</response>
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

    /// <summary>
    /// 操作ログを CSV 形式のファイルとしてエクスポートする。
    /// </summary>
    /// <param name="from">開始日時（DateTime?, 省略可）</param>
    /// <param name="to">終了日時（DateTime?, 当日末まで含む、省略可）</param>
    /// <returns>IActionResult — CSV ファイル（text/csv, .log 拡張子）</returns>
    /// <response code="200">ファイルを返す</response>
    /// <response code="403">admin ロール以外</response>
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

    /// <summary>
    /// AuditLog エンティティを AuditLogResponse DTO に変換する。
    /// </summary>
    /// <param name="a">変換元エンティティ（AuditLog）</param>
    /// <returns>AuditLogResponse — レスポンス DTO</returns>
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
