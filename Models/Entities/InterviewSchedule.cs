using Recruitment_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class InterviewSchedule
    {
        public int Id { get; set; }

        public int JobApplicationId { get; set; }

        public DateTime InterviewDate { get; set; }

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(500)]
        public string MeetingLink { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public JobApplication JobApplication { get; set; } = null!;
    }
}
