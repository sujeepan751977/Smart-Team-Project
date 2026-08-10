using Recruitment_Project.DTOs.ContactRequests;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class ContactRequestService : IContactRequestService
    {
        private readonly IContactRequestRepository _contactRepository;
        private readonly IEmployerRepository _employerRepository;
        private readonly IJobSeekerRepository _jobSeekerRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly INotificationService _notificationService;

        public ContactRequestService(
            IContactRequestRepository contactRepository,
            IEmployerRepository employerRepository,
            IJobSeekerRepository jobSeekerRepository,
            IApplicationRepository applicationRepository,
            INotificationService notificationService)
        {
            _contactRepository = contactRepository;
            _employerRepository = employerRepository;
            _jobSeekerRepository = jobSeekerRepository;
            _applicationRepository = applicationRepository;
            _notificationService = notificationService;
        }

        public async Task<ContactRequestDto> CreateAsync(
            int userId,
            int applicationId,
            CreateContactRequestDto dto)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new KeyNotFoundException(
                    "Employer profile not found.");

            var application =
                await _applicationRepository
                    .GetByIdAsync(applicationId);

            if (application == null)
                throw new KeyNotFoundException(
                    "Application not found.");

            if (application.Vacancy.EmployerProfileId != employer.Id)
                throw new UnauthorizedAccessException(
                    "Not your application.");

            var existing =
                await _contactRepository
                    .GetPendingRequestAsync(applicationId);

            if (existing != null)
                throw new InvalidOperationException(
                    "Pending contact request already exists.");

            var request = new ContactRequest
            {
                JobApplicationId = applicationId,
                EmployerProfileId = employer.Id,
                JobSeekerProfileId = application.JobSeekerProfileId,
                EmployerMessage = dto.EmployerMessage,
                Status = ContactRequestStatus.Pending
            };

            await _contactRepository.AddAsync(request);
            await _contactRepository.SaveChangesAsync();

            // Notify JobSeeker
            var jobSeekerUserId =
                application.JobSeekerProfile.UserId;

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = jobSeekerUserId,
                    Type = NotificationType.ContactRequest,
                    Title = "New Contact Request",
                    Message =
                        $"An employer has sent you a contact request for the vacancy \"{application.Vacancy.Title}\"."
                });

            return Map(request);
        }

        public async Task<List<ContactRequestDto>> GetEmployerRequestsAsync(
            int userId)
        {
            var employer =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employer == null)
                throw new KeyNotFoundException(
                    "Employer profile not found.");

            var data =
                await _contactRepository
                    .GetByEmployerAsync(employer.Id);

            return data.Select(Map).ToList();
        }

        public async Task<List<ContactRequestDto>> GetJobSeekerRequestsAsync(
            int userId)
        {
            var seeker =
                await _jobSeekerRepository
                    .GetProfileByUserIdAsync(userId);

            if (seeker == null)
                throw new KeyNotFoundException(
                    "Job seeker profile not found.");

            var data =
                await _contactRepository
                    .GetByJobSeekerAsync(seeker.Id);

            return data.Select(Map).ToList();
        }

        public async Task RespondAsync(
            int userId,
            int requestId,
            string response)
        {
            var seeker =
                await _jobSeekerRepository
                    .GetProfileByUserIdAsync(userId);

            if (seeker == null)
                throw new KeyNotFoundException(
                    "Job seeker profile not found.");

            var request =
                await _contactRepository
                    .GetByIdAsync(requestId);

            if (request == null)
                throw new KeyNotFoundException(
                    "Contact request not found.");

            if (request.JobSeekerProfileId != seeker.Id)
                throw new UnauthorizedAccessException(
                    "You are not authorized to respond to this request.");

            if (response.Equals(
                    "Accepted",
                    StringComparison.OrdinalIgnoreCase))
            {
                request.Status =
                    ContactRequestStatus.Accepted;
            }
            else if (response.Equals(
                         "Rejected",
                         StringComparison.OrdinalIgnoreCase))
            {
                request.Status =
                    ContactRequestStatus.Rejected;
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid response.");
            }

            request.JobSeekerResponse = response;
            request.RespondedAt = DateTime.UtcNow;

            await _contactRepository.UpdateAsync(request);
            await _contactRepository.SaveChangesAsync();

            // Notify Employer
            var employerUserId =
                request.EmployerProfile.UserId;

            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId = employerUserId,
                    Type = NotificationType.ContactRequest,
                    Title = "Contact Request Response",
                    Message =
                        $"Your contact request has been {request.Status.ToString().ToLower()}."
                });
        }

        private ContactRequestDto Map(
            ContactRequest x)
        {
            return new ContactRequestDto
            {
                Id = x.Id,
                JobApplicationId = x.JobApplicationId,
                EmployerProfileId = x.EmployerProfileId,
                JobSeekerProfileId = x.JobSeekerProfileId,
                Status = x.Status.ToString(),
                EmployerMessage = x.EmployerMessage,
                JobSeekerResponse = x.JobSeekerResponse,
                RequestedAt = x.RequestedAt,
                RespondedAt = x.RespondedAt
            };
        }
    }
}