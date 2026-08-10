using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationAuditValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "ModerationAuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousValue",
                table: "ModerationAuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "ModerationAuditLogs");

            migrationBuilder.DropColumn(
                name: "PreviousValue",
                table: "ModerationAuditLogs");
        }
    }
}
