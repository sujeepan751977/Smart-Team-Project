using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Repositories
{
    public interface IModerationAuditRepository
    {
        Task<IEnumerable<ModerationAuditLog>> GetAllAsync();
    }
}