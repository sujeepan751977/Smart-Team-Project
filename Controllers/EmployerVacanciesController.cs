using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.DTOs.Jobs;
using Recruitment_Project.Helpers;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/employer/vacancies")]
    [Authorize(Roles = RoleNames.Employer)]
    public class EmployerVacanciesController : ControllerBase
    {
        private readonly IVacancyService _vacancyService;

        public EmployerVacanciesController(
            IVacancyService vacancyService)
        {
            _vacancyService = vacancyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyVacancies()
        {
            var userId = GetUserId();

            var vacancies =
                await _vacancyService.GetMyVacanciesAsync(userId);

            return Ok(vacancies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMyVacancy(int id)
        {
            var userId = GetUserId();

            var vacancy =
                await _vacancyService.GetMyVacancyByIdAsync(
                    userId,
                    id);

            if (vacancy == null)
            {
                return NotFound("Vacancy not found");
            }

            return Ok(vacancy);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVacancy(
            [FromBody] CreateVacancyDto dto)
        {
            var userId = GetUserId();

            var vacancyId =
                await _vacancyService.CreateVacancyAsync(
                    userId,
                    dto);

            return Ok(new
            {
                Success = true,
                VacancyId = vacancyId,
                Message = "Vacancy created successfully."
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVacancy(
            int id,
            [FromBody] UpdateVacancyDto dto)
        {
            var userId = GetUserId();

            await _vacancyService.UpdateVacancyAsync(
                userId,
                id,
                dto);

            return Ok(new
            {
                Success = true,
                Message = "Vacancy updated successfully."
            });
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitVacancy(int id)
        {
            var userId = GetUserId();

            await _vacancyService.SubmitVacancyAsync(
                userId,
                id);

            return Ok(new
            {
                Success = true,
                Message = "Vacancy submitted successfully."
            });
        }

        [HttpPatch("{id}/close")]
        public async Task<IActionResult> CloseVacancy(int id)
        {
            var userId = GetUserId();

            await _vacancyService.CloseVacancyAsync(
                userId,
                id);

            return Ok(new
            {
                Success = true,
                Message = "Vacancy closed successfully."
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