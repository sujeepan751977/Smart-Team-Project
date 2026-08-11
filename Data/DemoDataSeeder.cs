using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Models.Enums;

namespace Recruitment_Project.Data
{
    /// <summary>
    /// Seeds demo users and mock data for local development / presentation.
    /// Controlled by configuration: DemoSeed:Enabled / DemoSeed:Reseed
    /// </summary>
    public static class DemoDataSeeder
    {
        public const string AdminEmail = "admin.demo@smartrecruit.local";
        public const string EmployerEmail = "employer.demo@smartrecruit.local";
        public const string JobSeekerEmail = "jobseeker.demo@smartrecruit.local";

        public const string AdminPassword = "Admin@12345";
        public const string EmployerPassword = "Employer@123";
        public const string JobSeekerPassword = "JobSeeker@123";

        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            var enabled = config.GetValue("DemoSeed:Enabled", true);
            if (!enabled)
                return;

            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("DemoDataSeeder");

            await db.Database.MigrateAsync();

            var reseed = config.GetValue("DemoSeed:Reseed", false);
            var adminExists = await db.Users.AnyAsync(x => x.Email == AdminEmail);

            if (adminExists && !reseed)
            {
                logger.LogInformation("Demo seed skipped (already present). Set DemoSeed:Reseed=true to refresh.");
                return;
            }

            if (adminExists && reseed)
            {
                logger.LogInformation("Reseeding demo data…");
                await CleanupDemoAsync(db);
            }

            logger.LogInformation("Seeding demo mock data…");
            await SeedCoreAsync(db, env.ContentRootPath);
            logger.LogInformation(
                "Demo seed complete. Logins — Admin: {Admin}/{AdminPw} | Employer: {Emp}/{EmpPw} | JobSeeker: {Js}/{JsPw}",
                AdminEmail, AdminPassword,
                EmployerEmail, EmployerPassword,
                JobSeekerEmail, JobSeekerPassword);
        }

        private static async Task CleanupDemoAsync(AppDbContext db)
        {
            var emails = new[] { AdminEmail, EmployerEmail, JobSeekerEmail };
            var users = await db.Users.Where(x => emails.Contains(x.Email)).ToListAsync();
            var userIds = users.Select(x => x.Id).ToList();

            var empProfileIds = await db.EmployerProfiles
                .Where(x => userIds.Contains(x.UserId))
                .Select(x => x.Id)
                .ToListAsync();

            var jsProfileIds = await db.JobSeekerProfiles
                .Where(x => userIds.Contains(x.UserId))
                .Select(x => x.Id)
                .ToListAsync();

            var vacancyIds = await db.Vacancies
                .Where(x => empProfileIds.Contains(x.EmployerProfileId))
                .Select(x => x.Id)
                .ToListAsync();

            var appIds = await db.JobApplications
                .Where(x => jsProfileIds.Contains(x.JobSeekerProfileId) || vacancyIds.Contains(x.VacancyId))
                .Select(x => x.Id)
                .ToListAsync();

            db.InterviewSchedules.RemoveRange(
                db.InterviewSchedules.Where(x => appIds.Contains(x.JobApplicationId)));
            db.ContactRequests.RemoveRange(
                db.ContactRequests.Where(x =>
                    appIds.Contains(x.JobApplicationId) ||
                    jsProfileIds.Contains(x.JobSeekerProfileId) ||
                    empProfileIds.Contains(x.EmployerProfileId)));
            db.JobApplications.RemoveRange(
                db.JobApplications.Where(x => appIds.Contains(x.Id)));
            db.JobReports.RemoveRange(
                db.JobReports.Where(x =>
                    vacancyIds.Contains(x.VacancyId) || userIds.Contains(x.ReportedByUserId)));
            db.VacancySkills.RemoveRange(
                db.VacancySkills.Where(x => vacancyIds.Contains(x.VacancyId)));
            db.Vacancies.RemoveRange(
                db.Vacancies.Where(x => vacancyIds.Contains(x.Id)));

            var verIds = await db.EmployerVerifications
                .Where(x => empProfileIds.Contains(x.EmployerProfileId))
                .Select(x => x.Id)
                .ToListAsync();
            db.EmployerVerificationDocuments.RemoveRange(
                db.EmployerVerificationDocuments.Where(x => verIds.Contains(x.EmployerVerificationId)));
            db.EmployerVerifications.RemoveRange(
                db.EmployerVerifications.Where(x => verIds.Contains(x.Id)));

            db.CvDocuments.RemoveRange(
                db.CvDocuments.Where(x => jsProfileIds.Contains(x.JobSeekerProfileId)));
            db.JobSeekerSkills.RemoveRange(
                db.JobSeekerSkills.Where(x => jsProfileIds.Contains(x.JobSeekerProfileId)));
            db.JobSeekerProfiles.RemoveRange(
                db.JobSeekerProfiles.Where(x => jsProfileIds.Contains(x.Id)));
            db.EmployerProfiles.RemoveRange(
                db.EmployerProfiles.Where(x => empProfileIds.Contains(x.Id)));
            db.Notifications.RemoveRange(
                db.Notifications.Where(x => userIds.Contains(x.UserId)));
            db.ModerationAuditLogs.RemoveRange(
                db.ModerationAuditLogs.Where(x => userIds.Contains(x.AdminUserId)));
            db.Users.RemoveRange(users);

            await db.SaveChangesAsync();
        }

