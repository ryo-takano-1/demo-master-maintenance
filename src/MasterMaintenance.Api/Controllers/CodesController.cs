using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodesController(AppDbContext db) : ControllerBase
{
    /// <summary>一覧取得（検索 + ページネーション）</summary>
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

    /// <summary>1件取得</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CodeResponse>> GetCode(int id)
    {
        var code = await db.Codes.Include(c => c.CodeType).FirstOrDefaultAsync(c => c.Id == id);
        if (code is null) return NotFound();
        return Ok(ToResponse(code));
    }

    /// <summary>新規作成</summary>
    [HttpPost]
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

        // ナビゲーションプロパティをセット
        code.CodeType = codeType;

        return CreatedAtAction(nameof(GetCode), new { id = code.Id }, ToResponse(code));
    }

    /// <summary>更新</summary>
    [HttpPut("{id}")]
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

        code.CodeTypeId = request.CodeTypeId;
        code.Value = request.Value;
        code.Name = request.Name;
        code.DisplayOrder = request.DisplayOrder;
        code.IsActive = request.IsActive;
        code.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(ToResponse(code));
    }

    /// <summary>削除</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCode(int id)
    {
        var code = await db.Codes.FindAsync(id);
        if (code is null) return NotFound();

        db.Codes.Remove(code);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static CodeResponse ToResponse(Code c) => new()
    {
        Id = c.Id,
        CodeTypeId = c.CodeTypeId,
        CodeTypeName = c.CodeType.Name,
        Value = c.Value,
        Name = c.Name,
        DisplayOrder = c.DisplayOrder,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };
}
