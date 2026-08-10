using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Middleware;
using Recruitment_Project.Models.Entities;
using Recruitment_Project.Options;
using Recruitment_Project.Repositories;
using Recruitment_Project.Services;

namespace Recruitment_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -------------------------
            // Database
            // -------------------------
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString(
                        "DefaultConnection")));

            // -------------------------
            // JWT Options
            // -------------------------
            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(
                    JwtOptions.SectionName));

            var jwtOptions = builder.Configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()!;

            // -------------------------
            // Authentication
            // -------------------------
            builder.Services
                .AddAuthentication(
                    JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = jwtOptions.Issuer,
                            ValidAudience = jwtOptions.Audience,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtOptions.SecretKey))
                        };
                });

            // -------------------------
            // Authorization
            // -------------------------
            builder.Services.AddAuthorization();

            // -------------------------
            // Dependency Injection
            // -------------------------

            // User
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // -------------------------
            // Member 1 - Job Seeker
            // -------------------------
            builder.Services.AddScoped<
                IJobSeekerRepository,
                JobSeekerRepository>();

            builder.Services.AddScoped<
                IJobSearchRepository,
                JobSearchRepository>();

            builder.Services.AddScoped<
                ICvRepository,
                CvRepository>();

            builder.Services.AddScoped<
                IJobSeekerService,
                JobSeekerService>();

            builder.Services.AddScoped<
                IMatchingService,
                MatchingService>();

            builder.Services.AddScoped<
                ICvFileStorageService,
                CvFileStorageService>();

            builder.Services.AddScoped<
                IJobSearchService,
                JobSearchService>();

            // -------------------------
            // Member 3 - Employer
            // -------------------------
            builder.Services.AddScoped<
                IEmployerRepository,
                EmployerRepository>();

            builder.Services.AddScoped<
                IEmployerVerificationRepository,
                EmployerVerificationRepository>();

            builder.Services.AddScoped<
                IVacancyRepository,
                VacancyRepository>();

            builder.Services.AddScoped<
                IEmployerService,
                EmployerService>();

            builder.Services.AddScoped<
                IEmployerVerificationService,
                EmployerVerificationService>();

            builder.Services.AddScoped<
                IEmployerVerificationAdminService,
                EmployerVerificationAdminService>();

            builder.Services.AddScoped<
                IVacancyService,
                VacancyService>();

            builder.Services.AddScoped<
                IVacancyApprovalService,
                VacancyApprovalService>();

            builder.Services.AddScoped<
                IVerificationDocumentStorageService,
                VerificationDocumentStorageService>();

            // -------------------------
            // Member 4 - Applications
            // -------------------------
            builder.Services.AddScoped<
                IApplicationRepository,
                ApplicationRepository>();

            builder.Services.AddScoped<
                IApplicationService,
                ApplicationService>();

	    builder.Services.AddScoped<
		IContactRequestRepository,
    		ContactRequestRepository>();

	    builder.Services.AddScoped<
   		 IContactRequestService,
   		 ContactRequestService>();

	    builder.Services.AddScoped<
    		IInterviewScheduleRepository,
    		InterviewScheduleRepository>();

	    builder.Services.AddScoped<
    		IInterviewScheduleService,
    		InterviewScheduleService>();

            // -------------------------
            // Member 5 - Job Reports
            // -------------------------
            builder.Services.AddScoped<
                IJobReportRepository,
                JobReportRepository>();

            builder.Services.AddScoped<
                IJobReportService,
                JobReportService>();

            builder.Services.AddScoped<
                IJobReportAdminService,
                JobReportAdminService>();

            // -------------------------
            // Notifications
            // -------------------------
            builder.Services.AddScoped<
                INotificationRepository,
                NotificationRepository>();

            builder.Services.AddScoped<
                INotificationService,
                NotificationService>();

            // -------------------------
            // Authentication / Admin
            // -------------------------
            builder.Services.AddScoped<
                IAuthService,
                AuthService>();

            builder.Services.AddScoped<
                IAdminUserService,
                AdminUserService>();

            builder.Services.AddScoped<
                IJwtTokenService,
                JwtTokenService>();

            // -------------------------
            // Controllers
            // -------------------------
            builder.Services.AddControllers();

            // -------------------------
            // Swagger
            // -------------------------
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Recruitment API",
                        Version = "v1"
                    });

                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            "Enter JWT Token only",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    });

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference =
                                    new Microsoft.OpenApi.Models.OpenApiReference
                                    {
                                        Type =
                                            ReferenceType.SecurityScheme,
                                            Id = "Bearer"
                                    }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

            var app = builder.Build();

            // -------------------------
            // Global Exception Middleware
            // -------------------------
            app.UseMiddleware<ExceptionMiddleware>();

            // -------------------------
            // Swagger
            // -------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // -------------------------
            // HTTPS
            // -------------------------
            app.UseHttpsRedirection();

            // -------------------------
            // Authentication
            // -------------------------
            app.UseAuthentication();

            // -------------------------
            // Active Account Middleware
            // -------------------------
            app.UseMiddleware<ActiveAccountMiddleware>();

            // -------------------------
            // Authorization
            // -------------------------
            app.UseAuthorization();

            // -------------------------
            // Controllers
            // -------------------------
            app.MapControllers();

            app.Run();
        }
    }
}