        private static async Task SeedCoreAsync(AppDbContext db, string contentRoot)
        {
            var now = DateTime.UtcNow;
            var expiry = now.AddMonths(3);

            var admin = new User
            {
                FullName = "Demo Administrator",
                Email = AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
                Role = UserRole.Administrator,
                IsActive = true,
                CreatedAt = now
            };
            var employerUser = new User
            {
                FullName = "Demo Employer HR",
                Email = EmployerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(EmployerPassword),
                Role = UserRole.Employer,
                IsActive = true,
                CreatedAt = now
            };
            var seekerUser = new User
            {
                FullName = "Demo Job Seeker",
                Email = JobSeekerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(JobSeekerPassword),
                Role = UserRole.JobSeeker,
                IsActive = true,
                CreatedAt = now
            };

            db.Users.AddRange(admin, employerUser, seekerUser);
            await db.SaveChangesAsync();

            var employer = new EmployerProfile
            {
                UserId = employerUser.Id,
                CompanyName = "SmartHire Demo Solutions",
                RegisteredCompanyName = "SmartHire Demo Solutions (Pvt) Ltd",
                RegistrationNumber = "REG-DEMO-2026",
                Industry = "Information Technology",
                RegisteredAddress = "42 Independence Avenue, Colombo 07",
                OperatingLocation = "Colombo",
                OfficialCompanyEmail = EmployerEmail,
                CompanyPhone = "+94112223344",
                Website = "https://smarthire-demo.example",
                AuthorizedRepresentative = "Demo Employer HR",
                CompanyDescription = "Demo employer company used for Smart Recruit project testing and presentation.",
                AccountStatus = EmployerAccountStatus.Active,
                CreatedAt = now
            };
            db.EmployerProfiles.Add(employer);
            await db.SaveChangesAsync();

            var verification = new EmployerVerification
            {
                EmployerProfileId = employer.Id,
                Status = EmployerVerificationStatus.PendingReview,
                SubmittedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.EmployerVerifications.Add(verification);
            await db.SaveChangesAsync();

            var storageDir = Path.Combine(contentRoot, "Storage", "EmployerVerificationDocuments");
            Directory.CreateDirectory(storageDir);
            var verDocFullPath = Path.Combine(storageDir, "demo-registration.pdf");
            EnsurePlaceholderPdf(verDocFullPath);

            db.EmployerVerificationDocuments.Add(new EmployerVerificationDocument
            {
                EmployerVerificationId = verification.Id,
                FileName = "business-registration-demo.pdf",
                FilePath = "Storage/EmployerVerificationDocuments/demo-registration.pdf",
                ContentType = "application/pdf",
                Status = VerificationDocumentStatus.Pending,
                UploadedAt = now
            });

            var seeker = new JobSeekerProfile
            {
                UserId = seekerUser.Id,
                ProfessionalTitle = "Full Stack Developer",
                Location = "Colombo",
                ExperienceInYears = 3,
                Education = "BSc Computer Science",
                About = "Demo job seeker with C#, JavaScript, SQL and cloud skills for Smart Recruit demos.",
                ProfileCompletionPercentage = 100,
                CreatedAt = now
            };
            db.JobSeekerProfiles.Add(seeker);
            await db.SaveChangesAsync();

            var skillNames = new[]
            {
                "C#", "ASP.NET Core", "JavaScript", "SQL", "React",
                "Azure", "Entity Framework", "HTML", "CSS", "Git",
                "Docker", "Python"
            };
            foreach (var name in skillNames)
            {
                if (!await db.Skills.AnyAsync(x => x.Name == name))
                {
                    db.Skills.Add(new Skill { Name = name, CreatedAt = now });
                }
            }
            await db.SaveChangesAsync();

            var seekerSkillNames = new[] { "C#", "ASP.NET Core", "JavaScript", "SQL", "HTML", "CSS", "Git" };
            var skills = await db.Skills.Where(x => seekerSkillNames.Contains(x.Name)).ToListAsync();
            foreach (var skill in skills)
            {
                db.JobSeekerSkills.Add(new JobSeekerSkill
                {
                    JobSeekerProfileId = seeker.Id,
                    SkillId = skill.Id,
                    CreatedAt = now
                });
            }

            var cvDir = Path.Combine(contentRoot, "CVStorage");
            Directory.CreateDirectory(cvDir);
            var existingCv = Directory.GetFiles(cvDir, "*.pdf").FirstOrDefault();
            var cvFullPath = existingCv ?? Path.Combine(cvDir, "demo-jobseeker-cv.pdf");
            if (existingCv == null)
                EnsurePlaceholderPdf(cvFullPath);

            db.CvDocuments.Add(new CvDocument
            {
                JobSeekerProfileId = seeker.Id,
                FileName = "demo-jobseeker-cv.pdf",
                FilePath = cvFullPath,
                ContentType = "application/pdf",
                FileSize = new FileInfo(cvFullPath).Exists ? new FileInfo(cvFullPath).Length : 1024,
                UploadedAt = now
            });

            var vacancyDefs = new (string Title, string Category, string Type, string Location, string Exp, string Salary, string Desc, string Req, VacancyStatus Status)[]
            {
                ("Software Engineer", "IT", "Full-time", "Colombo", "2", "120000-180000",
                    "Build and maintain ASP.NET Core services for Smart Recruit demo hiring.", "C#, SQL, Git", VacancyStatus.Open),
                ("Frontend Developer", "IT", "Full-time", "Colombo", "2", "110000-160000",
                    "Create responsive UI pages with HTML, CSS and JavaScript.", "JavaScript, HTML, CSS", VacancyStatus.Open),
                ("Backend Developer", "IT", "Full-time", "Colombo", "3", "140000-200000",
                    "Design REST APIs, authentication and data access layers.", "ASP.NET Core, EF Core, SQL", VacancyStatus.Open),
                ("QA Engineer", "IT", "Full-time", "Kandy", "1", "90000-130000",
                    "Manual and automated testing for recruitment workflows.", "SQL, Git, Testing basics", VacancyStatus.Open),
                ("DevOps Engineer", "IT", "Full-time", "Remote", "3", "160000-220000",
                    "CI/CD pipelines, containers and cloud hosting support.", "Azure, Docker, Git", VacancyStatus.Open),
                ("Data Analyst", "Analytics", "Full-time", "Colombo", "2", "100000-150000",
                    "Analyze hiring funnels and prepare dashboards.", "SQL, Python, Excel", VacancyStatus.Open),
                ("UI/UX Designer", "Design", "Contract", "Colombo", "2", "100000-140000",
                    "Design clean recruitment portal experiences.", "Figma, HTML, CSS", VacancyStatus.Open),
                ("Mobile App Developer", "IT", "Full-time", "Galle", "2", "120000-170000",
                    "Build mobile companion features for job seekers.", "JavaScript, API integration", VacancyStatus.Open),
                ("SQL Database Developer", "IT", "Full-time", "Colombo", "3", "130000-190000",
                    "Design schemas, indexes and stored procedures.", "SQL, EF Core", VacancyStatus.Open),
                ("IT Support Officer", "Support", "Full-time", "Colombo", "1", "70000-100000",
                    "First-line support for employer and job seeker users.", "Communication, basic IT", VacancyStatus.Open),
                ("Cloud Engineer", "IT", "Full-time", "Remote", "4", "180000-250000",
                    "Manage Azure resources and secure deployments.", "Azure, Docker, Git", VacancyStatus.Open),
                ("Business Analyst", "Business", "Full-time", "Colombo", "2", "110000-160000",
                    "Gather requirements for recruitment modules and write specs.", "Documentation, SQL basics", VacancyStatus.Open),
                ("Intern Software Developer (Draft)", "IT", "Internship", "Colombo", "0", "30000-40000",
                    "Draft vacancy for intern role - not published.", "HTML, CSS basics", VacancyStatus.Draft),
                ("HR Coordinator (Pending Approval)", "HR", "Full-time", "Colombo", "1", "80000-110000",
                    "Pending admin approval demo vacancy.", "Communication, MS Office", VacancyStatus.PendingApproval),
            };

            var preferredSkillOrder = new[]
            {
                "C#", "ASP.NET Core", "JavaScript", "SQL", "HTML", "CSS", "Git", "Azure", "Docker", "Python"
            };
            var allSkills = await db.Skills.ToListAsync();
            var openVacancies = new List<Vacancy>();

            foreach (var v in vacancyDefs)
            {
                var vacancy = new Vacancy
                {
                    EmployerProfileId = employer.Id,
                    Title = v.Title,
                    Category = v.Category,
                    EmploymentType = v.Type,
                    WorkLocation = v.Location,
                    ExperienceLevel = v.Exp,
                    SalaryRange = v.Salary,
                    Description = v.Desc,
                    Requirements = v.Req,
                    ExpiryDate = expiry,
                    Status = v.Status,
                    CreatedAt = now
                };
                db.Vacancies.Add(vacancy);
                await db.SaveChangesAsync();

                if (v.Status == VacancyStatus.Open)
                {
                    openVacancies.Add(vacancy);
                    var attach = allSkills
                        .Where(s => preferredSkillOrder.Contains(s.Name))
                        .OrderBy(s => Array.IndexOf(preferredSkillOrder, s.Name))
                        .Take(3)
                        .ToList();
                    foreach (var skill in attach)
                    {
                        db.VacancySkills.Add(new VacancySkill
                        {
                            VacancyId = vacancy.Id,
                            SkillId = skill.Id,
                            CreatedAt = now
                        });
                    }
                }
            }
            await db.SaveChangesAsync();

            var appStatuses = new (ApplicationStatus Status, decimal Score, string Letter)[]
            {
                (ApplicationStatus.Applied, 78m, "I am excited to apply for Software Engineer. Demo application (Applied)."),
                (ApplicationStatus.Applied, 74m, "Frontend developer application. Demo application (Applied)."),
                (ApplicationStatus.UnderReview, 81m, "Strong backend experience with ASP.NET Core. Demo application (UnderReview)."),
                (ApplicationStatus.UnderReview, 72m, "Please review my QA skills. Demo application (UnderReview)."),
                (ApplicationStatus.Rejected, 55m, "Applied for DevOps role. Demo application (Rejected)."),
                (ApplicationStatus.Rejected, 48m, "Applied for Data Analyst role. Demo application (Rejected)."),
            };

            JobApplication? reviewApp = null;
            for (var i = 0; i < Math.Min(6, openVacancies.Count); i++)
            {
                var app = new JobApplication
                {
                    VacancyId = openVacancies[i].Id,
                    JobSeekerProfileId = seeker.Id,
                    Status = appStatuses[i].Status,
                    MatchScore = appStatuses[i].Score,
                    CoverLetter = appStatuses[i].Letter,
                    AppliedAt = now.AddDays(-(i + 1)),
                    UpdatedAt = now
                };
                db.JobApplications.Add(app);
                if (appStatuses[i].Status == ApplicationStatus.UnderReview && reviewApp == null)
                    reviewApp = app;
            }
            await db.SaveChangesAsync();

            if (reviewApp != null)
            {
                // reload to get Id
                reviewApp = await db.JobApplications
                    .Where(x => x.JobSeekerProfileId == seeker.Id && x.Status == ApplicationStatus.UnderReview)
                    .OrderBy(x => x.Id)
                    .FirstAsync();

                db.ContactRequests.Add(new ContactRequest
                {
                    JobApplicationId = reviewApp.Id,
                    EmployerProfileId = employer.Id,
                    JobSeekerProfileId = seeker.Id,
                    Status = ContactRequestStatus.Pending,
                    EmployerMessage = "Demo contact request: we would like to discuss this Under Review application.",
                    RequestedAt = now
                });

                db.InterviewSchedules.Add(new InterviewSchedule
                {
                    JobApplicationId = reviewApp.Id,
                    Title = "Technical Interview - Demo",
                    InterviewDate = now.AddDays(5),
                    DurationMinutes = 45,
                    Location = "Microsoft Teams",
                    MeetingLink = "https://teams.microsoft.com/l/meetup-join/demo",
                    Instructions = "Please join 5 minutes early. Bring ID.",
                    Status = InterviewStatus.Scheduled,
                    CreatedAt = now
                });
            }

            db.Notifications.AddRange(
                new Notification
                {
                    UserId = seekerUser.Id,
                    Title = "Welcome Job Seeker",
                    Message = "Your demo job seeker account is ready. 6 applications were seeded.",
                    Type = NotificationType.System,
                    IsRead = false,
                    CreatedAt = now
                },
                new Notification
                {
                    UserId = seekerUser.Id,
                    Title = "Application Update",
                    Message = "Two applications are Under Review.",
                    Type = NotificationType.Application,
                    IsRead = false,
                    CreatedAt = now
                },
                new Notification
                {
                    UserId = employerUser.Id,
                    Title = "Welcome Employer",
                    Message = "Your demo employer account has open jobs seeded.",
                    Type = NotificationType.System,
                    IsRead = false,
                    CreatedAt = now
                },
                new Notification
                {
                    UserId = employerUser.Id,
                    Title = "New Applications",
                    Message = "Demo job seeker applied to 6 of your vacancies.",
                    Type = NotificationType.Application,
                    IsRead = false,
                    CreatedAt = now
                },
                new Notification
                {
                    UserId = admin.Id,
                    Title = "Admin Demo Ready",
                    Message = "Demo admin account is ready for moderation and employer verification review.",
                    Type = NotificationType.System,
                    IsRead = false,
                    CreatedAt = now
                });

            await db.SaveChangesAsync();
        }

        private static void EnsurePlaceholderPdf(string fullPath)
        {
            if (File.Exists(fullPath))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            var pdf = System.Text.Encoding.ASCII.GetBytes(
                "%PDF-1.1\n1 0 obj<<>>endobj\ntrailer<<>>\n%%EOF\n");
            File.WriteAllBytes(fullPath, pdf);
        }
    }
}
