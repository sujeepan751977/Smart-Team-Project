using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.DTOs.EmployerVerification
{
    public class EmployerVerificationDto
    {
        public int Id { get; set; }

        public int EmployerProfileId { get; set; }

        public string? CompanyName { get; set; }

        public EmployerVerificationStatus Status { get; set; }

        public string? AdministratorFeedback { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public List<EmployerVerificationDocumentDto> Documents { get; set; }
            = new List<EmployerVerificationDocumentDto>();
    }
}
