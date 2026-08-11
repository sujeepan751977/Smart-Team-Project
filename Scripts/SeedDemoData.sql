-- =============================================================================
-- Smart Recruit - Demo Seed Script (SSMS / sqlcmd)
-- Database : RecruitmentProjectDb
-- Server   : (localdb)\MSSQLLocalDB
--
-- Demo accounts (Appendix B):
--   Administrator | admin.demo@smartrecruit.local     | Admin@12345
--   Employer      | employer.demo@smartrecruit.local  | Employer@123
--   Job Seeker    | jobseeker.demo@smartrecruit.local | JobSeeker@123
--
-- Seeds:
--   - 12 OPEN employer vacancies (+ 1 Draft + 1 Pending for variety)
--   - 6 job seeker applications:
--       2 Applied, 2 UnderReview, 2 Rejected
--   - Plus interview + contact request samples on one UnderReview app
--   - Skills, verification, notifications, contact request, interview sample
-- =============================================================================

USE RecruitmentProjectDb;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @expiry DATETIME2 = DATEADD(MONTH, 3, @now);

-- Password hashes generated with BCrypt (compatible with BCrypt.Net-Next)
DECLARE @AdminHash NVARCHAR(200) = N'$2b$11$t0pln.s4jS8pgSZ4aoAaJ.OmUgFb0iTmEPFvMrWp5IzYzYi0zfy5q'; -- Admin@12345
DECLARE @EmpHash   NVARCHAR(200) = N'$2b$11$5GM.VoLTr/dJ0z3w//YyouX7BhIPmTsaCH6oibBFF11vwteCgy/Zu'; -- Employer@123
DECLARE @JsHash    NVARCHAR(200) = N'$2b$11$A8cZjrGV4WF/8FrkuCp0r.lhlk66Yjc0HHn9tXqF1BztvgUBLtDIu'; -- JobSeeker@123

DECLARE @AdminEmail NVARCHAR(150) = N'admin.demo@smartrecruit.local';
DECLARE @EmpEmail   NVARCHAR(150) = N'employer.demo@smartrecruit.local';
DECLARE @JsEmail    NVARCHAR(150) = N'jobseeker.demo@smartrecruit.local';

------------------------------------------------------------
-- Cleanup previous demo seed only (safe for other data)
------------------------------------------------------------
DECLARE @AdminId INT, @EmpUserId INT, @JsUserId INT;
DECLARE @EmpProfileId INT, @JsProfileId INT;

SELECT @AdminId = Id FROM Users WHERE Email = @AdminEmail;
SELECT @EmpUserId = Id FROM Users WHERE Email = @EmpEmail;
SELECT @JsUserId = Id FROM Users WHERE Email = @JsEmail;
SELECT @EmpProfileId = Id FROM EmployerProfiles WHERE UserId = @EmpUserId;
SELECT @JsProfileId = Id FROM JobSeekerProfiles WHERE UserId = @JsUserId;

IF @JsProfileId IS NOT NULL
BEGIN
    DELETE FROM InterviewSchedules
    WHERE JobApplicationId IN (SELECT Id FROM JobApplications WHERE JobSeekerProfileId = @JsProfileId);

    DELETE FROM ContactRequests
    WHERE JobSeekerProfileId = @JsProfileId
       OR JobApplicationId IN (SELECT Id FROM JobApplications WHERE JobSeekerProfileId = @JsProfileId);

    DELETE FROM JobApplications WHERE JobSeekerProfileId = @JsProfileId;
    DELETE FROM JobSeekerSkills WHERE JobSeekerProfileId = @JsProfileId;
    DELETE FROM CvDocuments WHERE JobSeekerProfileId = @JsProfileId;
END

IF @EmpProfileId IS NOT NULL
BEGIN
    DELETE FROM JobReports
    WHERE VacancyId IN (SELECT Id FROM Vacancies WHERE EmployerProfileId = @EmpProfileId);

    DELETE FROM VacancySkills
    WHERE VacancyId IN (SELECT Id FROM Vacancies WHERE EmployerProfileId = @EmpProfileId);

    DELETE FROM JobApplications
    WHERE VacancyId IN (SELECT Id FROM Vacancies WHERE EmployerProfileId = @EmpProfileId);

    DELETE FROM Vacancies WHERE EmployerProfileId = @EmpProfileId;

    DELETE FROM EmployerVerificationDocuments
    WHERE EmployerVerificationId IN (
        SELECT Id FROM EmployerVerifications WHERE EmployerProfileId = @EmpProfileId);

    DELETE FROM EmployerVerifications WHERE EmployerProfileId = @EmpProfileId;
    DELETE FROM EmployerProfiles WHERE Id = @EmpProfileId;
