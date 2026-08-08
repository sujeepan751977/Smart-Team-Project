namespace Recruitment_Project.Models.Entities
{
    public class JobSeekerSkill
    {
        public int Id { get; set; }

        public int JobSeekerProfileId { get; set; }

        public int SkillId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}
