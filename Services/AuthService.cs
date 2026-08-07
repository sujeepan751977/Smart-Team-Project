using Recruitment_Project.DTOs.Auth;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Recruitment_Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJobSeekerRepository _jobSeekerRepository;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IJobSeekerRepository jobSeekerRepository)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _jobSeekerRepository = jobSeekerRepository;
        }

        public async Task<AuthResponseDto> RegisterJobSeekerAsync(RegisterJobSeekerDto request)
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.JobSeeker,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var profile = new JobSeekerProfile
            {
                UserId = user.Id,
                ProfessionalTitle = string.Empty,
                Location = string.Empty,
                ExperienceInYears = 0,
                Education = string.Empty,
                About = string.Empty,
                ProfileCompletionPercentage = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _jobSeekerRepository.AddProfileAsync(profile);
            await _jobSeekerRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Job Seeker registered successfully.",
                UserId = user.Id
            };
        }

        public async Task<AuthResponseDto> RegisterEmployerAsync(RegisterEmployerDto request)
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Employer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Employer registered successfully.",
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

            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Your account has been disabled. Please contact the administrator."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto request, int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
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

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
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
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }



        Task<UserDto?> IAuthService.GetCurrentUserAsync(int userId)
        {
            return GetCurrentUserAsync(userId);
        }


    }
}
