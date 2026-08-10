using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(int id);

        Task<JobApplication?> GetByVacancyAndJobSeekerAsync(
            int vacancyId,
            int jobSeekerProfileId);

        Task<List<JobApplication>> GetByJobSeekerAsync(
            int jobSeekerProfileId);

        Task<List<JobApplication>> GetByEmployerAsync(
            int employerProfileId);

        Task<List<JobApplication>> GetByVacancyAsync(
            int vacancyId);

        Task AddAsync(JobApplication application);

        Task SaveChangesAsync();
    }
}