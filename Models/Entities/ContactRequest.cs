using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Models.Entities
{
    public class ContactRequest
    {
        public int Id { get; set; }

        public int JobApplicationId { get; set; }

        public int EmployerProfileId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public ContactRequestStatus Status { get; set; } = ContactRequestStatus.Pending;

        public string? EmployerMessage { get; set; }

        public string? JobSeekerResponse { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        // Navigation Properties
        public JobApplication JobApplication { get; set; } = null!;

        public EmployerProfile EmployerProfile { get; set; } = null!;

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}
