using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Employers;
using Recruitment_Project.Helpers;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [Route("api/employers")]
    [ApiController]
    [Authorize(Roles = RoleNames.Employer)]
    public class EmployersController : ControllerBase
    {
        private readonly IEmployerService _employerService;

        public EmployersController(IEmployerService employerService)
        {
            _employerService = employerService;
        }

        [HttpGet("me/profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.GetUserId();

            var profile = await _employerService.GetProfileAsync(userId);

            if (profile == null)
            {
                return NotFound("Employer profile not found");
            }

            return Ok(profile);
        }

        [HttpPost("me/profile")]
        public async Task<IActionResult> CreateMyProfile(
    [FromBody] UpdateEmployerProfileDto dto)
        {
            var userId = User.GetUserId();

            await _employerService.CreateProfileAsync(userId, dto);

            return Ok(new
            {
                Success = true,
                Message = "Employer profile created successfully."
            });
        }

        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateMyProfile(
    [FromBody] UpdateEmployerProfileDto dto)
        {
            var userId = User.GetUserId();

            await _employerService.UpdateProfileAsync(userId, dto);

            return Ok(new
            {
                Success = true,
                Message = "Employer profile updated successfully."
            });
        }

        [HttpGet("me/dashboard")]
        public async Task<IActionResult> GetMyDashboard()
        {
            var userId = User.GetUserId();

            var dashboard = await _employerService.GetDashboardAsync(userId);

            return Ok(dashboard);
        }
    }
}
