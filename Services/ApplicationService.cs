using Recruitment_Project.DTOs.Applications;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobSeekerRepository _jobSeekerRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMatchingService _matchingService;
        private readonly IEmployerRepository _employerRepository;
        private readonly INotificationService _notificationService;

        public ApplicationService(
            IApplicationRepository applicationRepository,
            IJobSeekerRepository jobSeekerRepository,
            IVacancyRepository vacancyRepository,
            IMatchingService matchingService,
            IEmployerRepository employerRepository,
            INotificationService notificationService)
        {
            _applicationRepository = applicationRepository;
            _jobSeekerRepository = jobSeekerRepository;
            _vacancyRepository = vacancyRepository;
            _matchingService = matchingService;
            _employerRepository = employerRepository;
            _notificationService = notificationService;
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

            if (vacancy.ExpiryDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "This vacancy has expired.");
            }

            if (vacancy.EmployerProfile.AccountStatus ==
                    EmployerAccountStatus.Disabled ||
                vacancy.EmployerProfile.AccountStatus ==
                    EmployerAccountStatus.Suspended)
            {
                throw new InvalidOperationException(
                    "Applications are not allowed for this employer.");
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

            // Reload application with Vacancy -> EmployerProfile
            // and JobSeekerProfile -> User navigation properties.
            var savedApplication =
                await _applicationRepository
                    .GetByIdAsync(application.Id);

            if (savedApplication != null)
            {
                var employerUserId =
                    savedApplication
                        .Vacancy
                        .EmployerProfile
                        .UserId;

                await _notificationService.CreateAsync(
                    new Notification
                    {
                        UserId = employerUserId,
                        Type = NotificationType.Application,
                        Title = "New Job Application",
                        Message =
                            $"A new application has been submitted for your vacancy \"{savedApplication.Vacancy.Title}\"."
                    });
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
                await _employerRepository
                    .GetByUserIdAsync(userId);

            if (employer == null)
            {
                throw new KeyNotFoundException(
                    "Employer profile not found.");
            }

            var applications =
                await _applicationRepository
                    .GetByEmployerAsync(employer.Id);

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

        public async Task<ApplicationDto?> GetApplicationByIdAsync(
            int userId,
            int applicationId)
        {
            var application =
                await _applicationRepository
                    .GetByIdAsync(applicationId);

            if (application == null)
            {
                return null;
            }

            var employer =
                await _employerRepository
                    .GetByUserIdAsync(userId);

            var jobSeeker =
                await _jobSeekerRepository
                    .GetProfileByUserIdAsync(userId);

            var isEmployerOwner =
                employer != null &&
                application.Vacancy.EmployerProfileId == employer.Id;

            var isJobSeekerOwner =
                jobSeeker != null &&
                application.JobSeekerProfileId == jobSeeker.Id;

            if (!isEmployerOwner && !isJobSeekerOwner)
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
            var employer =
                await _employerRepository
                    .GetByUserIdAsync(userId);

            if (employer == null)
            {
                throw new KeyNotFoundException(
                    "Employer profile not found.");
            }

            var application =
                await _applicationRepository
                    .GetByIdAsync(applicationId);

            if (application == null)
            {
                throw new KeyNotFoundException(
                    "Application not found.");
            }

            if (application.Vacancy.EmployerProfileId != employer.Id)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to update this application.");
            }

            if (!Enum.TryParse<ApplicationStatus>(
                status,
                true,
                out var newStatus))
            {
                throw new InvalidOperationException(
                    "Invalid application status.");
            }

            if (!IsValidStatusTransition(application.Status, newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot change application status from {application.Status} to {newStatus}.");
            }

            var previousStatus = application.Status;

            application.Status = newStatus;
            application.UpdatedAt = DateTime.UtcNow;

            await _applicationRepository.SaveChangesAsync();

            // Notify JobSeeker about application status change.
            var jobSeekerUserId =
                application
                    .JobSeekerProfile
                    .UserId;

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = jobSeekerUserId,
                    Type = NotificationType.Application,
                    Title = "Application Status Updated",
                    Message =
                        $"Your application for \"{application.Vacancy.Title}\" has changed from {previousStatus} to {newStatus}."
                });
        }

        public async Task<List<ApplicationDto>> GetApplicantsByVacancyAsync(
            int userId,
            int vacancyId)
        {
            var employer =
                await _employerRepository
                    .GetByUserIdAsync(userId);

            if (employer == null)
            {
                throw new KeyNotFoundException(
                    "Employer profile not found.");
            }

            var vacancy =
                await _vacancyRepository
                    .GetByIdAsync(vacancyId);

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
                await _applicationRepository
                    .GetByVacancyAsync(vacancyId);

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

        private static bool IsValidStatusTransition(
            ApplicationStatus current,
            ApplicationStatus next)
        {
            if (current == next)
                return true;

            return current switch
            {
                ApplicationStatus.Applied =>
                    next is ApplicationStatus.UnderReview
                        or ApplicationStatus.Shortlisted
                        or ApplicationStatus.Rejected,

                ApplicationStatus.UnderReview =>
                    next is ApplicationStatus.Shortlisted
                        or ApplicationStatus.Rejected,

                ApplicationStatus.Shortlisted =>
                    next is ApplicationStatus.Rejected,

                _ => false
            };
        }
    }
}
