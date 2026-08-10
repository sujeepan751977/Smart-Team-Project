using Recruitment_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class ModerationAuditLog
    {
        public int Id { get; set; }

        public int AdminUserId { get; set; }

        public ModerationActionType ActionType { get; set; }

        [Required]
        [MaxLength(100)]
        public string TargetEntity { get; set; } = string.Empty;

        public int TargetEntityId { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        [MaxLength(100)]
        public string? PreviousValue { get; set; }

        [MaxLength(100)]
        public string? NewValue { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public User AdminUser { get; set; } = null!;
    }
}