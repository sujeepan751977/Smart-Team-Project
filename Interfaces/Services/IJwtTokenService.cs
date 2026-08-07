using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
