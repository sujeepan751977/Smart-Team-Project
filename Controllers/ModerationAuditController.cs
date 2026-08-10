using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Project.Interfaces.Services;

namespace Recruitment_Project.Controllers
{
    [ApiController]
    [Route("api/admin/moderation")]
    [Authorize(Roles = "Administrator")]
    public class ModerationAuditController : ControllerBase
    {
        private readonly IModerationAuditService _auditService;

        public ModerationAuditController(
            IModerationAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("audit")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var auditLogs = await _auditService.GetAuditLogsAsync();

            return Ok(auditLogs);
        }
    }
}