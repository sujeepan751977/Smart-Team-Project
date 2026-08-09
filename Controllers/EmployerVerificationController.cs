using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Employers;
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

        [HttpPost("document/upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(
    [FromForm] UploadEmployerVerificationDocumentDto request)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _verificationService.AddDocumentFileAsync(
                userId,
                request);

            return Ok(new
            {
                Success = true,
                Message = "Verification document uploaded successfully."
            });
        }

        [HttpPut("company-information")]
        public async Task<IActionResult> UpdateCompanyInformation(
    [FromBody] UpdateEmployerProfileDto request)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _verificationService.UpdateCompanyInformationAsync(
                userId,
                request);

            return Ok(new
            {
                Success = true,
                Message = "Company information updated successfully."
            });
        }

        [HttpPatch("withdraw")]
        public async Task<IActionResult> WithdrawVerification()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _verificationService.WithdrawVerificationAsync(userId);

            return Ok(new
            {
                Success = true,
                Message = "Employer verification withdrawn successfully."
            });
        }

        [HttpPost("resubmit")]
        public async Task<IActionResult> ResubmitVerification()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _verificationService.ResubmitVerificationAsync(userId);

            return Ok(new
            {
                Success = true,
                Message = "Employer verification resubmitted successfully."
            });
        }

        [HttpDelete("document/{documentId}")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _verificationService.DeleteDocumentAsync(
                userId,
                documentId);

            return Ok(new
            {
                Success = true,
                Message = "Verification document deleted successfully."
            });
        }


    }
}
