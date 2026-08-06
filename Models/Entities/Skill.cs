using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class Skill
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<JobSeekerSkill> JobSeekerSkills { get; set; } = new List<JobSeekerSkill>();

        public ICollection<VacancySkill> VacancySkills { get; set; } = new List<VacancySkill>();
    }
}
