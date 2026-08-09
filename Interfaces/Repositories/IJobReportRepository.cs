using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IJobReportRepository
    {
        Task<IEnumerable<JobReport>> GetByUserIdAsync(int userId);

        Task<JobReport?> GetByIdAsync(int id);

        Task<bool> ExistsAsync(int vacancyId, int userId);

        Task AddAsync(JobReport jobReport);

        Task UpdateAsync(JobReport jobReport);

        Task SaveChangesAsync();

        Task<IEnumerable<JobReport>> GetAllAsync();

        Task<IEnumerable<JobReport>> GetByStatusAsync(JobReportStatus status);
    }
}