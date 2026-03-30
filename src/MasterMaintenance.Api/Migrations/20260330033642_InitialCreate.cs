using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MasterMaintenance.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RecordId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Changes = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Codes_CodeTypes_CodeTypeId",
                        column: x => x.CodeTypeId,
                        principalTable: "CodeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CodeTypes",
                columns: new[] { "Id", "CreatedAt", "Key", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DEPT", "部門", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ROLE", "役職", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "STATUS", "ステータス", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "REGION", "地域", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "PasswordHash", "Role", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { "U001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sato.taro@example.com", true, "$2a$11$FyEMqRnK/ctDdYDIscFvXeNF4QrUgpv2zq.ligrcYqaAITQepGsK.", "admin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "佐藤 太郎" },
                    { "U002", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tanaka.hanako@example.com", true, "$2a$11$FyEMqRnK/ctDdYDIscFvXeNF4QrUgpv2zq.ligrcYqaAITQepGsK.", "editor", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "田中 花子" },
                    { "U003", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "suzuki.ichiro@example.com", true, "$2a$11$FyEMqRnK/ctDdYDIscFvXeNF4QrUgpv2zq.ligrcYqaAITQepGsK.", "viewer", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "鈴木 一郎" },
                    { "U004", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "takahashi.misaki@example.com", true, "$2a$11$FyEMqRnK/ctDdYDIscFvXeNF4QrUgpv2zq.ligrcYqaAITQepGsK.", "editor", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "高橋 美咲" },
                    { "U005", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "watanabe.kenji@example.com", false, "$2a$11$FyEMqRnK/ctDdYDIscFvXeNF4QrUgpv2zq.ligrcYqaAITQepGsK.", "viewer", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "渡辺 健二" }
                });

            migrationBuilder.InsertData(
                table: "Codes",
                columns: new[] { "Id", "CodeTypeId", "CreatedAt", "DisplayOrder", "IsActive", "Name", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "営業部", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DEPT_01" },
                    { 2, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "開発部", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DEPT_02" },
                    { 3, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "総務部", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DEPT_03" },
                    { 4, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "部長", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ROLE_01" },
                    { 5, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "課長", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ROLE_02" },
                    { 6, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "一般社員", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ROLE_03" },
                    { 7, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "申請中", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "STATUS_01" },
                    { 8, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, false, "承認済", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "STATUS_02" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Codes_CodeTypeId",
                table: "Codes",
                column: "CodeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Codes_Value",
                table: "Codes",
                column: "Value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeTypes_Key",
                table: "CodeTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Codes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CodeTypes");
        }
    }
}
