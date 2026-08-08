namespace Recruitment_Project.Models.Entities
{
    public class VacancySkill
    {
        public int Id { get; set; }

        public int VacancyId { get; set; }

        public int SkillId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Vacancy Vacancy { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}
