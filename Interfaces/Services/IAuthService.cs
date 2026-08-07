using Recruitment_Project.DTOs.Auth;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterJobSeekerAsync(RegisterJobSeekerDto request);

        Task<AuthResponseDto> RegisterEmployerAsync(RegisterEmployerDto request);

        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

        Task<UserDto?> GetCurrentUserAsync(int userId);

        Task<bool> ChangePasswordAsync(ChangePasswordDto request, int userId);

        Task<bool> ResetPasswordAsync(ResetPasswordDto request);
    }
}
