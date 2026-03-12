# UniFleq IMS — Inventory Management System

> A web-based Inventory Management System developed for **UniFleq Solutions**, a computer hardware business based in Davao City, Philippines.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=flat&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-Database-CC2927?style=flat&logo=microsoftsqlserver)
![Live](https://img.shields.io/badge/Live-unifleq.runasp.net-d62a24?style=flat)

---

## 🌐 Live Demo

**[http://unifleq.runasp.net](http://unifleq.runasp.net)**

| Username   | Password     | Role              |
|------------|--------------|-------------------|
| admin      | (ask owner)  | Admin             |
| jbravo     | (ask owner)  | InventoryManager  |
| dphantom   | (ask owner)  | InventoryClerk    |
| jhilarious | (ask owner)  | ProcurementStaff  |
| bogsers12  | (ask owner)  | Auditor           |

---

## 📋 Overview

UniFleq IMS digitizes and centralizes the inventory operations of UniFleq Solutions. It replaces manual stock tracking with a secure, role-controlled, and fully auditable web platform accessible from any browser on desktop and mobile.

---

## ✨ Features

- **Dashboard** — 6 KPI cards, stock-by-category doughnut chart, Top 5 most in-demand products chart with date filter, low stock alerts table, and recent transactions
- **Stock Monitor** — real-time inventory levels with search and filter
- **Stock Adjustment** — Stock-In, Stock-Out, and Manual Adjustment with automated low stock email alerts
- **Valuation Report** — Standard Costing inventory valuation with Excel export and Print/PDF
- **Products** — full catalog management with archive and delete protection
- **Categories** — add, edit, delete with product count protection
- **Purchase Orders** — full PO lifecycle (Pending → Received / Cancelled) with auto inventory update on receive
- **Suppliers** — supplier directory with PO history protection
- **User Accounts** — role-based user management (Admin only)
- **Audit Logs** — paginated activity trail with filters and Excel export showing exported-by name

---

## 🔐 Security Features

| Feature | Implementation |
|---|---|
| Bot Prevention | Google reCAPTCHA v2 on login |
| Password Security | BCrypt hashing — passwords never stored in plain text |
| Session Management | ASP.NET Cookie Auth — HttpOnly, 8hr / 14-day RememberMe |
| Access Control | Role-Based Authorization (`[Authorize(Roles)]`) on all controllers |
| CSRF Protection | Anti-Forgery Token on all POST forms |
| Audit Logging | Every action logged with PST timestamp |
| Geolocation Tracking | Login IP resolved to city/region via ip-api.com |

---

## 👥 Roles & Access

| Role | Access |
|---|---|
| **Admin** | Full access to all modules |
| **InventoryManager** | Inventory, Products, Categories, POs, Suppliers, Valuation |
| **InventoryClerk** | Stock Monitor, Stock Adjustment |
| **ProcurementStaff** | Purchase Orders, Suppliers |
| **Auditor** | Stock Monitor, Valuation Report, Audit Logs |

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core (Code First) |
| Frontend | Razor Views, Bootstrap Icons, Chart.js |
| Authentication | ASP.NET Cookie Authentication with Claims |
| Email | MailKit — Gmail SMTP with STARTTLS |
| Hosting | MonsterASP.NET |

---

## 🔗 External APIs

| API | Purpose |
|---|---|
| Google reCAPTCHA v2 | Bot prevention on login form |
| ip-api.com | IP geolocation for audit log login tracking |
| Gmail SMTP | Automated low stock email alerts |

---

## 🚀 Getting Started (Local Setup)

### Prerequisites
- Visual Studio 2022
- .NET 8 SDK
- SQL Server (local or remote)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/kdu-333/unifleq-ims.git
   cd unifleq-ims
   ```

2. **Create `appsettings.json`** in the project root (see `appsettings.example.json` for the required fields)
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
     },
     "ReCaptcha": {
       "SiteKey": "YOUR_RECAPTCHA_SITE_KEY",
       "SecretKey": "YOUR_RECAPTCHA_SECRET_KEY"
     },
     "EmailSettings": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your@email.com",
       "SenderName": "UniFleq IMS",
       "AppPassword": "YOUR_GMAIL_APP_PASSWORD",
       "NotifyEmail": "notify@email.com"
     }
   }
   ```

3. **Run the database script**
   Execute `unifleq_database.sql` on your SQL Server instance to create tables and seed sample data.

4. **Run the project**
   ```bash
   dotnet run
   ```
   Or press **F5** in Visual Studio.

---

## 📁 Project Structure

```
UnifleqSolutions_IMS/
├── Controllers/          # MVC Controllers (Account, Dashboard, Inventory, etc.)
├── Models/               # Entity models (Product, Inventory, AuditLog, etc.)
├── ViewModels/           # ViewModels for typed views
├── Views/                # Razor .cshtml views
├── Services/             # AuditService, EmailService, CaptchaService, GeoLocationService
├── Data/                 # AppDbContext (Entity Framework Core)
├── wwwroot/              # Static files (CSS, JS, images)
└── appsettings.example.json
```

---

## 📊 Algorithm & Design Patterns

- **MVC Pattern** — Model-View-Controller architecture throughout
- **Standard Costing** — Inventory Value = Quantity × Unit Price (appropriate for fixed-price hardware)
- **RBAC** — Role-Based Access Control enforced at controller level
- **Top 5 Demand Analysis** — Groups Stock-Out transactions by product, sums quantity, orders descending

---

## ⚠️ Notes

- `appsettings.json` is excluded from version control
- BCrypt is used for all password hashing — no plain-text passwords in the database
- All timestamps are stored and displayed in **Philippine Standard Time (UTC+8)**

---
