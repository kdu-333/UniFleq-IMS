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
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _audit;

        public ProductController(AppDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        private int CurrentUserID =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // GET /Product/Index
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(new ProductViewModel
            {
                Products = products,
                Categories = categories,
                NewProduct = new Product()
            });
        }

        // POST /Product/Create UNIFLEW
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create([Bind(Prefix = "NewProduct")] Product model)
        {
            ModelState.Remove("NewProduct.Inventory");
            ModelState.Remove("NewProduct.Category");
            ModelState.Remove("NewProduct.StockTransactions");
            ModelState.Remove("NewProduct.PurchaseOrderDetails");
            ModelState.Remove("Inventory");
            ModelState.Remove("Category");
            ModelState.Remove("StockTransactions");
            ModelState.Remove("PurchaseOrderDetails");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(errors);
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == model.CategoryID);
            if (!categoryExists)
                return BadRequest("Invalid category.");

            model.Status = string.IsNullOrWhiteSpace(model.Status) ? "Available" : model.Status;

            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            _context.Inventories.Add(new Inventory
            {
                ProductID = model.ProductID,
                Quantity = 0
            });
            await _context.SaveChangesAsync();

            var categoryName = await _context.Categories
                .Where(c => c.CategoryID == model.CategoryID)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync();

            await _audit.LogAsync(CurrentUserID, "CreateProduct",
                $"Created product '{model.ProductName}' (ID: {model.ProductID})");

            return Json(new
            {
                success = true,
                productID = model.ProductID,
                productName = model.ProductName,
                brand = model.Brand,
                categoryName = categoryName,
                unitPrice = model.UnitPrice,
                reorderLevel = model.ReorderLevel,
                status = model.Status
            });
        }

        // GET /Product/Edit/{id}
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(new ProductEditViewModel
            {
                Product = product,
                Categories = categories
            });
        }

        // POST /Product/Edit
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id, string productName, string? brand,
            int categoryID, decimal unitPrice, int? reorderLevel, string status)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.ProductName = productName;
            existing.Brand = brand;
            existing.CategoryID = categoryID;
            existing.UnitPrice = unitPrice;
            existing.ReorderLevel = reorderLevel;
            existing.Status = status;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "EditProduct",
                $"Edited product '{productName}' (ID: {id})");

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Product/Archive - sets Status to Archived
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Archive(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var oldStatus = product.Status;
            product.Status = oldStatus == "Archived" ? "Available" : "Archived";

            await _context.SaveChangesAsync();

            var action = product.Status == "Archived" ? "ArchiveProduct" : "UnarchiveProduct";
            await _audit.LogAsync(CurrentUserID, action,
                $"{(product.Status == "Archived" ? "Archived" : "Unarchived")} product '{product.ProductName}' (ID: {id})");

            return Json(new { success = true, newStatus = product.Status });
        }

        // POST /Product/Delete (AJAX)
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .Include(p => p.StockTransactions)
                .Include(p => p.PurchaseOrderDetails)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            if (product.StockTransactions.Any())
                return BadRequest("Cannot delete — product has stock transaction history.");

            if (product.PurchaseOrderDetails.Any())
                return BadRequest("Cannot delete — product has purchase order history.");

            if (product.Inventory != null)
                _context.Inventories.Remove(product.Inventory);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "DeleteProduct",
                $"Deleted product '{product.ProductName}' (ID: {id})");

            return Json(new { success = true });
        }

        // GET /Product/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}