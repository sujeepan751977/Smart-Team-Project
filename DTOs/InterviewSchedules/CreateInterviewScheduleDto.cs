using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.DTOs.InterviewSchedules
{
    public class CreateInterviewScheduleDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime InterviewDate { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        public string Location { get; set; } = string.Empty;

        public string MeetingLink { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;
    }
}