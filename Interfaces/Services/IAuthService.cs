using Recruitment_Project.DTOs.Auth;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

        Task<bool> ChangePasswordAsync(ChangePasswordDto request, int userId);

        Task<bool> ResetPasswordAsync(ResetPasswordDto request);

        Task<UserDto?> GetCurrentUserAsync(int userId);
    }
}
