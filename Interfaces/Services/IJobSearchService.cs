using Recruitment_Project.DTOs.Jobs;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IJobSearchService
    {
        Task<List<VacancyListDto>> SearchJobsAsync(
            JobSearchRequestDto request);


        Task<JobSeekerVacancyDetailsDto?> GetJobDetailsAsync(
            int jobId,
            int? userId);
    }
}