using Recruitment_Project.DTOs.EmployerVerification;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class EmployerVerificationService : IEmployerVerificationService
    {
        private readonly IEmployerRepository _employerRepository;
        private readonly IEmployerVerificationRepository _verificationRepository;

        public EmployerVerificationService(
            IEmployerRepository employerRepository,
            IEmployerVerificationRepository verificationRepository)
        {
            _employerRepository = employerRepository;
            _verificationRepository = verificationRepository;
        }

        public async Task<EmployerVerificationDto?> GetVerificationAsync(int userId)
        {
            var employerProfile = await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                return null;

            var verification =
                await _verificationRepository.GetByEmployerProfileIdAsync(
                    employerProfile.Id);

            if (verification == null)
                return null;

            return new EmployerVerificationDto
            {
                Id = verification.Id,
                EmployerProfileId = verification.EmployerProfileId,
                Status = verification.Status,
                AdministratorFeedback = verification.AdministratorFeedback,
                SubmittedAt = verification.SubmittedAt,
                ReviewedAt = verification.ReviewedAt,

                Documents = verification.Documents
                    .Select(x => new EmployerVerificationDocumentDto
                    {
                        Id = x.Id,
                        EmployerVerificationId = x.EmployerVerificationId,
                        FileName = x.FileName,
                        FilePath = x.FilePath,
                        ContentType = x.ContentType,
                        Status = x.Status,
                        AdministratorComment = x.AdministratorComment,
                        UploadedAt = x.UploadedAt
                    })
                    .ToList()
            };
        }

        public async Task SubmitVerificationAsync(int userId)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                throw new Exception("Employer profile not found");

            var verification =
                await _verificationRepository.GetByEmployerProfileIdAsync(
                    employerProfile.Id);

            if (verification == null)
            {
                verification = new EmployerVerification
                {
                    EmployerProfileId = employerProfile.Id,
                    Status = EmployerVerificationStatus.PendingReview,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _verificationRepository.AddAsync(verification);
            }
            else
            {
                verification.Status = EmployerVerificationStatus.PendingReview;
                verification.SubmittedAt = DateTime.UtcNow;
                verification.UpdatedAt = DateTime.UtcNow;

                await _verificationRepository.UpdateAsync(verification);
            }

            await _verificationRepository.SaveChangesAsync();
        }

        public async Task AddDocumentAsync(
            int userId,
            EmployerVerificationDocumentDto document)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                throw new Exception("Employer profile not found");

            var verification =
                await _verificationRepository.GetByEmployerProfileIdAsync(
                    employerProfile.Id);

            if (verification == null)
            {
                verification = new EmployerVerification
                {
                    EmployerProfileId = employerProfile.Id,
                    Status = EmployerVerificationStatus.Unverified,
                    CreatedAt = DateTime.UtcNow
                };

                await _verificationRepository.AddAsync(verification);
                await _verificationRepository.SaveChangesAsync();
            }

            var newDocument = new EmployerVerificationDocument
            {
                EmployerVerificationId = verification.Id,
                FileName = document.FileName,
                FilePath = document.FilePath,
                ContentType = document.ContentType,
                Status = VerificationDocumentStatus.Pending,
                UploadedAt = DateTime.UtcNow
            };

            verification.Documents.Add(newDocument);

            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }
    }
}