END

IF @JsProfileId IS NOT NULL
    DELETE FROM JobSeekerProfiles WHERE Id = @JsProfileId;

IF @AdminId IS NOT NULL
BEGIN
    DELETE FROM Notifications WHERE UserId = @AdminId;
    DELETE FROM ModerationAuditLogs WHERE AdminUserId = @AdminId;
    DELETE FROM Users WHERE Id = @AdminId;
END

IF @EmpUserId IS NOT NULL
BEGIN
    DELETE FROM Notifications WHERE UserId = @EmpUserId;
    DELETE FROM Users WHERE Id = @EmpUserId;
END

IF @JsUserId IS NOT NULL
BEGIN
    DELETE FROM Notifications WHERE UserId = @JsUserId;
    DELETE FROM JobReports WHERE ReportedByUserId = @JsUserId;
    DELETE FROM Users WHERE Id = @JsUserId;
END

------------------------------------------------------------
-- Users
-- Role: Administrator=1, Employer=2, JobSeeker=3
------------------------------------------------------------
INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
VALUES (N'Demo Administrator', @AdminEmail, @AdminHash, 1, 1, @now, NULL);
SET @AdminId = SCOPE_IDENTITY();

INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
VALUES (N'Demo Employer HR', @EmpEmail, @EmpHash, 2, 1, @now, NULL);
SET @EmpUserId = SCOPE_IDENTITY();

INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
VALUES (N'Demo Job Seeker', @JsEmail, @JsHash, 3, 1, @now, NULL);
SET @JsUserId = SCOPE_IDENTITY();

------------------------------------------------------------
-- Employer profile + pending verification (admin can Verify)
-- AccountStatus Active=1 | Verification PendingReview=2
------------------------------------------------------------
INSERT INTO EmployerProfiles (
    UserId, CompanyName, RegisteredCompanyName, RegistrationNumber, Industry,
    RegisteredAddress, OperatingLocation, OfficialCompanyEmail, CompanyPhone,
    Website, AuthorizedRepresentative, CompanyDescription, CreatedAt, UpdatedAt, AccountStatus)
VALUES (
    @EmpUserId,
    N'SmartHire Demo Solutions',
    N'SmartHire Demo Solutions (Pvt) Ltd',
    N'REG-DEMO-2026',
    N'Information Technology',
    N'42 Independence Avenue, Colombo 07',
    N'Colombo',
    @EmpEmail,
    N'+94112223344',
    N'https://smarthire-demo.example',
    N'Demo Employer HR',
    N'Demo employer company used for Smart Recruit project testing and presentation.',
    @now, NULL, 1);
SET @EmpProfileId = SCOPE_IDENTITY();

DECLARE @EmpVerificationId INT;
INSERT INTO EmployerVerifications (
    EmployerProfileId, Status, AdministratorFeedback, SubmittedAt, ReviewedAt, CreatedAt, UpdatedAt)
VALUES (
    @EmpProfileId, 2, NULL, @now, NULL, @now, @now);
SET @EmpVerificationId = SCOPE_IDENTITY();

-- DocumentStatus Pending=1
INSERT INTO EmployerVerificationDocuments (
    EmployerVerificationId, FileName, FilePath, ContentType, Status, AdministratorComment, UploadedAt)
VALUES (
    @EmpVerificationId,
    N'business-registration-demo.pdf',
    N'Storage/EmployerVerificationDocuments/demo-registration.pdf',
    N'application/pdf',
    1,
    NULL,
    @now);

------------------------------------------------------------
-- Job seeker profile + skills
------------------------------------------------------------
INSERT INTO JobSeekerProfiles (
    UserId, ProfessionalTitle, Location, ExperienceInYears, Education, About,
    ProfileCompletionPercentage, CreatedAt, UpdatedAt)
VALUES (
    @JsUserId,
    N'Full Stack Developer',
    N'Colombo',
    3,
    N'BSc Computer Science',
    N'Demo job seeker with C#, JavaScript, SQL and cloud skills for Smart Recruit demos.',
    100,
    @now, NULL);
