using Recruitment_Project.DTOs.JobSeekers;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IJobSeekerService
    {
        Task<JobSeekerProfileDto?> GetProfileAsync(int userId);

        Task<bool> UpdateProfileAsync(
            int userId,
            UpdateJobSeekerProfileDto dto);

        Task<JobSeekerDashboardDto?> GetDashboardAsync(int userId);
    }
}