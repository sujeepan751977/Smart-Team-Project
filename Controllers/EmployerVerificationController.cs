using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.EmployerVerification;
using Recruitment_Project.Interfaces.Services;
using System.Security.Claims;

namespace Recruitment_Project.Controllers
{
    [Route("api/employer-verification")]
    [ApiController]
    [Authorize(Roles = "Employer")]
    public class EmployerVerificationController : ControllerBase
    {
        private readonly IEmployerVerificationService _verificationService;

        public EmployerVerificationController(
            IEmployerVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVerification()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var verification =
                await _verificationService.GetVerificationAsync(userId);

            if (verification == null)
            {
                return NotFound("Employer verification not found");
            }

            return Ok(verification);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitVerification()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _verificationService.SubmitVerificationAsync(userId);

            return Ok(new
            {
                Success = true,
                Message = "Employer verification submitted successfully."
            });
        }

        [HttpPost("document")]
        public async Task<IActionResult> AddDocument(
            [FromBody] EmployerVerificationDocumentDto document)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _verificationService.AddDocumentAsync(userId, document);

            return Ok(new
            {
                Success = true,
                Message = "Verification document added successfully."
            });
        }
    }
}
