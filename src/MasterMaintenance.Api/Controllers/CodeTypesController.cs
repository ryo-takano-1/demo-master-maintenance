using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

/// <summary>
/// コード種別の CRUD 操作を提供する API コントローラー。
/// </summary>
/// <remarks>
/// <para>認証: JWT Bearer 必須（全エンドポイント）</para>
/// <para>認可: 閲覧は全ロール、作成・更新・削除は admin + editor</para>
/// <para>対応画面: codes.html（コードマスタ内のコード種別管理モーダル）</para>
/// </remarks>
[ApiController]
[Route("api/code-types")]
[Authorize]
public class CodeTypesController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// コード種別を全件取得する（ページネーションなし）。
    /// </summary>
    /// <returns>ActionResult&lt;List&lt;CodeTypeResponse&gt;&gt; — コード種別一覧</returns>
    /// <response code="200">全件を返す</response>
    /// <response code="401">未認証</response>
    [HttpGet]
    public async Task<ActionResult<List<CodeTypeResponse>>> GetCodeTypes()
    {
        var items = await db.CodeTypes
            .OrderBy(ct => ct.Id)
            .Select(ct => ToResponse(ct))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// 指定 ID のコード種別を1件取得する。
    /// </summary>
    /// <param name="id">コード種別 ID（int）</param>
    /// <returns>ActionResult&lt;CodeTypeResponse&gt; — コード種別情報</returns>
    /// <response code="200">コード種別を返す</response>
    /// <response code="404">指定 ID のコード種別が存在しない</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<CodeTypeResponse>> GetCodeType(int id)
    {
        var codeType = await db.CodeTypes.FindAsync(id);
        if (codeType is null) return NotFound();
        return Ok(ToResponse(codeType));
    }

    /// <summary>
    /// 新規コード種別を作成する。キーの重複チェックを行う。
    /// </summary>
    /// <param name="request">作成リクエスト（CreateCodeTypeRequest）</param>
    /// <returns>ActionResult&lt;CodeTypeResponse&gt; — 作成されたコード種別情報</returns>
    /// <response code="201">作成成功</response>
    /// <response code="400">バリデーションエラー</response>
    /// <response code="403">admin または editor ロール以外</response>
    /// <response code="409">キーが既に使用されている</response>
    [HttpPost]
    [Authorize(Roles = "admin,editor")]
    public async Task<ActionResult<CodeTypeResponse>> CreateCodeType(CreateCodeTypeRequest request)
    {
        if (await db.CodeTypes.AnyAsync(ct => ct.Key == request.Key))
            return Conflict(new { message = "指定されたキーは既に使用されています。" });

        var now = DateTime.UtcNow;
        var codeType = new CodeType
        {
            Key = request.Key,
            Name = request.Name,
            Color = request.Color,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.CodeTypes.Add(codeType);
        await db.SaveChangesAsync();

        // 操作ログ記録
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "Create",
            TableName = "CodeTypes",
            RecordId = codeType.Id.ToString(),
            Changes = JsonSerializer.Serialize(new { codeType.Key, codeType.Name }),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCodeType), new { id = codeType.Id }, ToResponse(codeType));
    }

    /// <summary>
    /// 指定 ID のコード種別を更新する。キーの重複チェック（自分自身を除く）を行う。
    /// </summary>
    /// <param name="id">コード種別 ID（int）</param>
    /// <param name="request">更新リクエスト（UpdateCodeTypeRequest）</param>
    /// <returns>ActionResult&lt;CodeTypeResponse&gt; — 更新後のコード種別情報</returns>
    /// <response code="200">更新成功</response>
    /// <response code="403">admin または editor ロール以外</response>
    /// <response code="404">指定 ID のコード種別が存在しない</response>
    /// <response code="409">キーが既に使用されている</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,editor")]
    public async Task<ActionResult<CodeTypeResponse>> UpdateCodeType(int id, UpdateCodeTypeRequest request)
    {
        var codeType = await db.CodeTypes.FindAsync(id);
        if (codeType is null) return NotFound();

        // キーの重複チェック（自分自身は除く）
        if (await db.CodeTypes.AnyAsync(ct => ct.Key == request.Key && ct.Id != id))
            return Conflict(new { message = "指定されたキーは既に使用されています。" });

        // 変更前の値を保持
        var oldKey = codeType.Key;
        var oldName = codeType.Name;
        var oldColor = codeType.Color;

        codeType.Key = request.Key;
        codeType.Name = request.Name;
        codeType.Color = request.Color;
        codeType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // 操作ログ記録（差分）
        var changes = new Dictionary<string, string>();
        if (oldKey != request.Key) changes["Key"] = $"{oldKey}\u2192{request.Key}";
        if (oldName != request.Name) changes["Name"] = $"{oldName}\u2192{request.Name}";
        if (oldColor != request.Color) changes["Color"] = $"{oldColor}\u2192{request.Color}";

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "Update",
            TableName = "CodeTypes",
            RecordId = id.ToString(),
            Changes = JsonSerializer.Serialize(changes),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return Ok(ToResponse(codeType));
    }

    /// <summary>
    /// 指定 ID のコード種別を削除する。使用中（配下のコードが存在する）場合は削除不可。
    /// </summary>
    /// <param name="id">コード種別 ID（int）</param>
    /// <returns>IActionResult — 204 No Content</returns>
    /// <response code="204">削除成功</response>
    /// <response code="403">admin または editor ロール以外</response>
    /// <response code="404">指定 ID のコード種別が存在しない</response>
    /// <response code="409">使用中のため削除不可</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,editor")]
    public async Task<IActionResult> DeleteCodeType(int id)
    {
        var codeType = await db.CodeTypes.FindAsync(id);
        if (codeType is null) return NotFound();

        // 使用中チェック
        var hasCode = await db.Codes.AnyAsync(c => c.CodeTypeId == id);
        if (hasCode)
            return Conflict(new { message = "このコード種別は使用中のため削除できません。" });

        // 削除データを保持
        var deletedData = JsonSerializer.Serialize(new { codeType.Key, codeType.Name });

        db.CodeTypes.Remove(codeType);
        await db.SaveChangesAsync();

        // 操作ログ記録
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "Delete",
            TableName = "CodeTypes",
            RecordId = id.ToString(),
            Changes = deletedData,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// CodeType エンティティを CodeTypeResponse DTO に変換する。
    /// </summary>
    /// <param name="ct">変換元エンティティ（CodeType）</param>
    /// <returns>CodeTypeResponse — レスポンス DTO</returns>
    private static CodeTypeResponse ToResponse(CodeType ct) => new()
    {
        Id = ct.Id,
        Key = ct.Key,
        Name = ct.Name,
        Color = ct.Color,
        CreatedAt = ct.CreatedAt,
        UpdatedAt = ct.UpdatedAt,
    };
}
