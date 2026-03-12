// Stock-In / Stock-Out / Manual Adjustment
using System;
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

//Roles allowed UNIFLEQ
{
    [Authorize(Roles = "Admin,InventoryManager,InventoryClerk")]
    public class StockAdjustmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _audit;
        private readonly EmailService _email;

        public StockAdjustmentController(AppDbContext context, IAuditService audit, EmailService email)
        {
            _context = context;
            _audit = audit;
            _email = email;
        }

        private int CurrentUserID =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // GET /StockAdjustment/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .Where(p => p.Status == "Available")
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var vm = new StockAdjustmentViewModel
            {
                Products = products,
                Categories = categories,
                NewProduct = new Product(),
                AdjustmentForm = new StockAdjustmentForm()
            };

            return View(vm);
        }

        // POST /StockAdjustment/Adjust (AJAX)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(StockAdjustmentForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Load inventory record 
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductID == form.ProductID);

            if (inventory == null)
                return BadRequest("Product inventory record not found.");

            // Apply adjustment
            if (form.TransactionType == "Stock-Out" || form.TransactionType == "Adjustment-Out")
            {
                if (inventory.Quantity < form.Quantity)
                    return BadRequest("Insufficient stock for this operation.");
                inventory.Quantity -= form.Quantity;
            }
            else // Stock-In or Adjustment-In
            {
                inventory.Quantity += form.Quantity;
            }

            inventory.LastUpdated = DateTime.Now;

            // Record the transaction 
            var transaction = new StockTransaction
            {
                ProductID = form.ProductID,
                UserID = CurrentUserID,
                TransactionType = form.TransactionType,
                Quantity = form.Quantity,
                Notes = form.Notes,
                TransactionDate = DateTime.Now
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Get product details 
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == form.ProductID);

            await _audit.LogAsync(CurrentUserID, "StockAdjustment",
                $"{form.TransactionType} of {form.Quantity} units for ProductID {form.ProductID}. Notes: {form.Notes}");

            if (product != null && inventory.Quantity <= (product.ReorderLevel ?? 0))
            {
                try
                {
                    await _email.SendLowStockAlertAsync(
                        product.ProductName,
                        product.Category?.CategoryName ?? "Unknown",
                        inventory.Quantity,
                        product.ReorderLevel ?? 0
                    );
                }
                catch { }
            }

            return Json(new
            {
                success = true,
                productName = product?.ProductName,
                transactionType = form.TransactionType,
                quantity = form.Quantity,
                newQuantity = inventory.Quantity,
                transactionID = transaction.TransactionID
            });
        }

        // GET /StockAdjustment/History
        [HttpGet]
        public async Task<IActionResult> History(string? type, int? productId, DateTime? from, DateTime? to)
        {
            var query = _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(st => st.TransactionType == type);
            if (productId.HasValue)
                query = query.Where(st => st.ProductID == productId.Value);
            if (from.HasValue)
                query = query.Where(st => st.TransactionDate >= from.Value);
            if (to.HasValue)
                query = query.Where(st => st.TransactionDate <= to.Value.AddDays(1));

            var transactions = await query
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();

            var products = await _context.Products
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(new StockTransactionViewModel
            {
                Transactions = transactions,
                Products = products,
                FilterType = type,
                FilterProduct = productId,
                DateFrom = from,
                DateTo = to
            });
        }
    }
}