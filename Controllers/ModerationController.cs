using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Moderation;
using Recruitment_Project.Interfaces.Services;
using System.Security.Claims;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Administrator")]
    public class ModerationController : ControllerBase
    {
        private readonly IModerationService _moderationService;

        public ModerationController(
            IModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        [HttpPatch("employers/{employerId}/warn")]
        public async Task<IActionResult> WarnEmployer(
            int employerId,
            ModerationDecisionRequestDto request)
        {
            var adminUserId = GetCurrentUserId();

            if (adminUserId == null)
                return Unauthorized();

            await _moderationService.WarnEmployerAsync(
                adminUserId.Value,
                employerId,
                request.DecisionNote);

            return Ok(new
            {
                Message = "Employer warned successfully."
            });
        }

        [HttpPatch("employers/{employerId}/suspend")]
        public async Task<IActionResult> SuspendEmployer(
            int employerId,
            ModerationDecisionRequestDto request)
        {
            var adminUserId = GetCurrentUserId();

            if (adminUserId == null)
                return Unauthorized();

            await _moderationService.SuspendEmployerAsync(
                adminUserId.Value,
                employerId,
                request.DecisionNote);

            return Ok(new
            {
                Message = "Employer suspended successfully."
            });
        }

        [HttpPatch("employers/{employerId}/disable")]
        public async Task<IActionResult> DisableEmployer(
            int employerId,
            ModerationDecisionRequestDto request)
        {
            var adminUserId = GetCurrentUserId();

            if (adminUserId == null)
                return Unauthorized();

            await _moderationService.DisableEmployerAsync(
                adminUserId.Value,
                employerId,
                request.DecisionNote);

            return Ok(new
            {
                Message = "Employer disabled successfully."
            });
        }

        [HttpPatch("vacancies/{vacancyId}/close")]
        public async Task<IActionResult> CloseVacancy(
            int vacancyId,
            ModerationDecisionRequestDto request)
        {
            var adminUserId = GetCurrentUserId();

            if (adminUserId == null)
                return Unauthorized();

            await _moderationService.CloseVacancyAsync(
                adminUserId.Value,
                vacancyId,
                request.DecisionNote);

            return Ok(new
            {
                Message = "Vacancy closed successfully."
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst(ClaimTypes.Name);

            if (userIdClaim == null)
                return null;

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return null;

            return userId;
        }
    }
}