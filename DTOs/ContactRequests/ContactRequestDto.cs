namespace Recruitment_Project.DTOs.ContactRequests
{
    public class ContactRequestDto
    {
        public int Id { get; set; }

        public int JobApplicationId { get; set; }

        public int EmployerProfileId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? EmployerMessage { get; set; }

        public string? JobSeekerResponse { get; set; }

        public DateTime RequestedAt { get; set; }

        public DateTime? RespondedAt { get; set; }
    }
}