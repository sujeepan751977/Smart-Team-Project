using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IJobReportService
    {
        Task<JobReport> CreateReportAsync(
            int vacancyId,
            int userId,
            JobReportReason reason,
            string? description);

        Task<IEnumerable<JobReport>> GetMyReportsAsync(
            int userId);

        Task<JobReport?> GetReportByIdAsync(
            int id,
            int userId);
    }
}