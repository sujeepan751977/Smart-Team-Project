using Recruitment_Project.DTOs.Admin;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IAdminUserService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<List<AdminUserDto>> GetAllUsersAsync();
        Task<AdminUserDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserStatusAsync(int id, bool isActive);
    }
}
