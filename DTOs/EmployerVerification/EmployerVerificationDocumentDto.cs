using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.DTOs.EmployerVerification
{
    public class EmployerVerificationDocumentDto
    {
        public int Id { get; set; }

        public int EmployerVerificationId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public VerificationDocumentStatus Status { get; set; }

        public string? AdministratorComment { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}