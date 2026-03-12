// Admin-only user management (CRUD)
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnifleqSolutions_IMS.Data;
using UnifleqSolutions_IMS.Models;
using UnifleqSolutions_IMS.Services;
using UnifleqSolutions_IMS.ViewModels;
using BCrypt.Net;

namespace UnifleqSolutions_IMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _audit;

        public UserController(AppDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        private int CurrentUserID =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // GET: /User/Index
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var roles = await _context.Roles.ToListAsync();

            return View(new UserViewModel
            {
                Users = users,
                Roles = roles
            });
        }

        // GET: /User/Create
        public async Task<IActionResult> Create()
        {
            return View(new UserCreateViewModel
            {
                Roles = await _context.Roles.ToListAsync()
            });
        }

        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await _context.Roles.ToListAsync();
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                model.Roles = await _context.Roles.ToListAsync();
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleID = model.RoleID,
                Status = model.Status
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "CreateUser",
                $"Created user '{model.Username}' with RoleID {model.RoleID}.");

            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(new UserEditViewModel
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Username = user.Username,
                RoleID = user.RoleID,
                Status = user.Status,
                Roles = await _context.Roles.ToListAsync()
            });
        }

        // POST: /User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (string.IsNullOrEmpty(model.NewPassword))
                ModelState.Remove("NewPassword");

            if (!ModelState.IsValid)
            {
                model.Roles = await _context.Roles.ToListAsync();
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserID);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Username = model.Username;
            user.RoleID = model.RoleID;
            user.Status = model.Status;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "EditUser",
                $"Edited user '{model.Username}' (ID: {model.UserID}).");

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /User/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.UserID == CurrentUserID)
                return BadRequest("You cannot deactivate your own account.");

            user.Status = user.Status == "Active" ? "Inactive" : "Active";
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "ToggleUserStatus",
                $"Set user '{user.Username}' status to '{user.Status}'.");

            return Json(new { success = true, newStatus = user.Status });
        }
    }
}