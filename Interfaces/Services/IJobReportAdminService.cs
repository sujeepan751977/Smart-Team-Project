using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IJobReportAdminService
    {
        Task<IEnumerable<JobReport>> GetAllReportsAsync();

        Task<IEnumerable<JobReport>> GetReportsByStatusAsync(
            JobReportStatus status);

        Task<JobReport?> GetReportDetailsAsync(
            int reportId);

        Task StartReviewAsync(
            int reportId);

        Task ResolveReportAsync(
            int reportId);

        Task DismissReportAsync(
            int reportId);
    }
}