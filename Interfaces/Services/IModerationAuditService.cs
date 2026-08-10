using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IModerationAuditService
    {
        Task<IEnumerable<ModerationAuditLog>> GetAuditLogsAsync();
    }
}