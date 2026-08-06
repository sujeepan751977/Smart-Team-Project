using Recruitment_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}
