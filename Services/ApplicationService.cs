using Recruitment_Project.DTOs.Applications;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;
using Recruitment_Project.Repositories;

namespace Recruitment_Project.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobSeekerRepository _jobSeekerRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMatchingService _matchingService;
        private readonly IEmployerRepository _employerRepository;
        public ApplicationService(
            IApplicationRepository applicationRepository,
            IJobSeekerRepository jobSeekerRepository,
            IVacancyRepository vacancyRepository,
            IMatchingService matchingService,
            IEmployerRepository employerRepository)
        {
            _applicationRepository = applicationRepository;
            _jobSeekerRepository = jobSeekerRepository;
            _vacancyRepository = vacancyRepository;
            _matchingService = matchingService;
            _employerRepository = employerRepository;
        }

        public async Task<ApplicationDto> ApplyAsync(
            int userId,
            int vacancyId,
            ApplyJobDto dto)
        {
            var jobSeeker =
                await _jobSeekerRepository
                    .GetProfileByUserIdAsync(userId);

            if (jobSeeker == null)
            {
                throw new KeyNotFoundException(
                    "Job seeker profile not found.");
            }

            var vacancy =
                await _vacancyRepository
                    .GetByIdAsync(vacancyId);

            if (vacancy == null)
            {
                throw new KeyNotFoundException(
                    "Vacancy not found.");
            }

            if (vacancy.Status != VacancyStatus.Open)
            {
                throw new InvalidOperationException(
                    "Applications are only allowed for open vacancies.");
            }

            var existingApplication =
                await _applicationRepository
                    .GetByVacancyAndJobSeekerAsync(
                        vacancyId,
                        jobSeeker.Id);

            if (existingApplication != null)
            {
                throw new InvalidOperationException(
                    "You have already applied for this vacancy.");
            }

            var matchResult =
                await _matchingService
                    .CalculateMatchAsync(
                        jobSeeker.Id,
                        vacancyId);

            var application = new JobApplication
            {
                VacancyId = vacancyId,
                JobSeekerProfileId = jobSeeker.Id,
                Status = ApplicationStatus.Applied,
                MatchScore = (decimal)matchResult.TotalMatchScore,
                CoverLetter = dto.CoverLetter,
                AppliedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddAsync(application);
            await _applicationRepository.SaveChangesAsync();

            return new ApplicationDto
            {
                Id = application.Id,
                VacancyId = application.VacancyId,
                JobSeekerProfileId = application.JobSeekerProfileId,
                Status = application.Status.ToString(),
                MatchScore = application.MatchScore,
                CoverLetter = application.CoverLetter,
                AppliedAt = application.AppliedAt,
                UpdatedAt = application.UpdatedAt
            };
        }

        public async Task<List<ApplicationDto>> GetMyApplicationsAsync(
      int userId)
        {
            var jobSeeker =
                await _jobSeekerRepository
                    .GetProfileByUserIdAsync(userId);

            if (jobSeeker == null)
            {
                throw new KeyNotFoundException(
                    "Job seeker profile not found.");
            }

            var applications =
                await _applicationRepository
                    .GetByJobSeekerAsync(jobSeeker.Id);

            return applications
                .Select(x => new ApplicationDto
                {
                    Id = x.Id,
                    VacancyId = x.VacancyId,
                    JobSeekerProfileId = x.JobSeekerProfileId,
                    Status = x.Status.ToString(),
                    MatchScore = x.MatchScore,
                    CoverLetter = x.CoverLetter,
                    AppliedAt = x.AppliedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToList();
        }

        public async Task<List<ApplicationDto>> GetEmployerApplicationsAsync(
            int userId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new KeyNotFoundException(
                    "Employer profile not found.");

            var applications =
                await _applicationRepository
                    .GetByEmployerAsync(employer.Id);


            return applications.Select(x => new ApplicationDto
            {
                Id = x.Id,
                VacancyId = x.VacancyId,
                JobSeekerProfileId = x.JobSeekerProfileId,
                Status = x.Status.ToString(),
                MatchScore = x.MatchScore,
                CoverLetter = x.CoverLetter,
                AppliedAt = x.AppliedAt,
                UpdatedAt = x.UpdatedAt

            }).ToList();
        }

        public async Task<ApplicationDto?> GetApplicationByIdAsync(
            int userId,
            int applicationId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
            {
                throw new KeyNotFoundException(
                    "Employer profile not found.");
            }

            var application =
                await _applicationRepository.GetByIdAsync(applicationId);

            if (application == null)
            {
                return null;
            }

            if (application.Vacancy.EmployerProfileId != employer.Id)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view this application.");
            }

            return new ApplicationDto
            {
                Id = application.Id,
                VacancyId = application.VacancyId,
                JobSeekerProfileId = application.JobSeekerProfileId,
                Status = application.Status.ToString(),
                MatchScore = application.MatchScore,
                CoverLetter = application.CoverLetter,
                AppliedAt = application.AppliedAt,
                UpdatedAt = application.UpdatedAt
            };
        }
        public async Task UpdateStatusAsync(
            int userId,
            int applicationId,
            string status)
        {
            var application =
                await _applicationRepository
                    .GetByIdAsync(applicationId);

            if (application == null)
            {
                throw new KeyNotFoundException(
                    "Application not found.");
            }

            if (Enum.TryParse<ApplicationStatus>(
                status,
                true,
                out var newStatus))
            {
                application.Status = newStatus;
                application.UpdatedAt = DateTime.UtcNow;

                await _applicationRepository.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid application status.");
            }
        }

        public async Task<List<ApplicationDto>> GetApplicantsByVacancyAsync(
            int userId,
            int vacancyId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
            {
                throw new KeyNotFoundException(
                    "Employer profile not found.");
            }

            var vacancy =
                await _vacancyRepository.GetByIdAsync(vacancyId);

            if (vacancy == null)
            {
                throw new KeyNotFoundException(
                    "Vacancy not found.");
            }

            if (vacancy.EmployerProfileId != employer.Id)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view applicants for this vacancy.");
            }

            var applications =
                await _applicationRepository.GetByVacancyAsync(vacancyId);

            return applications
                .Select(x => new ApplicationDto
                {
                    Id = x.Id,
                    VacancyId = x.VacancyId,
                    JobSeekerProfileId = x.JobSeekerProfileId,
                    Status = x.Status.ToString(),
                    MatchScore = x.MatchScore,
                    CoverLetter = x.CoverLetter,
                    AppliedAt = x.AppliedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToList();
        }
    }
}