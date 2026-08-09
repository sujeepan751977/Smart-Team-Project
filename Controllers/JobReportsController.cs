using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Enums;
using Recruitment_Project.Services;
using System.Security.Claims;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/jobs/{vacancyId}/reports")]
    [Authorize]
    public class JobReportsController : ControllerBase
    {
        private readonly IJobReportService _jobReportService;

        public JobReportsController(
            IJobReportService jobReportService)
        {
            _jobReportService = jobReportService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateReport(
            int vacancyId,
            JobReportReason reason,
            string? description)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();


            var result = await _jobReportService
                .CreateReportAsync(
                    vacancyId,
                    userId.Value,
                    reason,
                    description);


            return Ok(result);
        }


        [HttpGet("~/api/jobseekers/me/job-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();


            var reports = await _jobReportService
                .GetMyReportsAsync(userId.Value);


            return Ok(reports);
        }


        [HttpGet("~/api/jobseekers/me/job-reports/{id}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();


            var report = await _jobReportService
                .GetReportByIdAsync(id, userId.Value);


            if (report == null)
                return NotFound();


            return Ok(report);
        }


        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(ClaimTypes.Name);


            if (claim == null)
                return null;


            if (!int.TryParse(claim.Value, out var userId))
                return null;


            return userId;
        }
    }
}