using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddVacancyAdministratorRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdministratorRejectionReason",
                table: "Vacancies",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdministratorRejectionReason",
                table: "Vacancies");
        }
    }
}
