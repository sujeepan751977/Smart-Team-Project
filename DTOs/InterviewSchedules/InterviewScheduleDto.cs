namespace Recruitment_Project.DTOs.InterviewSchedules
{
    public class InterviewScheduleDto
    {
        public int Id { get; set; }

        public int JobApplicationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime InterviewDate { get; set; }

        public int DurationMinutes { get; set; }

        public string Location { get; set; } = string.Empty;

        public string MeetingLink { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}