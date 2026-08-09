using Recruitment_Project.DTOs.EmployerVerification;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Services
{
    public class EmployerVerificationAdminService : IEmployerVerificationAdminService
    {
        private readonly IEmployerVerificationRepository _verificationRepository;

        public EmployerVerificationAdminService(
            IEmployerVerificationRepository verificationRepository)
        {
            _verificationRepository = verificationRepository;
        }

        public async Task<List<EmployerVerificationDto>> GetAllAsync()
        {
            var verifications =
                await _verificationRepository.GetAllAsync();

            return verifications
                .Select(MapToDto)
                .ToList();
        }

        public async Task<EmployerVerificationDto?> GetByIdAsync(int id)
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
                throw new Exception("Employer verification not found");

            if (verification.Status != EmployerVerificationStatus.PendingReview)
                throw new Exception(
                    "Only pending verifications can request information");

            verification.Status =
                EmployerVerificationStatus.MoreInformationRequired;

            verification.AdministratorFeedback = feedback;
            verification.ReviewedAt = DateTime.UtcNow;
            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }

        public async Task VerifyAsync(int id)
        {
            var verification =
                await _verificationRepository.GetByIdAsync(id);

            if (verification == null)
                throw new Exception("Employer verification not found");

            if (verification.Status != EmployerVerificationStatus.PendingReview)
                throw new Exception(
                    "Only pending verifications can be verified");

            verification.Status =
                EmployerVerificationStatus.Verified;

            verification.ReviewedAt = DateTime.UtcNow;
            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }

        public async Task RejectAsync(
            int id,
            string feedback)
        {
            var verification =
                await _verificationRepository.GetByIdAsync(id);

            if (verification == null)
                throw new Exception("Employer verification not found");

            if (verification.Status != EmployerVerificationStatus.PendingReview)
                throw new Exception(
                    "Only pending verifications can be rejected");

            verification.Status =
                EmployerVerificationStatus.Rejected;

            verification.AdministratorFeedback = feedback;
            verification.ReviewedAt = DateTime.UtcNow;
            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }

        private static EmployerVerificationDto MapToDto(
            Models.Entities.EmployerVerification verification)
        {
            return new EmployerVerificationDto
            {
                Id = verification.Id,
                EmployerProfileId = verification.EmployerProfileId,
                Status = verification.Status,
                AdministratorFeedback =
                    verification.AdministratorFeedback,
                SubmittedAt = verification.SubmittedAt,
                ReviewedAt = verification.ReviewedAt,

                Documents = verification.Documents
                    .Select(x => new EmployerVerificationDocumentDto
                    {
                        Id = x.Id,
                        EmployerVerificationId =
                            x.EmployerVerificationId,
                        FileName = x.FileName,
                        FilePath = x.FilePath,
                        ContentType = x.ContentType,
                        Status = x.Status,
                        AdministratorComment =
                            x.AdministratorComment,
                        UploadedAt = x.UploadedAt
                    })
                    .ToList()
            };
        }
    }
}