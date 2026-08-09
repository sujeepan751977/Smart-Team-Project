using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Jobs;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobSearchService _jobSearchService;



        public JobsController(
            IJobSearchService jobSearchService)
        {
            _jobSearchService = jobSearchService;
        }



        [HttpGet]
        public async Task<IActionResult> SearchJobs(
            [FromQuery] JobSearchRequestDto request)
        {
            var result =
                await _jobSearchService
                .SearchJobsAsync(request);


            return Ok(result);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobDetails(
            int id)
        {
            var userId = GetUserId();


            var result =
                await _jobSearchService
                .GetJobDetailsAsync(
                    id,
                    userId);


            if (result == null)
                return NotFound();


            return Ok(result);
        }



        private int GetUserId()
        {
            var claim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                throw new Exception("User id claim not found");
            }

            return int.Parse(claim.Value);
        }
    }
    
}