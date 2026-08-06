using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class JobSeekerProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [MaxLength(100)]
        public string ProfessionalTitle { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        public int ExperienceInYears { get; set; }

        [MaxLength(200)]
        public string Education { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string About { get; set; } = string.Empty;

        public int ProfileCompletionPercentage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;

        public ICollection<JobSeekerSkill> Skills { get; set; } = new List<JobSeekerSkill>();

        public ICollection<CvDocument> CvDocuments { get; set; } = new List<CvDocument>();

    }
}