SET @JsProfileId = SCOPE_IDENTITY();

-- Skills (insert if missing)
DECLARE @SkillNames TABLE (Name NVARCHAR(100));
INSERT INTO @SkillNames(Name) VALUES
 (N'C#'),(N'ASP.NET Core'),(N'JavaScript'),(N'SQL'),(N'React'),
 (N'Azure'),(N'Entity Framework'),(N'HTML'),(N'CSS'),(N'Git'),
 (N'Docker'),(N'Python');

INSERT INTO Skills (Name, CreatedAt)
SELECT s.Name, @now
FROM @SkillNames s
WHERE NOT EXISTS (SELECT 1 FROM Skills x WHERE x.Name = s.Name);

INSERT INTO JobSeekerSkills (JobSeekerProfileId, SkillId, CreatedAt)
SELECT @JsProfileId, sk.Id, @now
FROM Skills sk
WHERE sk.Name IN (N'C#', N'ASP.NET Core', N'JavaScript', N'SQL', N'HTML', N'CSS', N'Git')
  AND NOT EXISTS (
      SELECT 1 FROM JobSeekerSkills js
      WHERE js.JobSeekerProfileId = @JsProfileId AND js.SkillId = sk.Id);

------------------------------------------------------------
-- Demo CV (uses existing sample PDF in CVStorage if present)
------------------------------------------------------------
INSERT INTO CvDocuments (
    JobSeekerProfileId, FileName, FilePath, ContentType, FileSize, UploadedAt)
VALUES (
    @JsProfileId,
    N'demo-jobseeker-cv.pdf',
    N'D:\final-project-submission\Smart-Team-Project-Test\CVStorage\a4a1626d-4397-43e6-8efd-898cef65b15f.pdf',
    N'application/pdf',
    102400,
    @now);

------------------------------------------------------------
-- Helper: insert vacancy and capture id
-- VacancyStatus: Draft=1, PendingApproval=2, Open=3, Rejected=4, Closed=5
------------------------------------------------------------
DECLARE @Vac TABLE (
    Seq INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200),
    Category NVARCHAR(100),
    EmploymentType NVARCHAR(100),
    WorkLocation NVARCHAR(100),
    ExperienceLevel NVARCHAR(100),
    SalaryRange NVARCHAR(100),
    Description NVARCHAR(4000),
    Requirements NVARCHAR(2000),
    Status INT,
    VacancyId INT NULL
);

INSERT INTO @Vac (Title, Category, EmploymentType, WorkLocation, ExperienceLevel, SalaryRange, Description, Requirements, Status)
VALUES
(N'Software Engineer', N'IT', N'Full-time', N'Colombo', N'2', N'120000-180000',
 N'Build and maintain ASP.NET Core services for Smart Recruit demo hiring.', N'C#, SQL, Git', 3),
(N'Frontend Developer', N'IT', N'Full-time', N'Colombo', N'2', N'110000-160000',
 N'Create responsive UI pages with HTML, CSS and JavaScript.', N'JavaScript, HTML, CSS', 3),
(N'Backend Developer', N'IT', N'Full-time', N'Colombo', N'3', N'140000-200000',
 N'Design REST APIs, authentication and data access layers.', N'ASP.NET Core, EF Core, SQL', 3),
(N'QA Engineer', N'IT', N'Full-time', N'Kandy', N'1', N'90000-130000',
 N'Manual and automated testing for recruitment workflows.', N'SQL, Git, Testing basics', 3),
(N'DevOps Engineer', N'IT', N'Full-time', N'Remote', N'3', N'160000-220000',
 N'CI/CD pipelines, containers and cloud hosting support.', N'Azure, Docker, Git', 3),
(N'Data Analyst', N'Analytics', N'Full-time', N'Colombo', N'2', N'100000-150000',
 N'Analyze hiring funnels and prepare dashboards.', N'SQL, Python, Excel', 3),
(N'UI/UX Designer', N'Design', N'Contract', N'Colombo', N'2', N'100000-140000',
 N'Design clean recruitment portal experiences.', N'Figma, HTML, CSS', 3),
(N'Mobile App Developer', N'IT', N'Full-time', N'Galle', N'2', N'120000-170000',
 N'Build mobile companion features for job seekers.', N'JavaScript, API integration', 3),
(N'SQL Database Developer', N'IT', N'Full-time', N'Colombo', N'3', N'130000-190000',
 N'Design schemas, indexes and stored procedures.', N'SQL, EF Core', 3),
