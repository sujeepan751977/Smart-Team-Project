using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.JobSeekers;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/jobseekers")]
    [Authorize]
    public class JobSeekersController : ControllerBase
    {
        private readonly IJobSeekerService _jobSeekerService;
        private readonly ICvFileStorageService _cvService;


        public JobSeekersController(
            IJobSeekerService jobSeekerService,
            ICvFileStorageService cvService)
        {
            _jobSeekerService = jobSeekerService;
            _cvService = cvService;
        }



        [HttpGet("me/profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();

            var result =
                await _jobSeekerService
                .GetProfileAsync(userId);


            if (result == null)
                return NotFound();


            return Ok(result);
        }



        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateProfile(
            UpdateJobSeekerProfileDto dto)
        {
            var userId = GetUserId();


            var updated =
                await _jobSeekerService
                .UpdateProfileAsync(
                    userId,
                    dto);


            if (!updated)
                return NotFound();


            return Ok(new
            {
                message = "Profile updated successfully"
            });
        }



        [HttpGet("me/dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetUserId();


            var result =
                await _jobSeekerService
                .GetDashboardAsync(userId);


            if (result == null)
                return NotFound();


            return Ok(result);
        }



        [HttpPost("me/cv")]
        public async Task<IActionResult> UploadCv(
            IFormFile file)
        {
            var userId = GetUserId();


            var result =
                await _cvService
                .UploadCvAsync(
                    userId,
                    file);


            return Ok(result);
        }



        [HttpGet("me/cv")]
        public async Task<IActionResult> GetCv()
        {
            var userId = GetUserId();


            var result =
                await _cvService
                .GetCvAsync(userId);


            if (result == null)
                return NotFound();


            return Ok(result);
        }

        [HttpPut("me/cv")]
        public async Task<IActionResult> ReplaceCv(IFormFile file)
        {
            var userId = GetUserId();

            var result =
                await _cvService
                .ReplaceCvAsync(
                    userId,
                    file);

            return Ok(new
            {
                message = "CV replaced successfully"
            });
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