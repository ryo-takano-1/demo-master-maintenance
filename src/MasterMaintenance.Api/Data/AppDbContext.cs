using Microsoft.EntityFrameworkCore;
using MasterMaintenance.Api.Models;

namespace MasterMaintenance.Api.Data;

/// <summary>
/// アプリケーションの EF Core DbContext。
/// </summary>
/// <remarks>
/// <para>データベース: SQLite（既定: app.db）</para>
/// <para>OnModelCreating でエンティティ設定とシードデータ投入を行う。</para>
/// </remarks>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>ユーザーマスタ（DbSet&lt;User&gt;）</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>コード種別マスタ（DbSet&lt;CodeType&gt;）</summary>
    public DbSet<CodeType> CodeTypes => Set<CodeType>();

    /// <summary>コードマスタ（DbSet&lt;Code&gt;）</summary>
    public DbSet<Code> Codes => Set<Code>();

    /// <summary>操作ログ（DbSet&lt;AuditLog&gt;）</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <summary>
    /// エンティティの設定とシードデータの投入を行う。
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー（ModelBuilder）</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----- User -----
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(10);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ----- CodeType -----
        modelBuilder.Entity<CodeType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // ----- Code -----
        modelBuilder.Entity<Code>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Value).IsUnique();

            entity.HasOne(e => e.CodeType)
                  .WithMany(ct => ct.Codes)
                  .HasForeignKey(e => e.CodeTypeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ----- AuditLog -----
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TableName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RecordId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ----- Seed Data -----
        SeedData(modelBuilder);
    }

    /// <summary>
    /// 初期データ（ユーザー、コード種別、コード）を投入する。
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー（ModelBuilder）</param>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // "Password123!" を BCrypt ハッシュ化した固定値（ソルト固定でマイグレーション差分を安定させる）
        const string passwordHash = "$2a$11$FyEMqRnK/ctDdYDIscFvXeNF4QrUgpv2zq.ligrcYqaAITQepGsK.";

        // Users
        modelBuilder.Entity<User>().HasData(
            new User { Id = "U001", UserName = "佐藤 太郎", Email = "sato.taro@example.com", PasswordHash = passwordHash, Role = "admin", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { Id = "U002", UserName = "田中 花子", Email = "tanaka.hanako@example.com", PasswordHash = passwordHash, Role = "editor", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { Id = "U003", UserName = "鈴木 一郎", Email = "suzuki.ichiro@example.com", PasswordHash = passwordHash, Role = "viewer", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { Id = "U004", UserName = "高橋 美咲", Email = "takahashi.misaki@example.com", PasswordHash = passwordHash, Role = "editor", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { Id = "U005", UserName = "渡辺 健二", Email = "watanabe.kenji@example.com", PasswordHash = passwordHash, Role = "viewer", IsActive = false, CreatedAt = now, UpdatedAt = now }
        );

        // CodeTypes
        modelBuilder.Entity<CodeType>().HasData(
            new CodeType { Id = 1, Key = "DEPT", Name = "部門", Color = "primary", CreatedAt = now, UpdatedAt = now },
            new CodeType { Id = 2, Key = "ROLE", Name = "役職", Color = "success", CreatedAt = now, UpdatedAt = now },
            new CodeType { Id = 3, Key = "STATUS", Name = "ステータス", Color = "warning", CreatedAt = now, UpdatedAt = now },
            new CodeType { Id = 4, Key = "REGION", Name = "地域", Color = "info", CreatedAt = now, UpdatedAt = now }
        );

        // Codes（codes.html のサンプルデータに合わせる）
        modelBuilder.Entity<Code>().HasData(
            new Code { Id = 1, CodeTypeId = 1, Value = "DEPT_01", Name = "営業部", DisplayOrder = 1, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Code { Id = 2, CodeTypeId = 1, Value = "DEPT_02", Name = "開発部", DisplayOrder = 2, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Code { Id = 3, CodeTypeId = 1, Value = "DEPT_03", Name = "総務部", DisplayOrder = 3, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Code { Id = 4, CodeTypeId = 2, Value = "ROLE_01", Name = "部長", DisplayOrder = 1, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Code { Id = 5, CodeTypeId = 2, Value = "ROLE_02", Name = "課長", DisplayOrder = 2, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Code { Id = 6, CodeTypeId = 2, Value = "ROLE_03", Name = "一般社員", DisplayOrder = 3, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Code { Id = 7, CodeTypeId = 3, Value = "STATUS_01", Name = "申請中", DisplayOrder = 1, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Code { Id = 8, CodeTypeId = 3, Value = "STATUS_02", Name = "承認済", DisplayOrder = 2, IsActive = false, CreatedAt = now, UpdatedAt = now }
        );
    }
}
