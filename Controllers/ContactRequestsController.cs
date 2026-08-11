using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.ContactRequests;
using Recruitment_Project.Helpers;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api")]
    public class ContactRequestsController : ControllerBase
    {
        private readonly IContactRequestService _service;


        public ContactRequestsController(
            IContactRequestService service)
        {
            _service = service;
        }


        // Employer creates contact request
        [Authorize(Roles = RoleNames.Employer)]
        [HttpPost("employer/applications/{applicationId}/contact-request")]
        public async Task<IActionResult> Create(
            int applicationId,
            CreateContactRequestDto dto)
        {
            var userId = User.GetUserId();

            var result =
                await _service.CreateAsync(
                    userId,
                    applicationId,
                    dto);

            return Ok(result);
        }



        // Employer view sent requests
        [Authorize(Roles = RoleNames.Employer)]
        [HttpGet("employer/contact-requests")]
        public async Task<IActionResult> GetEmployerRequests()
        {
            var userId = User.GetUserId();

            var result =
                await _service.GetEmployerRequestsAsync(userId);

            return Ok(result);
        }



        // Job seeker view received requests
        [Authorize(Roles = RoleNames.JobSeeker)]
        [HttpGet("jobseekers/me/contact-requests")]
        public async Task<IActionResult> GetJobSeekerRequests()
        {
            var userId = User.GetUserId();

            var result =
                await _service.GetJobSeekerRequestsAsync(userId);

            return Ok(result);
        }



        // Job seeker accept/reject
        [Authorize(Roles = RoleNames.JobSeeker)]
        [HttpPatch("jobseekers/me/contact-requests/{id}/response")]
        public async Task<IActionResult> Respond(
            int id,
            [FromBody] RespondContactRequestDto request)
        {
            var userId = User.GetUserId();

            await _service.RespondAsync(
                userId,
                id,
                request.Response);

            return Ok(new
            {
                message = "Response updated successfully"
            });
        }
    }
}