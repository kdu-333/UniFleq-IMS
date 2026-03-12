using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnifleqSolutions_IMS.Data;
using UnifleqSolutions_IMS.ViewModels;
namespace UnifleqSolutions_IMS.Controllers
{
    [Authorize(Roles = "Admin,Auditor")]
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _context;
        public AuditLogController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? userId, DateTime? from, DateTime? to, string? search)
        {
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();
            var query = _context.AuditLogs
                .Include(l => l.User)
                .AsQueryable();
            if (userId.HasValue)
                query = query.Where(l => l.UserID == userId.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(l => l.Action.Contains(search));
            if (from.HasValue)
                query = query.Where(l => l.LogDate >= from.Value);
            if (to.HasValue)
                query = query.Where(l => l.LogDate <= to.Value.AddDays(1));
            var logs = await query
                .OrderByDescending(l => l.LogDate)
                .Take(200)
                .ToListAsync();
            return View(new AuditLogViewModel
            {
                Logs = logs,
                Users = users,
                FilterUser = userId,
                DateFrom = from,
                DateTo = to,
                SearchAction = search
            });
        }
    }
}