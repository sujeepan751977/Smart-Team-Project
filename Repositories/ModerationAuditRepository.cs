using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class ModerationAuditRepository : IModerationAuditRepository
    {
        private readonly AppDbContext _context;

        public ModerationAuditRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ModerationAuditLog>> GetAllAsync()
        {
            return await _context.ModerationAuditLogs
                .Include(x => x.AdminUser)
                .OrderByDescending(x => x.ActionDate)
                .ToListAsync();
        }
    }
}