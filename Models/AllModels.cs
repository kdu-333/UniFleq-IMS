using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnifleqSolutions_IMS.Models
{
    //ROLES
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }

    // USERS
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public int RoleID { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active / Inactive

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("RoleID")]
        public Role? Role { get; set; }
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }

    // CATEGORIES
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required, MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    // PRODUCTS
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required, MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Brand { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        public decimal UnitPrice { get; set; }

        public int? ReorderLevel { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Available"; // Available / Discontinued

        // Navigation
        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }
        public Inventory? Inventory { get; set; }
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
        public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
    }

    // INVENTORY
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int Quantity { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
    }

 
    // STOCK TRANSACTIONS
    public class StockTransaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required, MaxLength(20)]
        public string TransactionType { get; set; } = string.Empty; // Stock-In / Stock-Out / Adjustment

        [Required]
        public int Quantity { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }
    }


    // SUPPLIERS
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required, MaxLength(100)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(150)]
        public string? Address { get; set; }

        // Navigation
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }

    // PURCHASE ORDERS
    public class PurchaseOrder
    {
        [Key]
        public int POID { get; set; }

        [Required]
        public int SupplierID { get; set; }

        [Required]
        public int UserID { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending / Received / Cancelled

        // Navigation
        [ForeignKey("SupplierID")]
        public Supplier? Supplier { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }
        public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
    }

    // PURCHASE ORDER DETAILS
    public class PurchaseOrderDetail
    {
        [Key]
        public int PODetailID { get; set; }

        [Required]
        public int POID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; }

        // Navigation
        [ForeignKey("POID")]
        public PurchaseOrder? PurchaseOrder { get; set; }
        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
    }

    // AUDIT LOGS
    public class AuditLog
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Details { get; set; }

        public DateTime LogDate { get; set; } = DateTime.Now;

        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}
