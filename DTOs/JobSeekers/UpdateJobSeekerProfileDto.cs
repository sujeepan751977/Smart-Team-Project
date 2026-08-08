namespace Recruitment_Project.DTOs.JobSeekers
{
    public class UpdateJobSeekerProfileDto
    {
        public string ProfessionalTitle { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public int Experience { get; set; }

        public string Education { get; set; } = string.Empty;

        public string About { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();
    }
}
