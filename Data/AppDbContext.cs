using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Models.Entities;
using System.Reflection.Emit;

namespace Recruitment_Project.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<JobSeekerSkill> JobSeekerSkills => Set<JobSeekerSkill>();
        public DbSet<CvDocument> CvDocuments => Set<CvDocument>();

        public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
        public DbSet<EmployerVerification> EmployerVerifications => Set<EmployerVerification>();
        public DbSet<EmployerVerificationDocument> EmployerVerificationDocuments => Set<EmployerVerificationDocument>();

        public DbSet<Vacancy> Vacancies => Set<Vacancy>();
        public DbSet<VacancySkill> VacancySkills => Set<VacancySkill>();

        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<InterviewSchedule> InterviewSchedules => Set<InterviewSchedule>();

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<JobReport> JobReports => Set<JobReport>();
        public DbSet<ModerationAuditLog> ModerationAuditLogs => Set<ModerationAuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(x => x.JobSeekerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<JobSeekerProfile>(x => x.UserId);

            modelBuilder.Entity<User>()
                .HasOne(x => x.EmployerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<EmployerProfile>(x => x.UserId);

            modelBuilder.Entity<JobSeekerSkill>()
                .HasOne(x => x.JobSeekerProfile)
                .WithMany(x => x.Skills)
                .HasForeignKey(x => x.JobSeekerProfileId);

            modelBuilder.Entity<JobSeekerSkill>()
                .HasOne(x => x.Skill)
                .WithMany(x => x.JobSeekerSkills)
                .HasForeignKey(x => x.SkillId);

            modelBuilder.Entity<CvDocument>()
                .HasOne(x => x.JobSeekerProfile)
                .WithMany(x => x.CvDocuments)
                .HasForeignKey(x => x.JobSeekerProfileId);

            modelBuilder.Entity<EmployerVerification>()
                .HasOne(x => x.EmployerProfile)
                .WithOne(x => x.Verification)
                .HasForeignKey<EmployerVerification>(x => x.EmployerProfileId);

            modelBuilder.Entity<EmployerVerificationDocument>()
                .HasOne(x => x.EmployerVerification)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.EmployerVerificationId);

            modelBuilder.Entity<Vacancy>()
                .HasOne(x => x.EmployerProfile)
                .WithMany(x => x.Vacancies)
                .HasForeignKey(x => x.EmployerProfileId);
            modelBuilder.Entity<VacancySkill>()
                .HasOne(x => x.Vacancy)
                .WithMany(x => x.RequiredSkills)
                .HasForeignKey(x => x.VacancyId);

            modelBuilder.Entity<VacancySkill>()
                .HasOne(x => x.Skill)
                .WithMany(x => x.VacancySkills)
                .HasForeignKey(x => x.SkillId);

            modelBuilder.Entity<JobApplication>()
                .Property(x => x.MatchScore)
                .HasPrecision(18, 2);

            modelBuilder.Entity<JobApplication>()
                .HasOne(x => x.Vacancy)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.VacancyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<JobApplication>()
                .HasOne(x => x.JobSeekerProfile)
                .WithMany()
                .HasForeignKey(x => x.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContactRequest>()
                .HasOne(x => x.JobApplication)
                .WithMany()
                .HasForeignKey(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContactRequest>()
                .HasOne(x => x.EmployerProfile)
                .WithMany()
                .HasForeignKey(x => x.EmployerProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContactRequest>()
                .HasOne(x => x.JobSeekerProfile)
                .WithMany()
                .HasForeignKey(x => x.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<InterviewSchedule>()
                .HasOne(x => x.JobApplication)
                .WithMany()
                .HasForeignKey(x => x.JobApplicationId);

            modelBuilder.Entity<Notification>()
                .HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<JobReport>()
                .HasOne(x => x.Vacancy)
                .WithMany()
                .HasForeignKey(x => x.VacancyId);

            modelBuilder.Entity<JobReport>()
                .HasOne(x => x.ReportedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModerationAuditLog>()
                .HasOne(x => x.AdminUser)
                .WithMany()
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
