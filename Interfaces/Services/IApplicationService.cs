using Recruitment_Project.DTOs.Applications;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IApplicationService
    {
        Task<ApplicationDto> ApplyAsync(
         int userId,
         int vacancyId,
         ApplyJobDto dto);

        Task<List<ApplicationDto>> GetMyApplicationsAsync(
            int userId);

        Task<List<ApplicationDto>> GetEmployerApplicationsAsync(
            int userId);

        Task<ApplicationDto?> GetApplicationByIdAsync(
            int userId,
            int applicationId);

        Task UpdateStatusAsync(
            int userId,
            int applicationId,
            string status);

        Task<List<ApplicationDto>> GetApplicantsByVacancyAsync(
            int userId,
            int vacancyId);
    }
}
