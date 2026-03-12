using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnifleqSolutions_IMS.Data;
using UnifleqSolutions_IMS.ViewModels;

namespace UnifleqSolutions_IMS.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        //GET /Inventory/Index
        public async Task<IActionResult> Index(string? search, int? categoryId, string? stockStatus)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var query = _context.Inventories
                .Include(i => i.Product).ThenInclude(p => p!.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(i =>
                    i.Product!.ProductName.Contains(search) ||
                    (i.Product.Brand != null && i.Product.Brand.Contains(search)));

            if (categoryId.HasValue)
                query = query.Where(i => i.Product!.CategoryID == categoryId.Value);

            var inventories = await query.ToListAsync();

            //map to view items and compute stock status
            var items = inventories.Select(i =>
            {
                string status = "OK";
                if (i.Quantity == 0)
                    status = "Out";
                else if (i.Product!.ReorderLevel.HasValue && i.Quantity <= i.Product.ReorderLevel.Value)
                    status = "Low";

                return new InventoryItem
                {
                    ProductID    = i.ProductID,
                    ProductName  = i.Product!.ProductName,
                    Brand        = i.Product.Brand ?? "",
                    CategoryName = i.Product.Category?.CategoryName ?? "",
                    Quantity     = i.Quantity,
                    ReorderLevel = i.Product.ReorderLevel ?? 0,
                    UnitPrice    = i.Product.UnitPrice,
                    TotalValue   = i.Quantity * i.Product.UnitPrice,
                    Status       = i.Product.Status,
                    StockStatus  = status,
                    LastUpdated  = i.LastUpdated
                };
            }).ToList();

            //apply stock status filter AFTER mapping
            if (!string.IsNullOrEmpty(stockStatus))
                items = items.Where(i => i.StockStatus == stockStatus).ToList();

            var vm = new InventoryViewModel
            {
                Items           = items,
                Categories      = categories,
                FilterCategory  = categoryId?.ToString(),
                FilterStatus    = stockStatus,
                SearchTerm      = search,
                TotalValue      = items.Sum(i => i.TotalValue),
                LowStockCount   = items.Count(i => i.StockStatus == "Low"),
                OutOfStockCount = items.Count(i => i.StockStatus == "Out")
            };

            return View(vm);
        }

        //GET /Inventory/Valuation
        [Authorize(Roles = "Admin,InventoryManager,Auditor")]
        public async Task<IActionResult> Valuation(int? categoryId)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName).ToListAsync();

            var query = _context.Inventories
                .Include(i => i.Product).ThenInclude(p => p!.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(i => i.Product!.CategoryID == categoryId.Value);

            var items = await query
                .Select(i => new ValuationItem
                {
                    ProductName  = i.Product!.ProductName,
                    CategoryName = i.Product.Category!.CategoryName,
                    Brand        = i.Product.Brand ?? "",
                    UnitPrice    = i.Product.UnitPrice,
                    Quantity     = i.Quantity,
                    TotalValue   = i.Quantity * i.Product.UnitPrice
                })
                .OrderByDescending(v => v.TotalValue)
                .ToListAsync();

            return View(new ValuationReportViewModel
            {
                Items            = items,
                TotalValue       = items.Sum(v => v.TotalValue),
                TotalUnits       = items.Sum(v => v.Quantity),
                Categories       = categories,
                FilterCategoryID = categoryId
            });
        }
    }
}
