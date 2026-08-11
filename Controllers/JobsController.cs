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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SearchJobs(
            [FromQuery] JobSearchRequestDto request)
        {
            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            Response.Headers.Pragma = "no-cache";

            var result =
                await _jobSearchService
                .SearchJobsAsync(request, TryGetUserId());

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobDetails(
            int id)
        {
            var result =
                await _jobSearchService
                .GetJobDetailsAsync(
                    id,
                    TryGetUserId());

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        private int? TryGetUserId()
        {
            if (User.Identity?.IsAuthenticated != true)
                return null;

            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("nameid");

            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return null;

            return userId;
        }
    }
}
