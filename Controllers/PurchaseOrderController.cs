//NOT YET WORKING
// Create / View / Receive purchase orders

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
{
    [Authorize(Roles = "Admin,ProcurementStaff,InventoryManager")]
    public class PurchaseOrderController : Controller
    {
        private readonly AppDbContext  _context;
        private readonly IAuditService _audit;

        public PurchaseOrderController(AppDbContext context, IAuditService audit)
        {
            _context = context;
            _audit   = audit;
        }

        private int CurrentUserID =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        //GET /PurchaseOrder/Index
        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.User)
                .Include(po => po.PurchaseOrderDetails)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(po => po.Status == status);

            var orders = await query
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            var suppliers = await _context.Suppliers
                .OrderBy(s => s.SupplierName).ToListAsync();

            return View(new PurchaseOrderViewModel
            {
                Orders       = orders,
                Suppliers    = suppliers,
                FilterStatus = status
            });
        }

        //GET /PurchaseOrder/Create
        public async Task<IActionResult> Create()
        {
            return View(new PurchaseOrderCreateViewModel
            {
                Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync(),
                Products  = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Status == "Available")
                    .OrderBy(p => p.ProductName).ToListAsync()
            });
        }

        //POST /PurchaseOrder/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderCreateViewModel model)
        {
            //filter out empty line items
            model.Items = model.Items
                .Where(i => i.ProductID > 0 && i.Quantity > 0 && i.UnitCost > 0)
                .ToList();

            if (!model.Items.Any())
                ModelState.AddModelError("", "At least one product line item is required.");

            if (!ModelState.IsValid)
            {
                model.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
                model.Products  = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Status == "Available")
                    .OrderBy(p => p.ProductName).ToListAsync();
                return View(model);
            }

            var po = new PurchaseOrder
            {
                SupplierID = model.SupplierID,
                UserID     = CurrentUserID,
                OrderDate  = DateTime.Now,
                Status     = "Pending"
            };

            _context.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            foreach (var item in model.Items)
            {
                _context.PurchaseOrderDetails.Add(new PurchaseOrderDetail
                {
                    POID      = po.POID,
                    ProductID = item.ProductID,
                    Quantity  = item.Quantity,
                    UnitCost  = item.UnitCost
                });
            }
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "CreatePurchaseOrder",
                $"Created PO #{po.POID} for SupplierID {po.SupplierID} with {model.Items.Count} line(s).");

            TempData["Success"] = $"Purchase Order #{po.POID} created successfully.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();

            var details = await _context.PurchaseOrderDetails
                .Include(d => d.Product)
                .Where(d => d.POID == id)
                .ToListAsync();

            return View(new PurchaseOrderDetailViewModel
            {
                Order     = po,
                Details   = details,
                TotalCost = details.Sum(d => d.Quantity * d.UnitCost)
            });
        }

        //POST /PurchaseOrder/Receive/{id}
        //marks PO as Received and updates inventory quantities
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.PurchaseOrderDetails)
                .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();
            if (po.Status != "Pending")
                return BadRequest("Only Pending orders can be received.");

            //update inventory for each line item
            foreach (var detail in po.PurchaseOrderDetails)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductID == detail.ProductID);

                if (inventory != null)
                {
                    inventory.Quantity    += detail.Quantity;
                    inventory.LastUpdated  = DateTime.Now;
                }

                //create Stock-In transaction record
                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductID       = detail.ProductID,
                    UserID          = CurrentUserID,
                    TransactionType = "Stock-In",
                    Quantity        = detail.Quantity,
                    Notes           = $"Received from PO #{po.POID}",
                    TransactionDate = DateTime.Now
                });
            }

            po.Status = "Received";
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "ReceivePurchaseOrder",
                $"Marked PO #{id} as Received. Inventory updated for {po.PurchaseOrderDetails.Count} product(s).");

            TempData["Success"] = $"Purchase Order #{id} received. Inventory updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        // POST /PurchaseOrder/Cancel/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var po = await _context.PurchaseOrders
                .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();
            if (po.Status != "Pending")
                return BadRequest("Only Pending orders can be cancelled.");

            po.Status = "Cancelled";
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "CancelPurchaseOrder",
                $"Cancelled PO #{id}.");

            TempData["Success"] = $"Purchase Order #{id} has been cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }
}
