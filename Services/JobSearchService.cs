using Recruitment_Project.DTOs.Jobs;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Services
{
    public class JobSearchService : IJobSearchService
    {
        private readonly IJobSearchRepository _repository;
        private readonly IMatchingService _matchingService;
        private readonly IJobTrustService _jobTrustService;

        public JobSearchService(
            IJobSearchRepository repository,
            IMatchingService matchingService,
            IJobTrustService jobTrustService)
        {
            _repository = repository;
            _matchingService = matchingService;
            _jobTrustService = jobTrustService;
        }

        public async Task<List<VacancyListDto>> SearchJobsAsync(
            JobSearchRequestDto request)
        {
            var vacancies =
                await _repository.SearchJobsAsync(
                    request.Search,
                    request.Company,
                    request.Skill,
                    request.Location,
                    request.MinimumExperience,
                    request.PageNumber,
                    request.PageSize);

            return vacancies.Select(x =>
                new VacancyListDto
                {
                    Id = x.Id,

                    JobTitle =
                        x.Title,

                    CompanyName =
                        x.EmployerProfile.CompanyName,

                    Location =
                        x.WorkLocation,

                    Description =
                        x.Description,

                    RequiredExperience =
                        0,

                    EducationRequirement =
                        x.Requirements,

                    RequiredSkills =
                        x.RequiredSkills
                        .Select(s => s.Skill.Name)
                        .ToList()
                })
                .ToList();
        }

        public async Task<JobSeekerVacancyDetailsDto?> GetJobDetailsAsync(
            int jobId,
            int userId)
        {
            var vacancy =
                await _repository.GetJobDetailsAsync(jobId);

            if (vacancy == null)
                return null;

            var match =
                await _matchingService
                .CalculateMatchAsync(
                    userId,
                    jobId);

            var trustLabel =
                await _jobTrustService
                .GetTrustLabelAsync(jobId);

            return new JobSeekerVacancyDetailsDto
            {
                Id = vacancy.Id,

                JobTitle =
                    vacancy.Title,

                CompanyName =
                    vacancy.EmployerProfile.CompanyName,

                Location =
                    vacancy.WorkLocation,

                Description =
                    vacancy.Description,

                RequiredExperience =
                    0,

                EducationRequirement =
                    vacancy.Requirements,

                RequiredSkills =
                    vacancy.RequiredSkills
                    .Select(x => x.Skill.Name)
                    .ToList(),

                MatchScore =
                    match.TotalMatchScore,

                MatchBreakdown =
                    match.Breakdown,

                MatchedSkills =
                    match.MatchedSkills,

                MissingSkills =
                    match.MissingSkills,

                CanApply = true,

                TrustLabel =
                    trustLabel
            };
        }
    }
}