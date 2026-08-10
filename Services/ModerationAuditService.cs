using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Services
{
    public class ModerationAuditService : IModerationAuditService
    {
        private readonly IModerationAuditRepository _auditRepository;

        public ModerationAuditService(
            IModerationAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<IEnumerable<ModerationAuditLog>> GetAuditLogsAsync()
        {
            return await _auditRepository.GetAllAsync();
        }
    }
}