using Recruitment_Project.DTOs.Auth;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Recruitment_Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists."
                };
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.Trim().ToLower(),
                PasswordHash = HashPassword(request.Password),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful.",
                UserId = user.Id
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLower());

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (user.PasswordHash != HashPassword(request.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                UserId = user.Id,
                Role = user.Role.ToString()
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto request, int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            if (user.PasswordHash != HashPassword(request.CurrentPassword))
                return false;

            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLower());

            if (user == null)
                return false;

            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();

            var bytes = Encoding.UTF8.GetBytes(password);

            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        Task<UserDto?> IAuthService.GetCurrentUserAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
