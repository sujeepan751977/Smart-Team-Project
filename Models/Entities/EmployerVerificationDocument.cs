using Recruitment_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class EmployerVerificationDocument
    {
        public int Id { get; set; }

        public int EmployerVerificationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public VerificationDocumentStatus Status { get; set; } = VerificationDocumentStatus.Pending;

        public string? AdministratorComment { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public EmployerVerification EmployerVerification { get; set; } = null!;
    }
}
