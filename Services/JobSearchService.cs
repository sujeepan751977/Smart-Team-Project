using Recruitment_Project.DTOs.Jobs;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class JobSearchService : IJobSearchService
    {
        private readonly IJobSearchRepository _repository;
        private readonly IMatchingService _matchingService;
        private readonly IJobTrustService _jobTrustService;
        private readonly IJobSeekerRepository _jobSeekerRepository;
        private readonly IApplicationRepository _applicationRepository;

        public JobSearchService(
            IJobSearchRepository repository,
            IMatchingService matchingService,
            IJobTrustService jobTrustService,
            IJobSeekerRepository jobSeekerRepository,
            IApplicationRepository applicationRepository)
        {
            _repository = repository;
            _matchingService = matchingService;
            _jobTrustService = jobTrustService;
            _jobSeekerRepository = jobSeekerRepository;
            _applicationRepository = applicationRepository;
        }

        public async Task<List<VacancyListDto>> SearchJobsAsync(
            JobSearchRequestDto request,
            int? userId = null)
        {
            if (request.PageNumber < 1)
                request.PageNumber = 1;

            if (request.PageSize < 1)
                request.PageSize = 10;

            var vacancies =
                await _repository.SearchJobsAsync(
                    request.Search,
                    request.Company,
                    request.Skill,
                    request.Location,
                    request.MinimumExperience,
                    request.PageNumber,
                    request.PageSize);

            var jobSeeker = userId.HasValue
                ? await _jobSeekerRepository
                    .GetProfileByUserIdAsync(userId.Value)
                : null;

            var results = new List<VacancyListDto>();

            foreach (var x in vacancies)
            {
                double matchScore = 0;
                var hasMatchScore = jobSeeker != null;

                if (jobSeeker != null)
                {
                    var match =
                        await _matchingService
                            .CalculateMatchAsync(
                                jobSeeker.Id,
                                x.Id);

                    matchScore = match.TotalMatchScore;
                }

                results.Add(new VacancyListDto
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
                        ParseExperience(x.ExperienceLevel),

                    EducationRequirement =
                        x.Requirements,

                    RequiredSkills =
                        x.RequiredSkills
                        .Where(s => s.Skill != null)
                        .Select(s => s.Skill.Name)
                        .ToList(),

                    MatchScore = matchScore,
                    HasMatchScore = hasMatchScore
                });
            }

            if (jobSeeker != null)
            {
                results = results
                    .OrderByDescending(x => x.MatchScore)
                    .ThenBy(x => x.JobTitle)
                    .ToList();
            }

            return results;
        }

        public async Task<JobSeekerVacancyDetailsDto?> GetJobDetailsAsync(
            int jobId,
            int? userId)
        {
            var vacancy =
                await _repository.GetJobDetailsAsync(jobId);

            if (vacancy == null)
                return null;

            var jobSeeker = userId.HasValue
                ? await _jobSeekerRepository
                    .GetProfileByUserIdAsync(userId.Value)
                : null;

            var profileId = jobSeeker?.Id ?? 0;

            var match =
                await _matchingService
                .CalculateMatchAsync(
                    profileId,
                    jobId);

            var trustLabel =
                await _jobTrustService
                .GetTrustLabelAsync(jobId);

            var alreadyApplied = false;

            if (jobSeeker != null)
            {
                var existing =
                    await _applicationRepository
                        .GetByVacancyAndJobSeekerAsync(
                            jobId,
                            jobSeeker.Id);

                alreadyApplied = existing != null;
            }

            var canApply =
                vacancy.Status == VacancyStatus.Open &&
                vacancy.ExpiryDate >= DateTime.UtcNow &&
                vacancy.EmployerProfile.AccountStatus != EmployerAccountStatus.Disabled &&
                vacancy.EmployerProfile.AccountStatus != EmployerAccountStatus.Suspended &&
                !alreadyApplied &&
                jobSeeker != null;

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
                    ParseExperience(vacancy.ExperienceLevel),

                EducationRequirement =
                    vacancy.Requirements,

                RequiredSkills =
                    vacancy.RequiredSkills
                    .Where(x => x.Skill != null)
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

                CanApply = canApply,

                TrustLabel =
                    trustLabel
            };
        }

        private static int ParseExperience(string experienceLevel)
        {
            if (string.IsNullOrWhiteSpace(experienceLevel))
                return 0;

            var digits = new string(
                experienceLevel
                    .Where(char.IsDigit)
                    .ToArray());

            return int.TryParse(digits, out var years)
                ? years
                : 0;
        }
    }
}
