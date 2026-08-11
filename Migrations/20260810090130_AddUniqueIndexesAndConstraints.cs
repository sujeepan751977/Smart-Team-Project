using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VacancySkills_VacancyId",
                table: "VacancySkills");

            migrationBuilder.DropIndex(
                name: "IX_JobSeekerSkills_JobSeekerProfileId",
                table: "JobSeekerSkills");

            migrationBuilder.DropIndex(
                name: "IX_JobReports_VacancyId",
                table: "JobReports");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_VacancyId",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_VacancySkills_VacancyId_SkillId",
                table: "VacancySkills",
                columns: new[] { "VacancyId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_JobSeekerProfileId_SkillId",
                table: "JobSeekerSkills",
                columns: new[] { "JobSeekerProfileId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobReports_VacancyId_ReportedByUserId",
                table: "JobReports",
                columns: new[] { "VacancyId", "ReportedByUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_VacancyId_JobSeekerProfileId",
                table: "JobApplications",
                columns: new[] { "VacancyId", "JobSeekerProfileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VacancySkills_VacancyId_SkillId",
                table: "VacancySkills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_Name",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_JobSeekerSkills_JobSeekerProfileId_SkillId",
                table: "JobSeekerSkills");

            migrationBuilder.DropIndex(
                name: "IX_JobReports_VacancyId_ReportedByUserId",
                table: "JobReports");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_VacancyId_JobSeekerProfileId",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_VacancySkills_VacancyId",
                table: "VacancySkills",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_JobSeekerProfileId",
                table: "JobSeekerSkills",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobReports_VacancyId",
                table: "JobReports",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_VacancyId",
                table: "JobApplications",
                column: "VacancyId");
        }
    }
}
