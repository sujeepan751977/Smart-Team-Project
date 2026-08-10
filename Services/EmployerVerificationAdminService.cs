using Recruitment_Project.DTOs.EmployerVerification;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class EmployerVerificationAdminService
        : IEmployerVerificationAdminService
    {
        private readonly IEmployerVerificationRepository _verificationRepository;
        private readonly INotificationService _notificationService;

        public EmployerVerificationAdminService(
            IEmployerVerificationRepository verificationRepository,
            INotificationService notificationService)
        {
            _verificationRepository = verificationRepository;
            _notificationService = notificationService;
        }

        public async Task<List<EmployerVerificationDto>> GetAllAsync()
        {
            var verifications =
                await _verificationRepository.GetAllAsync();

            return verifications
                .Select(MapToDto)
                .ToList();
        }

        public async Task<EmployerVerificationDto?> GetByIdAsync(
            int id)
        {
            var verification =
                await _verificationRepository.GetByIdAsync(id);

            if (verification == null)
                return null;

            return MapToDto(verification);
        }

        public async Task RequestInformationAsync(
            int id,
            string feedback)
        {
            var verification =
                await _verificationRepository.GetByIdAsync(id);

            if (verification == null)
                throw new Exception(
                    "Employer verification not found");

            if (verification.Status !=
                EmployerVerificationStatus.PendingReview)
            {
                throw new Exception(
                    "Only pending verifications can request information");
            }

            verification.Status =
                EmployerVerificationStatus.MoreInformationRequired;

            verification.AdministratorFeedback = feedback;
            verification.ReviewedAt = DateTime.UtcNow;
            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(
                verification);

            await _verificationRepository.SaveChangesAsync();

            // Notify Employer
            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId =
                        verification
                            .EmployerProfile
                            .UserId,

                    Type =
                        NotificationType.EmployerVerification,

                    Title =
                        "Additional Verification Information Required",

                    Message =
                        $"Additional information is required for your employer verification. " +
                        $"Administrator feedback: {feedback}"
                });
        }

        public async Task VerifyAsync(int id)
        {
            var verification =
                await _verificationRepository.GetByIdAsync(id);

            if (verification == null)
                throw new Exception(
                    "Employer verification not found");

            if (verification.Status !=
                EmployerVerificationStatus.PendingReview)
            {
                throw new Exception(
                    "Only pending verifications can be verified");
            }

            verification.Status =
                EmployerVerificationStatus.Verified;

            verification.ReviewedAt = DateTime.UtcNow;
            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(
                verification);

            await _verificationRepository.SaveChangesAsync();

            // Notify Employer
            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId =
                        verification
                            .EmployerProfile
                            .UserId,

                    Type =
                        NotificationType.EmployerVerification,

                    Title =
                        "Employer Verification Approved",

                    Message =
                        "Your employer verification has been approved by the administrator."
                });
        }

        public async Task RejectAsync(
            int id,
            string feedback)
        {
            var verification =
                await _verificationRepository.GetByIdAsync(id);

            if (verification == null)
                throw new Exception(
                    "Employer verification not found");

            if (verification.Status !=
                EmployerVerificationStatus.PendingReview)
            {
                throw new Exception(
                    "Only pending verifications can be rejected");
            }

            verification.Status =
                EmployerVerificationStatus.Rejected;

            verification.AdministratorFeedback = feedback;
            verification.ReviewedAt = DateTime.UtcNow;
            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(
                verification);

            await _verificationRepository.SaveChangesAsync();

            // Notify Employer
            await _notificationService.CreateAsync(
                new Notification
                {
                    UserId =
                        verification
                            .EmployerProfile
                            .UserId,

                    Type =
                        NotificationType.EmployerVerification,

                    Title =
                        "Employer Verification Rejected",

                    Message =
                        $"Your employer verification has been rejected. " +
                        $"Administrator feedback: {feedback}"
                });
        }

        private static EmployerVerificationDto MapToDto(
            EmployerVerification verification)
        {
            return new EmployerVerificationDto
            {
                Id = verification.Id,

                EmployerProfileId =
                    verification.EmployerProfileId,

                Status =
                    verification.Status,

                AdministratorFeedback =
                    verification.AdministratorFeedback,

                SubmittedAt =
                    verification.SubmittedAt,

                ReviewedAt =
                    verification.ReviewedAt,

                Documents =
                    verification.Documents
                        .Select(x =>
                            new EmployerVerificationDocumentDto
                            {
                                Id = x.Id,

                                EmployerVerificationId =
                                    x.EmployerVerificationId,

                                FileName =
                                    x.FileName,

                                FilePath =
                                    x.FilePath,

                                ContentType =
                                    x.ContentType,

                                Status =
                                    x.Status,

                                AdministratorComment =
                                    x.AdministratorComment,

                                UploadedAt =
                                    x.UploadedAt
                            })
                        .ToList()
            };
        }
    }
}