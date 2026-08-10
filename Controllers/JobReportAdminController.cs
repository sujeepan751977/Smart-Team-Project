using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/admin/job-reports")]
    [Authorize(Roles = "Administrator")]
    public class JobReportAdminController : ControllerBase
    {
        private readonly IJobReportAdminService _jobReportAdminService;

        public JobReportAdminController(
            IJobReportAdminService jobReportAdminService)
        {
            _jobReportAdminService = jobReportAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _jobReportAdminService
                .GetAllReportsAsync();

            return Ok(reports);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetReportsByStatus(
            JobReportStatus status)
        {
            var reports = await _jobReportAdminService
                .GetReportsByStatusAsync(status);

            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportDetails(int id)
        {
            var report = await _jobReportAdminService
                .GetReportDetailsAsync(id);

            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpPatch("{id}/start-review")]
        public async Task<IActionResult> StartReview(int id)
        {
            await _jobReportAdminService
                .StartReviewAsync(id);

            return Ok();
        }

        [HttpPatch("{id}/resolve")]
        public async Task<IActionResult> ResolveReport(int id)
        {
            await _jobReportAdminService
                .ResolveReportAsync(id);

            return Ok();
        }

        [HttpPatch("{id}/dismiss")]
        public async Task<IActionResult> DismissReport(int id)
        {
            await _jobReportAdminService
                .DismissReportAsync(id);

            return Ok();
        }
    }
}