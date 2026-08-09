using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/admin/employer-verifications")]
    [Authorize(Roles = "Administrator")]
    public class AdminEmployerVerificationController : ControllerBase
    {
        private readonly IEmployerVerificationAdminService _verificationService;

        public AdminEmployerVerificationController(
            IEmployerVerificationAdminService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var verifications =
                await _verificationService.GetAllAsync();

            return Ok(verifications);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var verification =
                await _verificationService.GetByIdAsync(id);

            if (verification == null)
            {
                return NotFound(new
                {
                    Message = "Employer verification not found."
                });
            }

            return Ok(verification);
        }

        [HttpPatch("{id}/request-information")]
        public async Task<IActionResult> RequestInformation(
            int id,
            [FromBody] string feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback))
            {
                return BadRequest(new
                {
                    Message = "Feedback is required."
                });
            }

            await _verificationService
                .RequestInformationAsync(id, feedback);

            return Ok(new
            {
                Success = true,
                Message = "Additional information requested successfully."
            });
        }

        [HttpPatch("{id}/verify")]
        public async Task<IActionResult> Verify(int id)
        {
            await _verificationService.VerifyAsync(id);

            return Ok(new
            {
                Success = true,
                Message = "Employer verification approved successfully."
            });
        }

        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] string feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback))
            {
                return BadRequest(new
                {
                    Message = "Rejection feedback is required."
                });
            }

            await _verificationService
                .RejectAsync(id, feedback);

            return Ok(new
            {
                Success = true,
                Message = "Employer verification rejected successfully."
            });
        }
    }
}