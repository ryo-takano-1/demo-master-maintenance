using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

/// <summary>
/// コード情報の CRUD 操作を提供する API コントローラー。
/// </summary>
/// <remarks>
/// <para>認証: JWT Bearer 必須（全エンドポイント）</para>
/// <para>認可: 閲覧は全ロール、作成・更新・削除は admin + editor</para>
/// <para>対応画面: codes.html（コードマスタ）</para>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CodesController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// コード一覧を検索条件付きで取得する。
    /// </summary>
    /// <param name="codeTypeId">コード種別 ID（int?, 完全一致、省略可）</param>
    /// <param name="value">コード値（string?, 部分一致、省略可）</param>
    /// <param name="name">コード名（string?, 部分一致、省略可）</param>
    /// <param name="page">ページ番号（int, 1始まり、既定: 1）</param>
    /// <param name="pageSize">1ページあたりの件数（int, 既定: 10）</param>
    /// <returns>ActionResult&lt;PagedResponse&lt;CodeResponse&gt;&gt; — ページネーション付きコード一覧</returns>
    /// <response code="200">検索結果を返す</response>
    /// <response code="401">未認証</response>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CodeResponse>>> GetCodes(
        [FromQuery] int? codeTypeId,
        [FromQuery] string? value,
        [FromQuery] string? name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = db.Codes.Include(c => c.CodeType).AsQueryable();

        if (codeTypeId.HasValue)
            query = query.Where(c => c.CodeTypeId == codeTypeId.Value);

        if (!string.IsNullOrWhiteSpace(value))
            query = query.Where(c => c.Value.Contains(value));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.CodeTypeId)
            .ThenBy(c => c.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => ToResponse(c))
            .ToListAsync();

        return Ok(new PagedResponse<CodeResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    /// <summary>
    /// 指定 ID のコードを1件取得する。
    /// </summary>
    /// <param name="id">コード ID（int）</param>
    /// <returns>ActionResult&lt;CodeResponse&gt; — コード情報</returns>
    /// <response code="200">コードを返す</response>
    /// <response code="404">指定 ID のコードが存在しない</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<CodeResponse>> GetCode(int id)
    {
        var code = await db.Codes.Include(c => c.CodeType).FirstOrDefaultAsync(c => c.Id == id);
        if (code is null) return NotFound();
        return Ok(ToResponse(code));
    }

    /// <summary>
    /// 新規コードを作成する。コード種別の存在チェックを行う。
    /// </summary>
    /// <param name="request">作成リクエスト（CreateCodeRequest）</param>
    /// <returns>ActionResult&lt;CodeResponse&gt; — 作成されたコード情報</returns>
    /// <response code="201">作成成功</response>
    /// <response code="400">コード種別が存在しない、またはバリデーションエラー</response>
    /// <response code="403">admin または editor ロール以外</response>
    [HttpPost]
    [Authorize(Roles = "admin,editor")]
    public async Task<ActionResult<CodeResponse>> CreateCode(CreateCodeRequest request)
    {
        // コード種別の存在チェック
        var codeType = await db.CodeTypes.FindAsync(request.CodeTypeId);
        if (codeType is null)
            return BadRequest(new { message = "指定されたコード種別が存在しません。" });

        var now = DateTime.UtcNow;
        var code = new Code
        {
            CodeTypeId = request.CodeTypeId,
            Value = request.Value,
            Name = request.Name,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Codes.Add(code);
        await db.SaveChangesAsync();

        // 操作ログ記録
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "Create",
            TableName = "Codes",
            RecordId = code.Id.ToString(),
            Changes = JsonSerializer.Serialize(new
            {
                code.CodeTypeId, code.Value, code.Name, code.DisplayOrder, code.IsActive,
            }),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        // ナビゲーションプロパティをセット
        code.CodeType = codeType;

        return CreatedAtAction(nameof(GetCode), new { id = code.Id }, ToResponse(code));
    }

    /// <summary>
    /// 指定 ID のコードを更新する。コード種別変更時は存在チェックを行う。
    /// </summary>
    /// <param name="id">コード ID（int）</param>
    /// <param name="request">更新リクエスト（UpdateCodeRequest）</param>
    /// <returns>ActionResult&lt;CodeResponse&gt; — 更新後のコード情報</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">コード種別が存在しない</response>
    /// <response code="403">admin または editor ロール以外</response>
    /// <response code="404">指定 ID のコードが存在しない</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,editor")]
    public async Task<ActionResult<CodeResponse>> UpdateCode(int id, UpdateCodeRequest request)
    {
        var code = await db.Codes.Include(c => c.CodeType).FirstOrDefaultAsync(c => c.Id == id);
        if (code is null) return NotFound();

        // コード種別の存在チェック
        if (request.CodeTypeId != code.CodeTypeId)
        {
            var codeType = await db.CodeTypes.FindAsync(request.CodeTypeId);
            if (codeType is null)
                return BadRequest(new { message = "指定されたコード種別が存在しません。" });
            code.CodeType = codeType;
        }

        // 変更前の値を保持
        var oldCodeTypeId = code.CodeTypeId;
        var oldValue = code.Value;
        var oldName = code.Name;
        var oldDisplayOrder = code.DisplayOrder;
        var oldIsActive = code.IsActive;

        code.CodeTypeId = request.CodeTypeId;
        code.Value = request.Value;
        code.Name = request.Name;
        code.DisplayOrder = request.DisplayOrder;
        code.IsActive = request.IsActive;
        code.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // 操作ログ記録（差分）
        var changes = new Dictionary<string, string>();
        if (oldCodeTypeId != request.CodeTypeId) changes["CodeTypeId"] = $"{oldCodeTypeId}\u2192{request.CodeTypeId}";
        if (oldValue != request.Value) changes["Value"] = $"{oldValue}\u2192{request.Value}";
        if (oldName != request.Name) changes["Name"] = $"{oldName}\u2192{request.Name}";
        if (oldDisplayOrder != request.DisplayOrder) changes["DisplayOrder"] = $"{oldDisplayOrder}\u2192{request.DisplayOrder}";
        if (oldIsActive != request.IsActive) changes["IsActive"] = $"{oldIsActive}\u2192{request.IsActive}";

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "Update",
            TableName = "Codes",
            RecordId = id.ToString(),
            Changes = JsonSerializer.Serialize(changes),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return Ok(ToResponse(code));
    }

    /// <summary>
    /// 指定 ID のコードを削除する。
    /// </summary>
    /// <param name="id">コード ID（int）</param>
    /// <returns>IActionResult — 204 No Content</returns>
    /// <response code="204">削除成功</response>
    /// <response code="403">admin または editor ロール以外</response>
    /// <response code="404">指定 ID のコードが存在しない</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,editor")]
    public async Task<IActionResult> DeleteCode(int id)
    {
        var code = await db.Codes.FindAsync(id);
        if (code is null) return NotFound();

        // 削除データを保持
        var deletedData = JsonSerializer.Serialize(new
        {
            code.CodeTypeId, code.Value, code.Name, code.DisplayOrder, code.IsActive,
        });

        db.Codes.Remove(code);
        await db.SaveChangesAsync();

        // 操作ログ記録
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "Delete",
            TableName = "Codes",
            RecordId = id.ToString(),
            Changes = deletedData,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Code エンティティを CodeResponse DTO に変換する。
    /// </summary>
    /// <param name="c">変換元エンティティ（Code, CodeType を Include 済み）</param>
    /// <returns>CodeResponse — レスポンス DTO</returns>
    private static CodeResponse ToResponse(Code c) => new()
    {
        Id = c.Id,
        CodeTypeId = c.CodeTypeId,
        CodeTypeName = c.CodeType.Name,
        CodeTypeColor = c.CodeType.Color,
        Value = c.Value,
        Name = c.Name,
        DisplayOrder = c.DisplayOrder,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };
}
