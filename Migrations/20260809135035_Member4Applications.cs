using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment_Project.Migrations
{
    /// <inheritdoc />
    public partial class Member4Applications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "InterviewSchedules",
                newName: "Instructions");

            migrationBuilder.AddColumn<decimal>(
                name: "MatchScore",
                table: "JobApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "InterviewSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "InterviewSchedules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "InterviewSchedules");

            migrationBuilder.RenameColumn(
                name: "Instructions",
                table: "InterviewSchedules",
                newName: "Notes");
        }
    }
}
