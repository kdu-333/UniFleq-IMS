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

namespace UnifleqSolutions_IMS.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _audit;

        public CategoryController(AppDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        private int CurrentUserID =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public async Task<IActionResult> Index()
        {
            var cats = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(new CategoryViewModel
            {
                Categories = cats,
                NewCategory = new Category()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "CreateCategory",
                $"Created category '{model.CategoryName}'");

            return Json(new
            {
                success = true,
                categoryID = model.CategoryID,
                categoryName = model.CategoryName,
                description = model.Description
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id, string categoryName, string? description)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            var oldName = cat.CategoryName;
            cat.CategoryName = categoryName;
            cat.Description = description;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "EditCategory",
                $"Edited category '{oldName}' → '{categoryName}'");

            return Json(new
            {
                success = true,
                categoryID = cat.CategoryID,
                categoryName = cat.CategoryName,
                description = cat.Description
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (cat == null) return NotFound();

            if (cat.Products.Any())
                return BadRequest("Cannot delete a category that has products assigned to it.");

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "DeleteCategory",
                $"Deleted category '{cat.CategoryName}'");

            return Json(new { success = true });
        }
    }
}