(N'IT Support Officer', N'Support', N'Full-time', N'Colombo', N'1', N'70000-100000',
 N'First-line support for employer and job seeker users.', N'Communication, basic IT', 3),
(N'Cloud Engineer', N'IT', N'Full-time', N'Remote', N'4', N'180000-250000',
 N'Manage Azure resources and secure deployments.', N'Azure, Docker, Git', 3),
(N'Business Analyst', N'Business', N'Full-time', N'Colombo', N'2', N'110000-160000',
 N'Gather requirements for recruitment modules and write specs.', N'Documentation, SQL basics', 3),
-- variety extras
(N'Intern Software Developer (Draft)', N'IT', N'Internship', N'Colombo', N'0', N'30000-40000',
 N'Draft vacancy for intern role - not published.', N'HTML, CSS basics', 1),
(N'HR Coordinator (Pending Approval)', N'HR', N'Full-time', N'Colombo', N'1', N'80000-110000',
 N'Pending admin approval demo vacancy.', N'Communication, MS Office', 2);

DECLARE @i INT = 1, @max INT;
SELECT @max = MAX(Seq) FROM @Vac;

WHILE @i <= @max
BEGIN
    DECLARE @Title NVARCHAR(200), @Cat NVARCHAR(100), @EmpType NVARCHAR(100), @Loc NVARCHAR(100),
            @Exp NVARCHAR(100), @Sal NVARCHAR(100), @Desc NVARCHAR(4000), @Req NVARCHAR(2000), @St INT, @NewVacId INT;

    SELECT @Title=Title, @Cat=Category, @EmpType=EmploymentType, @Loc=WorkLocation,
           @Exp=ExperienceLevel, @Sal=SalaryRange, @Desc=Description, @Req=Requirements, @St=Status
    FROM @Vac WHERE Seq = @i;

    INSERT INTO Vacancies (
        EmployerProfileId, Title, Category, EmploymentType, WorkLocation, ExperienceLevel,
        SalaryRange, Description, Requirements, ExpiryDate, Status, AdministratorRejectionReason,
        CreatedAt, UpdatedAt)
    VALUES (
        @EmpProfileId, @Title, @Cat, @EmpType, @Loc, @Exp,
        @Sal, @Desc, @Req, @expiry, @St, NULL, @now, NULL);

    SET @NewVacId = SCOPE_IDENTITY();
    UPDATE @Vac SET VacancyId = @NewVacId WHERE Seq = @i;

    // Attach skills for open jobs (prefer skills the demo seeker has)
    IF @St = 3
    BEGIN
        INSERT INTO VacancySkills (VacancyId, SkillId, CreatedAt)
        SELECT TOP (3) @NewVacId, sk.Id, @now
        FROM Skills sk
        WHERE sk.Name IN (N'C#', N'ASP.NET Core', N'JavaScript', N'SQL', N'HTML', N'CSS', N'Git', N'Azure', N'Docker', N'Python')
          AND NOT EXISTS (
              SELECT 1 FROM VacancySkills vs WHERE vs.VacancyId = @NewVacId AND vs.SkillId = sk.Id)
        ORDER BY CASE sk.Name
            WHEN N'C#' THEN 1
            WHEN N'ASP.NET Core' THEN 2
            WHEN N'JavaScript' THEN 3
            WHEN N'SQL' THEN 4
            WHEN N'HTML' THEN 5
            WHEN N'CSS' THEN 6
            WHEN N'Git' THEN 7
            ELSE 99
        END, sk.Id;
    END

    SET @i += 1;
END

------------------------------------------------------------
-- 6 Applications for first 6 OPEN vacancies
-- ApplicationStatus: Applied=1, UnderReview=2, Shortlisted=3, Rejected=4
-- Mix: 2 Applied, 2 UnderReview, 2 Rejected
------------------------------------------------------------
DECLARE @OpenVacIds TABLE (RowNum INT IDENTITY(1,1), VacancyId INT);
INSERT INTO @OpenVacIds (VacancyId)
SELECT TOP (6) VacancyId FROM @Vac WHERE Status = 3 ORDER BY Seq;

