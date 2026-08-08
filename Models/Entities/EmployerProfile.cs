using System.ComponentModel.DataAnnotations;

namespace Recruitment_Project.Models.Entities
{
    public class EmployerProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string RegisteredCompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Industry { get; set; } = string.Empty;

        [MaxLength(300)]
        public string RegisteredAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string OperatingLocation { get; set; } = string.Empty;

        [MaxLength(200)]
        public string OfficialCompanyEmail { get; set; } = string.Empty;

        [MaxLength(30)]
        public string CompanyPhone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Website { get; set; } = string.Empty;

        [MaxLength(150)]
        public string AuthorizedRepresentative { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string CompanyDescription { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public EmployerVerification? Verification { get; set; }

        public ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
    }
}
