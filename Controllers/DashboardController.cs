using System;
using System.Collections.Generic;
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
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET /Dashboard/Index
        public async Task<IActionResult> Index(DateTime? demandFrom, DateTime? demandTo)
        {
            var today = DateTime.Today;

            // KPI Aggregates
            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var pendingOrders = await _context.PurchaseOrders
                .CountAsync(po => po.Status == "Pending");

            // Inventory value: sum(quantity * unitPrice)
            var totalValue = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.Product != null)
                .SumAsync(i => i.Quantity * i.Product!.UnitPrice);

            // Transactions today
            var txToday = await _context.StockTransactions
                .CountAsync(st => st.TransactionDate.Date == today);

            // Low Stock Items
            var lowStockItems = await _context.Inventories
                .Include(i => i.Product).ThenInclude(p => p!.Category)
                .Where(i => i.Product!.ReorderLevel.HasValue
                         && i.Quantity <= i.Product.ReorderLevel.Value)
                .Select(i => new LowStockItem
                {
                    ProductID = i.ProductID,
                    ProductName = i.Product!.ProductName,
                    CategoryName = i.Product.Category!.CategoryName,
                    Quantity = i.Quantity,
                    ReorderLevel = i.Product.ReorderLevel!.Value
                })
                .Take(10)
                .ToListAsync();

            // Recent Transactions
            var recentTx = await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.User)
                .OrderByDescending(st => st.TransactionDate)
                .Take(8)
                .Select(st => new RecentTransaction
                {
                    TransactionID = st.TransactionID,
                    ProductName = st.Product!.ProductName,
                    TransactionType = st.TransactionType,
                    Quantity = st.Quantity,
                    UserName = st.User!.FullName,
                    TransactionDate = st.TransactionDate
                })
                .ToListAsync();

            // Category Stock Summary (for doughnut chart)
            var categorySummary = await _context.Categories
                .Select(c => new CategoryStockSummary
                {
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count(),
                    TotalQuantity = c.Products
                        .Where(p => p.Inventory != null)
                        .Sum(p => p.Inventory!.Quantity)
                })
                .OrderByDescending(c => c.TotalQuantity)
                .ToListAsync();

            // Top 5 Most In-Demand Products nd date filter
            var demandQuery = _context.StockTransactions
                .Where(st => st.TransactionType == "Stock-Out"
                          || st.TransactionType == "Adjustment-Out");

            if (demandFrom.HasValue)
                demandQuery = demandQuery.Where(st => st.TransactionDate >= demandFrom.Value);

            if (demandTo.HasValue)
                demandQuery = demandQuery.Where(st => st.TransactionDate < demandTo.Value.AddDays(1));

            var topDemand = await demandQuery
                .GroupBy(st => st.Product!.ProductName)
                .Select(g => new TopDemandItem
                {
                    ProductName = g.Key,
                    StockOutCount = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.StockOutCount)
                .Take(5)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                LowStockCount = lowStockItems.Count,
                PendingOrdersCount = pendingOrders,
                TotalInventoryValue = totalValue,
                TotalTransactionsToday = txToday,
                LowStockItems = lowStockItems,
                RecentTransactions = recentTx,
                CategoryStockSummary = categorySummary,
                TopDemandItems = topDemand,
                DemandFilterFrom = demandFrom,
                DemandFilterTo = demandTo
            };

            return View(vm);
        }
    }
}
