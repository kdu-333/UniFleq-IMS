using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UnifleqSolutions_IMS.Models;

namespace UnifleqSolutions_IMS.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public DateTime? DemandFilterFrom { get; set; }
        public DateTime? DemandFilterTo { get; set; }
        public int TotalCategories { get; set; }
        public int LowStockCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int TotalTransactionsToday { get; set; }

        public List<LowStockItem> LowStockItems { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
        public List<CategoryStockSummary> CategoryStockSummary { get; set; } = new();
        public List<TopDemandItem> TopDemandItems { get; set; } = new();
    }

    public class LowStockItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
    }

    public class RecentTransaction
    {
        public int TransactionID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }

    public class CategoryStockSummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int ProductCount { get; set; }
    }

    public class TopDemandItem
    {
        public string ProductName { get; set; } = string.Empty;
        public int StockOutCount { get; set; }
    }

    public class ProductViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public Product NewProduct { get; set; } = new();
    }

    public class ProductEditViewModel
    {
        public Product Product { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }

    public class CategoryViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public Category NewCategory { get; set; } = new();
    }

    public class InventoryViewModel
    {
        public List<InventoryItem> Items { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? FilterCategory { get; set; }
        public string? FilterStatus { get; set; }
        public string? SearchTerm { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class InventoryItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StockStatus { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class StockAdjustmentViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public Product NewProduct { get; set; } = new();

        public StockAdjustmentForm AdjustmentForm { get; set; } = new();
    }

    public class StockAdjustmentForm
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        public string TransactionType { get; set; } = "Stock-In";

        [Required, Range(1, 99999)]
        public int Quantity { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }
    }

    public class StockTransactionViewModel
    {
        public List<StockTransaction> Transactions { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public string? FilterType { get; set; }
        public int? FilterProduct { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class SupplierViewModel
    {
        public List<Supplier> Suppliers { get; set; } = new();
        public Supplier NewSupplier { get; set; } = new();
    }

    public class PurchaseOrderViewModel
    {
        public List<PurchaseOrder> Orders { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();
        public string? FilterStatus { get; set; }
    }

    public class PurchaseOrderCreateViewModel
    {
        [Required]
        public int SupplierID { get; set; }

        public List<Supplier> Suppliers { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        public List<PODetailForm> Items { get; set; } = new() { new PODetailForm() };
    }

    public class PODetailForm
    {
        [Required]
        public int ProductID { get; set; }

        [Required, Range(1, 99999)]
        public int Quantity { get; set; }

        [Required, Range(0.01, 999999.99)]
        public decimal UnitCost { get; set; }
    }

    public class PurchaseOrderDetailViewModel
    {
        public PurchaseOrder Order { get; set; } = new();
        public List<PurchaseOrderDetail> Details { get; set; } = new();
        public decimal TotalCost { get; set; }
    }

    public class UserViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
    }

    public class UserCreateViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public int RoleID { get; set; }

        public string Status { get; set; } = "Active";
        public List<Role> Roles { get; set; } = new();
    }

    public class UserEditViewModel
    {
        public int UserID { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public int RoleID { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [DataType(DataType.Password)]
        [Display(Name = "New Password (leave blank to keep current)")]
        public string? NewPassword { get; set; }

        public List<Role> Roles { get; set; } = new();
    }

    public class AuditLogViewModel
    {
        public List<AuditLog> Logs { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public int? FilterUser { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SearchAction { get; set; }
    }

    public class ValuationReportViewModel
    {
        public List<ValuationItem> Items { get; set; } = new();
        public decimal TotalValue { get; set; }
        public int TotalUnits { get; set; }
        public List<Category> Categories { get; set; } = new();
        public int? FilterCategoryID { get; set; }
    }

    public class ValuationItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalValue { get; set; }
    }
}