DECLARE @AppStatuses TABLE (RowNum INT, Status INT, Score DECIMAL(18,2), Letter NVARCHAR(1000));
INSERT INTO @AppStatuses VALUES
 (1, 1, 78.00, N'I am excited to apply for Software Engineer. Demo application (Applied).'),
 (2, 1, 74.00, N'Frontend developer application. Demo application (Applied).'),
 (3, 2, 81.00, N'Strong backend experience with ASP.NET Core. Demo application (UnderReview).'),
 (4, 2, 72.00, N'Please review my QA skills. Demo application (UnderReview).'),
 (5, 4, 55.00, N'Applied for DevOps role. Demo application (Rejected).'),
 (6, 4, 48.00, N'Applied for Data Analyst role. Demo application (Rejected).');

INSERT INTO JobApplications (VacancyId, JobSeekerProfileId, Status, MatchScore, CoverLetter, AppliedAt, UpdatedAt)
SELECT v.VacancyId, @JsProfileId, a.Status, a.Score, a.Letter, DATEADD(DAY, -a.RowNum, @now), @now
FROM @OpenVacIds v
JOIN @AppStatuses a ON a.RowNum = v.RowNum;

------------------------------------------------------------
-- Extra variety: contact request + interview for one UnderReview app
------------------------------------------------------------
DECLARE @ReviewAppId INT;
SELECT TOP 1 @ReviewAppId = Id FROM JobApplications
WHERE JobSeekerProfileId = @JsProfileId AND Status = 2 ORDER BY Id;

IF @ReviewAppId IS NOT NULL
BEGIN
    INSERT INTO ContactRequests (
        JobApplicationId, EmployerProfileId, JobSeekerProfileId,
        Status, EmployerMessage, JobSeekerResponse, RequestedAt, RespondedAt)
    VALUES (
        @ReviewAppId, @EmpProfileId, @JsProfileId,
        1,
        N'Demo contact request: we would like to discuss this Under Review application.',
        NULL,
        @now, NULL);

    INSERT INTO InterviewSchedules (
        JobApplicationId, Title, InterviewDate, DurationMinutes, Location, MeetingLink, Instructions, Status, CreatedAt)
    VALUES (
        @ReviewAppId,
        N'Technical Interview - Demo',
        DATEADD(DAY, 5, @now),
        45,
        N'Microsoft Teams',
        N'https://teams.microsoft.com/l/meetup-join/demo',
        N'Please join 5 minutes early. Bring ID.',
        1,
        @now);
END

------------------------------------------------------------
-- Notifications for all demo users
------------------------------------------------------------
INSERT INTO Notifications (UserId, Title, Message, Type, IsRead, CreatedAt)
VALUES
(@JsUserId, N'Welcome Job Seeker', N'Your demo job seeker account is ready. 6 applications were seeded.', 1, 0, @now),
(@JsUserId, N'Application Update', N'Two applications are Under Review and one is Shortlisted.', 1, 0, @now),
(@EmpUserId, N'Welcome Employer', N'Your demo employer account has 12 open jobs seeded.', 1, 0, @now),
(@EmpUserId, N'New Applications', N'Demo job seeker applied to 6 of your vacancies.', 1, 0, @now),
(@AdminId, N'Admin Demo Ready', N'Demo admin account is ready for moderation and approvals.', 1, 0, @now);

COMMIT TRANSACTION;

------------------------------------------------------------
-- Verification summary
------------------------------------------------------------
PRINT '==== DEMO SEED COMPLETE ====';
SELECT 'Users' AS [Table], Email, Role, IsActive FROM Users
WHERE Email IN (@AdminEmail, @EmpEmail, @JsEmail);

SELECT 'VacanciesByStatus' AS Info, Status, COUNT(*) AS Cnt
FROM Vacancies WHERE EmployerProfileId = @EmpProfileId
GROUP BY Status;

SELECT 'ApplicationsByStatus' AS Info, Status, COUNT(*) AS Cnt
FROM JobApplications WHERE JobSeekerProfileId = @JsProfileId
GROUP BY Status;

SELECT a.Id, v.Title, a.Status, a.MatchScore
FROM JobApplications a
JOIN Vacancies v ON v.Id = a.VacancyId
WHERE a.JobSeekerProfileId = @JsProfileId
ORDER BY a.Id;

PRINT 'Login:';
PRINT 'Admin      admin.demo@smartrecruit.local / Admin@12345';
PRINT 'Employer   employer.demo@smartrecruit.local / Employer@123';
PRINT 'JobSeeker  jobseeker.demo@smartrecruit.local / JobSeeker@123';
GO
