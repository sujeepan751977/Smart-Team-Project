using Recruitment_Project.DTOs.ContactRequests;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IContactRequestService
    {
        Task<ContactRequestDto> CreateAsync(
            int userId,
            int applicationId,
            CreateContactRequestDto dto);


        Task<List<ContactRequestDto>> GetEmployerRequestsAsync(
            int userId);


        Task<List<ContactRequestDto>> GetJobSeekerRequestsAsync(
            int userId);


        Task RespondAsync(
            int userId,
            int requestId,
            string response);
    }
}