using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.InterviewSchedules;
using Recruitment_Project.Helpers;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api")]
    public class InterviewSchedulesController : ControllerBase
    {
        private readonly IInterviewScheduleService _service;


        public InterviewSchedulesController(
            IInterviewScheduleService service)
        {
            _service = service;
        }


        // Employer creates interview
        [Authorize(Roles = RoleNames.Employer)]
        [HttpPost("employer/applications/{applicationId}/interview")]
        public async Task<IActionResult> Create(
            int applicationId,
            CreateInterviewScheduleDto dto)
        {
            var userId = User.GetUserId();

            var result =
                await _service.CreateAsync(
                    userId,
                    applicationId,
                    dto);

            return Ok(result);
        }



        // Employer view interviews
        [Authorize(Roles = RoleNames.Employer)]
        [HttpGet("employer/interviews")]
        public async Task<IActionResult> GetEmployer()
        {
            var userId = User.GetUserId();

            var result =
                await _service
                .GetEmployerInterviewsAsync(userId);

            return Ok(result);
        }



        // JobSeeker view interviews
        [Authorize(Roles = RoleNames.JobSeeker)]
        [HttpGet("jobseekers/me/interviews")]
        public async Task<IActionResult> GetJobSeeker()
        {
            var userId = User.GetUserId();

            var result =
                await _service
                .GetJobSeekerInterviewsAsync(userId);

            return Ok(result);
        }
    }
}