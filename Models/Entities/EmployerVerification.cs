using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Models.Entities
{
    public class EmployerVerification
    {
        public int Id { get; set; }

        public int EmployerProfileId { get; set; }

        public EmployerVerificationStatus Status { get; set; } = EmployerVerificationStatus.Unverified;

        public string? AdministratorFeedback { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public EmployerProfile EmployerProfile { get; set; } = null!;

        public ICollection<EmployerVerificationDocument> Documents { get; set; } = new List<EmployerVerificationDocument>();
    }
}
