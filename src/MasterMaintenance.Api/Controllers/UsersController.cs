using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

/// <summary>
/// ユーザー情報の CRUD 操作を提供する API コントローラー。
/// </summary>
/// <remarks>
/// <para>認証: JWT Bearer 必須（全エンドポイント）</para>
/// <para>認可: 閲覧は全ロール、作成・削除は admin のみ、更新は admin + editor</para>
/// <para>対応画面: index.html（ユーザーマスタ）</para>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// ユーザー一覧を検索条件付きで取得する。
    /// </summary>
    /// <param name="id">ユーザー ID（string?, 部分一致、省略可）</param>
    /// <param name="userName">ユーザー名（string?, 部分一致、省略可）</param>
    /// <param name="role">ロール（string?, 完全一致、省略可）</param>
    /// <param name="page">ページ番号（int, 1始まり、既定: 1）</param>
    /// <param name="pageSize">1ページあたりの件数（int, 既定: 10）</param>
    /// <returns>ActionResult&lt;PagedResponse&lt;UserResponse&gt;&gt; — ページネーション付きユーザー一覧</returns>
    /// <response code="200">検索結果を返す</response>
    /// <response code="401">未認証</response>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetUsers(
        [FromQuery] string? id,
        [FromQuery] string? userName,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(id))
            query = query.Where(u => u.Id.Contains(id));

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(u => u.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => ToResponse(u))
            .ToListAsync();

        return Ok(new PagedResponse<UserResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    /// <summary>
    /// 指定 ID のユーザーを1件取得する。
    /// </summary>
    /// <param name="id">ユーザー ID（string）</param>
    /// <returns>ActionResult&lt;UserResponse&gt; — ユーザー情報</returns>
    /// <response code="200">ユーザーを返す</response>
    /// <response code="404">指定 ID のユーザーが存在しない</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(ToResponse(user));
    }

    /// <summary>
    /// 新規ユーザーを作成する。
    /// </summary>
    /// <param name="request">作成リクエスト（CreateUserRequest）</param>
    /// <returns>ActionResult&lt;UserResponse&gt; — 作成されたユーザー情報</returns>
    /// <response code="201">作成成功</response>
    /// <response code="400">バリデーションエラー</response>
    /// <response code="403">admin ロール以外</response>
    /// <response code="409">ID が既に使用されている</response>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
    {
        if (await db.Users.AnyAsync(u => u.Id == request.Id))
            return Conflict(new { message = "指定された ID は既に使用されています。" });

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = request.Id,
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // 操作ログ記録
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "Create",
            TableName = "Users",
            RecordId = user.Id,
            Changes = JsonSerializer.Serialize(new
            {
                user.Id, user.UserName, user.Email, user.Role, user.IsActive,
            }),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToResponse(user));
    }

    /// <summary>
    /// 指定 ID のユーザーを更新する。admin 以外はロール変更が無視される。
    /// </summary>
    /// <param name="id">ユーザー ID（string）</param>
    /// <param name="request">更新リクエスト（UpdateUserRequest）</param>
    /// <returns>ActionResult&lt;UserResponse&gt; — 更新後のユーザー情報</returns>
    /// <response code="200">更新成功</response>
    /// <response code="403">admin または editor ロール以外</response>
    /// <response code="404">指定 ID のユーザーが存在しない</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,editor")]
    public async Task<ActionResult<UserResponse>> UpdateUser(string id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        // 変更前の値を保持
        var oldUserName = user.UserName;
        var oldEmail = user.Email;
        var oldRole = user.Role;
        var oldIsActive = user.IsActive;

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.IsActive = request.IsActive;

        // admin 以外はロール変更を無視（権限昇格防止）
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole == "admin")
            user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await db.SaveChangesAsync();

        // 操作ログ記録（差分）
        var changes = new Dictionary<string, string>();
        if (oldUserName != request.UserName) changes["UserName"] = $"{oldUserName}\u2192{request.UserName}";
        if (oldEmail != request.Email) changes["Email"] = $"{oldEmail}\u2192{request.Email}";
        if (oldRole != user.Role) changes["Role"] = $"{oldRole}\u2192{user.Role}";
        if (oldIsActive != request.IsActive) changes["IsActive"] = $"{oldIsActive}\u2192{request.IsActive}";
        if (!string.IsNullOrWhiteSpace(request.Password)) changes["Password"] = "changed";

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "Update",
            TableName = "Users",
            RecordId = id,
            Changes = JsonSerializer.Serialize(changes),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return Ok(ToResponse(user));
    }

    /// <summary>
    /// 指定 ID のユーザーを削除する。
    /// </summary>
    /// <param name="id">ユーザー ID（string）</param>
    /// <returns>IActionResult — 204 No Content</returns>
    /// <response code="204">削除成功</response>
    /// <response code="403">admin ロール以外</response>
    /// <response code="404">指定 ID のユーザーが存在しない</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        // 削除データを保持
        var deletedData = JsonSerializer.Serialize(new
        {
            user.Id, user.UserName, user.Email, user.Role, user.IsActive,
        });

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        // 操作ログ記録
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "Delete",
            TableName = "Users",
            RecordId = id,
            Changes = deletedData,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// User エンティティを UserResponse DTO に変換する。
    /// </summary>
    /// <param name="u">変換元エンティティ（User）</param>
    /// <returns>UserResponse — レスポンス DTO</returns>
    private static UserResponse ToResponse(User u) => new()
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        Role = u.Role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt,
    };
}
