using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMaintenance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeTypeColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "CodeTypes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "CodeTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Color",
                value: "primary");

            migrationBuilder.UpdateData(
                table: "CodeTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Color",
                value: "success");

            migrationBuilder.UpdateData(
                table: "CodeTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Color",
                value: "warning");

            migrationBuilder.UpdateData(
                table: "CodeTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Color",
                value: "info");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "CodeTypes");
        }
    }
}
