using Recruitment_Project.DTOs.Admin;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmployerRepository _employerRepository;

        public AdminUserService(
            IUserRepository userRepository,
            IEmployerRepository employerRepository)
        {
            _userRepository = userRepository;
            _employerRepository = employerRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            return new AdminDashboardDto
            {
                TotalUsers = await _userRepository.GetTotalUsersAsync(),
                TotalJobSeekers = await _userRepository.GetTotalJobSeekersAsync(),
                TotalEmployers = await _userRepository.GetTotalEmployersAsync(),
                ActiveUsers = await _userRepository.GetActiveUsersAsync(),
                DisabledUsers = await _userRepository.GetDisabledUsersAsync()
            };
        }

        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return users.Select(user => new AdminUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }).ToList();
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return null;

            return new AdminUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateUserStatusAsync(int id, bool isActive)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            if (user.Role == UserRole.Employer)
            {
                var employer =
                    await _employerRepository.GetByUserIdAsync(user.Id);

                if (employer != null)
                {
                    employer.AccountStatus = isActive
                        ? EmployerAccountStatus.Active
                        : EmployerAccountStatus.Disabled;

                    employer.UpdatedAt = DateTime.UtcNow;

                    await _employerRepository.UpdateAsync(employer);
                }
            }

            await _userRepository.SaveChangesAsync();
            await _employerRepository.SaveChangesAsync();

            return true;
        }
    }
}
