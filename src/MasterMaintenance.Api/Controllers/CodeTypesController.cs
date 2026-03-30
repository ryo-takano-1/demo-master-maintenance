using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

[ApiController]
[Route("api/code-types")]
public class CodeTypesController(AppDbContext db) : ControllerBase
{
    /// <summary>全件取得</summary>
    [HttpGet]
    public async Task<ActionResult<List<CodeTypeResponse>>> GetCodeTypes()
    {
        var items = await db.CodeTypes
            .OrderBy(ct => ct.Id)
            .Select(ct => ToResponse(ct))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>1件取得</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CodeTypeResponse>> GetCodeType(int id)
    {
        var codeType = await db.CodeTypes.FindAsync(id);
        if (codeType is null) return NotFound();
        return Ok(ToResponse(codeType));
    }

    /// <summary>新規作成</summary>
    [HttpPost]
    public async Task<ActionResult<CodeTypeResponse>> CreateCodeType(CreateCodeTypeRequest request)
    {
        if (await db.CodeTypes.AnyAsync(ct => ct.Key == request.Key))
            return Conflict(new { message = "指定されたキーは既に使用されています。" });

        var now = DateTime.UtcNow;
        var codeType = new CodeType
        {
            Key = request.Key,
            Name = request.Name,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.CodeTypes.Add(codeType);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCodeType), new { id = codeType.Id }, ToResponse(codeType));
    }

    /// <summary>更新</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CodeTypeResponse>> UpdateCodeType(int id, UpdateCodeTypeRequest request)
    {
        var codeType = await db.CodeTypes.FindAsync(id);
        if (codeType is null) return NotFound();

        // キーの重複チェック（自分自身は除く）
        if (await db.CodeTypes.AnyAsync(ct => ct.Key == request.Key && ct.Id != id))
            return Conflict(new { message = "指定されたキーは既に使用されています。" });

        codeType.Key = request.Key;
        codeType.Name = request.Name;
        codeType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(ToResponse(codeType));
    }

    /// <summary>削除（使用中のコード種別は削除不可）</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCodeType(int id)
    {
        var codeType = await db.CodeTypes.FindAsync(id);
        if (codeType is null) return NotFound();

        // 使用中チェック
        var hasCode = await db.Codes.AnyAsync(c => c.CodeTypeId == id);
        if (hasCode)
            return Conflict(new { message = "このコード種別は使用中のため削除できません。" });

        db.CodeTypes.Remove(codeType);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static CodeTypeResponse ToResponse(CodeType ct) => new()
    {
        Id = ct.Id,
        Key = ct.Key,
        Name = ct.Name,
        CreatedAt = ct.CreatedAt,
        UpdatedAt = ct.UpdatedAt,
    };
}
