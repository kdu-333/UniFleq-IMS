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
    public class SupplierController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _audit;

        public SupplierController(AppDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        private int CurrentUserID =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.PurchaseOrders)
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            return View(new SupplierViewModel
            {
                Suppliers = suppliers,
                NewSupplier = new Supplier()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Suppliers.Add(model);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "CreateSupplier",
                $"Added supplier '{model.SupplierName}'");

            return Json(new
            {
                success = true,
                supplierID = model.SupplierID,
                supplierName = model.SupplierName,
                contact = model.ContactNumber,
                address = model.Address
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id, string supplierName, string? contactNumber, string? address)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            var oldName = supplier.SupplierName;
            supplier.SupplierName = supplierName;
            supplier.ContactNumber = contactNumber;
            supplier.Address = address;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "EditSupplier",
                $"Edited supplier '{oldName}' → '{supplierName}'");

            return Json(new
            {
                success = true,
                supplierID = supplier.SupplierID,
                supplierName = supplier.SupplierName,
                contact = supplier.ContactNumber,
                address = supplier.Address
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null) return NotFound();

            if (supplier.PurchaseOrders.Any())
                return BadRequest("Cannot delete a supplier with existing purchase orders.");

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserID, "DeleteSupplier",
                $"Deleted supplier '{supplier.SupplierName}'");

            return Json(new { success = true });
        }
    }
}