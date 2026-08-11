using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Jobs;
using Recruitment_Project.Interfaces.Services;
using System.Security.Claims;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly IJobSearchService _jobSearchService;

        public JobsController(
            IJobSearchService jobSearchService)
        {
            _jobSearchService = jobSearchService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SearchJobs(
            [FromQuery] JobSearchRequestDto request)
        {
            var result =
                await _jobSearchService
                .SearchJobsAsync(request);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobDetails(
            int id)
        {
            var userId = TryGetUserId();

            var result =
                await _jobSearchService
                .GetJobDetailsAsync(
                    id,
                    userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        private int? TryGetUserId()
        {
            if (User.Identity?.IsAuthenticated != true)
                return null;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return null;

            return userId;
        }
    }
}
