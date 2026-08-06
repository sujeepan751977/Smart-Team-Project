using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class CvDocument
    {
        public int Id { get; set; }

        public int JobSeekerProfileId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}
