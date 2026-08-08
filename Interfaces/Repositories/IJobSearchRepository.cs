using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IJobSearchRepository
    {
        Task<List<Vacancy>> SearchJobsAsync(
            string? search,
            string? company,
            string? skill,
            string? location,
            int? experience,
            int pageNumber,
            int pageSize);

        Task<Vacancy?> GetJobDetailsAsync(int id);
    }
}
