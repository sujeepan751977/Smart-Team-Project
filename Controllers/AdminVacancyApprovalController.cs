using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Common;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/admin/vacancies")]
    [Authorize(Roles = "Administrator")]
    public class AdminVacancyApprovalController : ControllerBase
    {
        private readonly IVacancyApprovalService _vacancyApprovalService;

        public AdminVacancyApprovalController(
            IVacancyApprovalService vacancyApprovalService)
        {
            _vacancyApprovalService = vacancyApprovalService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingVacancies()
        {
            var vacancies =
                await _vacancyApprovalService
                    .GetPendingVacanciesAsync();

            return Ok(vacancies);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveVacancy(int id)
        {
            await _vacancyApprovalService
                .ApproveVacancyAsync(id);

            return Ok(new
            {
                Success = true,
                Message = "Vacancy approved successfully."
            });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectVacancy(
            int id,
            [FromBody] ReasonRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new
                {
                    Message = "Rejection reason is required."
                });
            }

            await _vacancyApprovalService
                .RejectVacancyAsync(id, request.Reason);

            return Ok(new
            {
                Success = true,
                Message = "Vacancy rejected successfully."
            });
        }
    }
}
