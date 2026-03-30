using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Data;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
{
    /// <summary>一覧取得（検索 + ページネーション）</summary>
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

    /// <summary>1件取得</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(ToResponse(user));
    }

    /// <summary>新規作成</summary>
    [HttpPost]
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

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToResponse(user));
    }

    /// <summary>更新</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(string id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await db.SaveChangesAsync();

        return Ok(ToResponse(user));
    }

    /// <summary>削除</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return NoContent();
    }

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
