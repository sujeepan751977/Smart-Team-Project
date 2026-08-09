using Recruitment_Project.DTOs.Employers;
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
        private readonly IVerificationDocumentStorageService _storageService;

        public EmployerVerificationService(
            IEmployerRepository employerRepository,
            IEmployerVerificationRepository verificationRepository,
            IVerificationDocumentStorageService storageService)
        {
            _employerRepository = employerRepository;
            _verificationRepository = verificationRepository;
            _storageService = storageService;
        }

        public async Task<EmployerVerificationDto?> GetVerificationAsync(int userId)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

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
                verification.Status =
                    EmployerVerificationStatus.PendingReview;

                verification.SubmittedAt = DateTime.UtcNow;
                verification.UpdatedAt = DateTime.UtcNow;

                await _verificationRepository.UpdateAsync(verification);
            }

            await _verificationRepository.SaveChangesAsync();
        }

        public async Task WithdrawVerificationAsync(int userId)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                throw new Exception("Employer profile not found");

            var verification =
                await _verificationRepository.GetByEmployerProfileIdAsync(
                    employerProfile.Id);

            if (verification == null)
                throw new Exception("Employer verification not found");

            if (verification.Status !=
                EmployerVerificationStatus.PendingReview)
            {
                throw new Exception(
                    "Only pending verification requests can be withdrawn");
            }

            verification.Status =
                EmployerVerificationStatus.Unverified;

            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }

        public async Task ResubmitVerificationAsync(int userId)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                throw new Exception("Employer profile not found");

            var verification =
                await _verificationRepository.GetByEmployerProfileIdAsync(
                    employerProfile.Id);

            if (verification == null)
                throw new Exception("Employer verification not found");

            if (verification.Status !=
                    EmployerVerificationStatus.Rejected &&
                verification.Status !=
                    EmployerVerificationStatus.MoreInformationRequired)
            {
                throw new Exception(
                    "Only rejected or information-required verification can be resubmitted");
            }

            verification.Status =
                EmployerVerificationStatus.PendingReview;

            verification.SubmittedAt = DateTime.UtcNow;
            verification.ReviewedAt = null;
            verification.AdministratorFeedback = null;
            verification.UpdatedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(verification);
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

        public async Task AddDocumentFileAsync(
            int userId,
            UploadEmployerVerificationDocumentDto request)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                throw new Exception("Employer profile not found");

            if (request.File == null || request.File.Length == 0)
                throw new Exception("Document file is required");

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

            await using var stream =
                request.File.OpenReadStream();

            var filePath =
                await _storageService.SaveAsync(
                    stream,
                    request.File.FileName,
                    request.File.ContentType);

            var newDocument = new EmployerVerificationDocument
            {
                EmployerVerificationId = verification.Id,
                FileName = request.File.FileName,
                FilePath = filePath,
                ContentType = request.File.ContentType,
                Status = VerificationDocumentStatus.Pending,
                UploadedAt = DateTime.UtcNow
            };

            verification.Documents.Add(newDocument);

            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(
            int userId,
            int documentId)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                throw new Exception("Employer profile not found");

            var verification =
                await _verificationRepository.GetByEmployerProfileIdAsync(
                    employerProfile.Id);

            if (verification == null)
                throw new Exception("Employer verification not found");

            var document = verification.Documents
                .FirstOrDefault(x => x.Id == documentId);

            if (document == null)
                throw new Exception(
                    "Verification document not found");

            await _verificationRepository.DeleteDocumentAsync(
                documentId);

            await _verificationRepository.SaveChangesAsync();
        }

        public async Task UpdateCompanyInformationAsync(
            int userId,
            UpdateEmployerProfileDto dto)
        {
            var employerProfile =
                await _employerRepository.GetByUserIdAsync(userId);

            if (employerProfile == null)
                throw new Exception("Employer profile not found");

            employerProfile.CompanyName =
                dto.CompanyName;

            employerProfile.RegisteredCompanyName =
                dto.RegisteredCompanyName;

            employerProfile.RegistrationNumber =
                dto.RegistrationNumber;

            employerProfile.Industry =
                dto.Industry;

            employerProfile.RegisteredAddress =
                dto.RegisteredAddress;

            employerProfile.OperatingLocation =
                dto.OperatingLocation;

            employerProfile.OfficialCompanyEmail =
                dto.OfficialCompanyEmail;

            employerProfile.CompanyPhone =
                dto.CompanyPhone;

            employerProfile.Website =
                dto.Website;

            employerProfile.AuthorizedRepresentative =
                dto.AuthorizedRepresentative;

            employerProfile.CompanyDescription =
                dto.CompanyDescription;

            employerProfile.UpdatedAt =
                DateTime.UtcNow;

            await _employerRepository.UpdateAsync(
                employerProfile);

            await _employerRepository.SaveChangesAsync();
        }
    }
}