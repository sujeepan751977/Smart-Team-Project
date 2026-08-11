using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Applications;
using Recruitment_Project.Helpers;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api")]

    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(
            IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [Authorize(Roles = RoleNames.JobSeeker)]
        [HttpPost("jobs/{jobId}/applications")]
        public async Task<IActionResult> Apply(
            int jobId,
            [FromBody] ApplyJobDto dto)
        {
            var userId = User.GetUserId();

            var result =
                await _applicationService.ApplyAsync(
                    userId,
                    jobId,
                    dto);

            return Ok(result);
        }

        [Authorize(Roles = RoleNames.JobSeeker)]
        [HttpGet("jobseekers/me/applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = User.GetUserId();

            var result =
                await _applicationService
                    .GetMyApplicationsAsync(userId);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("applications/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.GetUserId();

            var result =
                await _applicationService
                    .GetApplicationByIdAsync(userId, id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [Authorize(Roles = RoleNames.Employer)]
        [HttpGet("employer/vacancies/{vacancyId}/applications")]
        public async Task<IActionResult> GetApplicants(int vacancyId)
        {
            var userId = User.GetUserId();

            var result =
                await _applicationService
                    .GetApplicantsByVacancyAsync(
                        userId,
                        vacancyId);

            return Ok(result);
        }

        [Authorize(Roles = RoleNames.Employer)]
        [HttpGet("employer/applications/{id}/cv")]
        public async Task<IActionResult> DownloadApplicantCv(int id)
        {
            var userId = User.GetUserId();

            var (fileStream, fileName, contentType) =
                await _applicationService
                    .DownloadApplicantCvAsync(userId, id);

            return File(fileStream, contentType, fileName);
        }

        [Authorize(Roles = RoleNames.Employer)]
        [HttpPut("applications/{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateApplicationStatusDto request)
        {
            var userId = User.GetUserId();

            await _applicationService
                .UpdateStatusAsync(
                    userId,
                    id,
                    request.Status);

            return Ok(new
            {
                message = "Status updated successfully"
            });
        }
    }
}
