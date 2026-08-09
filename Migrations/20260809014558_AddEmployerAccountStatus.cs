using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerAccountStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountStatus",
                table: "EmployerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "EmployerProfiles");
        }
    }
}
