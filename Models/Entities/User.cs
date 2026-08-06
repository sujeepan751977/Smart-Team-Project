using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public JobSeekerProfile? JobSeekerProfile { get; set; }

        public EmployerProfile? EmployerProfile { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
