using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Options;
using Recruitment_Project.Repositories;
using Recruitment_Project.Services;
using System.Text;
using Recruitment_Project.Middleware;


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
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // -------------------------
            // JWT Options
            // -------------------------
            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(JwtOptions.SectionName));

            var jwtOptions = builder.Configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()!;

            // -------------------------
            // Authentication
            // -------------------------
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                    };
                });

            // -------------------------
            // Authorization
            // -------------------------
            builder.Services.AddAuthorization();

            // -------------------------
            // Dependency Injection
            // -------------------------
            builder.Services.AddScoped<IUserRepository, UserRepository>();







            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAdminUserService, AdminUserService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Recruitment API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter JWT Token only",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

         
          

            // Global Exception Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            // -------------------------
            // Middleware
            // -------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseMiddleware<ActiveAccountMiddleware>();
            app.UseAuthorization();



            app.MapControllers();

            app.Run();

            
        }
    }
}