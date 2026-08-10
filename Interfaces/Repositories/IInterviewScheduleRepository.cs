using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IInterviewScheduleRepository
    {
        Task AddAsync(InterviewSchedule interview);

        Task<List<InterviewSchedule>> GetByEmployerAsync(
            int employerProfileId);

        Task<List<InterviewSchedule>> GetByJobSeekerAsync(
            int jobSeekerProfileId);

        Task SaveChangesAsync();
    }
}