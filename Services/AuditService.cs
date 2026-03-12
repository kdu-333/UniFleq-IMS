using System;
using System.Threading.Tasks;
using UnifleqSolutions_IMS.Data;
using UnifleqSolutions_IMS.Models;

namespace UnifleqSolutions_IMS.Services
{
    public interface IAuditService
    {
        Task LogAsync(int userID, string action, string? details = null);
    }

    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }
        public async Task LogAsync(int userID, string action, string? details = null)
        {
            var log = new AuditLog
            {
                UserID  = userID,
                Action  = action,
                Details = details,
                LogDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Manila")
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
