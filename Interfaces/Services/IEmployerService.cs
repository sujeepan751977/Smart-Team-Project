using Recruitment_Project.DTOs.Employers;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IEmployerService
    {
        Task<EmployerProfileDto?> GetProfileAsync(int userId);

        Task CreateProfileAsync(int userId, UpdateEmployerProfileDto dto);

        Task UpdateProfileAsync(int userId, UpdateEmployerProfileDto dto);
    }
}
    

