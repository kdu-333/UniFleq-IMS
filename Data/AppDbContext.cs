using Microsoft.EntityFrameworkCore;
using UnifleqSolutions_IMS.Models;

namespace UnifleqSolutions_IMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //db sets
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            //one-to-one: Product <-> Inventory
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(i => i.ProductID);

            //Cascade rules
            // Prevent multiple cascade paths on SQL Server
            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.User)
                .WithMany(u => u.StockTransactions)
                .HasForeignKey(st => st.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.User)
                .WithMany(u => u.PurchaseOrders)
                .HasForeignKey(po => po.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            //Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Admin",            Description = "Full system access" },
                new Role { RoleID = 2, RoleName = "InventoryManager", Description = "Manage stock and products" },
                new Role { RoleID = 3, RoleName = "InventoryClerk",   Description = "Record transactions and view stock" },
                new Role { RoleID = 4, RoleName = "ProcurementStaff", Description = "Manage purchase orders" },
                new Role { RoleID = 5, RoleName = "Auditor",          Description = "Read-only access to reports" }
            );

            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID       = 1,
                    RoleID       = 1,
                    FullName     = "System Administrator",
                    Username     = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Status       = "Active",
                    CreatedAt    = new DateTime(2026, 1, 1)
                }
            );

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "CPU",         Description = "Central Processing Units" },
                new Category { CategoryID = 2, CategoryName = "GPU",         Description = "Graphics Processing Units" },
                new Category { CategoryID = 3, CategoryName = "RAM",         Description = "Memory Modules" },
                new Category { CategoryID = 4, CategoryName = "SSD",         Description = "Solid State Drives" },
                new Category { CategoryID = 5, CategoryName = "HDD",         Description = "Hard Disk Drives" },
                new Category { CategoryID = 6, CategoryName = "Motherboard", Description = "Main circuit boards" },
                new Category { CategoryID = 7, CategoryName = "PSU",         Description = "Power Supply Units" },
                new Category { CategoryID = 8, CategoryName = "Peripherals", Description = "Keyboards, Mice, Monitors, etc." }
            );
        }
    }
}
