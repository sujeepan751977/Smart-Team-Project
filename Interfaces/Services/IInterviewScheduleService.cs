using Recruitment_Project.DTOs.InterviewSchedules;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IInterviewScheduleService
    {
        Task<InterviewScheduleDto> CreateAsync(
            int userId,
            int applicationId,
            CreateInterviewScheduleDto dto);


        Task<List<InterviewScheduleDto>> 
            GetEmployerInterviewsAsync(
            int userId);


        Task<List<InterviewScheduleDto>> 
            GetJobSeekerInterviewsAsync(
            int userId);
    }
}