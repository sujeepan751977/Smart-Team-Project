using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Auth;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using System.Security.Claims;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            IAuthService authService,
            IJwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register/job-seeker")]
        public async Task<IActionResult> RegisterJobSeeker(RegisterJobSeekerDto request)
        {
            var result = await _authService.RegisterJobSeekerAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register/employer")]
        public async Task<IActionResult> RegisterEmployer(RegisterEmployerDto request)
        {
            var result = await _authService.RegisterEmployerAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst(ClaimTypes.Name);

            if (userIdClaim == null)
                return Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _authService.GetCurrentUserAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}
