<div align="center">

# ⚡ COREDECK

### Cyber-Grade PC Hardware & Gaming Gear E-Commerce Platform
**Community Edition (Open Source)**

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-blueviolet?style=for-the-badge&logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-00f0ff?style=for-the-badge)](LICENSE)
[![Status: Alpha v0.1.0](https://img.shields.io/badge/Status-Alpha%20v0.1.0-00ff66?style=for-the-badge)]()

<p align="center">
  A modern, high-performance web storefront crafted specifically for PC builders, overclockers, and gaming enthusiasts. Built on ASP.NET Core 10 MVC with a dark slate/neon cyberpunk aesthetic.
</p>

</div>

---

## 🖥️ Overview

**CoreDeck** is an enthusiast hardware e-commerce store designed to deliver an immersive, terminal-inspired shopping experience. Engineered with modern .NET 10 architectures, CoreDeck combines rapid server-rendered MVC with responsive client ergonomics, comprehensive inventory control, dynamic order fulfillment, and role-based access security.

CoreDeck follows an **Open Core** development model:
- **CoreDeck Community (This Repository)**: Free, open-source under the MIT license. Provides the complete, production-ready foundation: enthusiast UI, product catalog, cart, checkout, invoice generation, authentication, and inventory administration.
- **CoreDeck-Pro (Commercial Edition)**: Adds advanced enthusiast tools, including the interactive custom PC compatibility rules engine, executive sales radar & analytics dashboards, real-time restock alerts, and payment gateways.

---

## ✨ Features

### 🛒 Enthusiast Storefront
- **Dynamic Hardware Arsenal**: Browse CPUs, GPUs, Motherboards, RAM, Cases, Power Supplies, and Handhelds.
- **Multi-Faceted Filtering & Sorting**: Filter instantly by hardware category, price range slider, search keywords, and stock availability toggle.
- **Interactive Cyberpunk UI**: Sleek dark slate glassmorphism cards, glowing cyan/neon accents, and typography tailored with *Inter* and *JetBrains Mono*.

### ⚡ Cart & Order Operations
- **Session-Persisted Cart**: Real-time cart state management with instant quantity adjustments, item removals, and dynamic badge updates.
- **Operator Checkout Pipeline**: Seamless checkout workflow capturing deployment destination, recipient details, and payment terms.
- **Deployment Tracking**: Order lifecycle tracking across statuses: `Pending` ➔ `Processing` ➔ `Shipped` ➔ `Delivered` (or `Cancelled`).
- **Cyber-Grade Printable Invoices**: Formatted printable/viewable deployment invoices with itemized specs, order serials, and cost breakdowns.

### 🛡️ Security & Role-Based Identity
- **ASP.NET Core Identity**: Strict credential validation, password complexity enforcement, and account management.
- **Role Separation**: Segregation between `Customer` (Shoppers/Operators) and `Admin` (Root Administrators).
- **Anti-Forgery & Injection Protection**: CSRF tokens enabled across all mutable forms; EF Core parameterized queries mitigate SQL injection risks.

### ⚙️ Root Inventory & Order Management
- **Hardware Inventory Control**: Add, modify, activate/deactivate hardware items, configure technical specs, and manage real-time stock levels.
- **Orders Dispatch Terminal**: Review inbound orders, filter by dispatch status, inspect item manifests, and update fulfillment milestones.
- **Automated Database Seeding**: Automatic EF Core schema migrations and rich seed data on launch.

---

## 🏗️ Architecture & Project Structure

CoreDeck is architected around clean ASP.NET Core MVC and Service-Repository conventions:

```
CoreDeck/
├── Controllers/                 # MVC Controllers
│   ├── AccountController.cs     # Identity authentication & operator profile
│   ├── AdminOrderController.cs  # Administrative order dispatch & status transitions
│   ├── AdminProductController.cs# Administrative inventory CRUD
│   ├── CartController.cs        # Cart interactions & session state
│   ├── HomeController.cs        # Landing page & system status
│   ├── OrderController.cs       # Checkout & customer order tracking
│   └── ProductController.cs     # Hardware catalog browsing & filters
├── Models/                      # Data entities & view models
│   ├── ApplicationUser.cs       # Extended Identity user entity
│   ├── Cart.cs & CartItem.cs    # Domain models for shopping cart
│   ├── Category.cs              # Hardware component categories
│   ├── Order.cs & OrderItem.cs  # Deployment orders & manifest items
│   └── Product.cs               # Hardware catalog specifications
├── Services/                    # Core business logic layer
│   ├── CartService.cs           # Session-backed cart management
│   ├── OrderService.cs          # Order processing & transaction lifecycle
│   └── DatabaseInitializerService.cs # Database migration & seed orchestrator
├── Data/                        # Data access
│   └── ApplicationDbContext.cs  # EF Core DbContext with relationship definitions
├── Views/                       # Razor Views & Cyberpunk UI layouts
│   ├── Shared/_Layout.cshtml    # Master navigation & cyberpunk shell
│   └── ...                      # Feature-specific view directories
└── wwwroot/                     # Static assets (Cyberpunk CSS, JS, vendor libs)
```

---

## 🚀 Quick Start & Local Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (v10.0.301 or newer)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/) (LocalDB, Express, or standard instance)
- Git

### 1. Clone the Repository
```bash
git clone https://github.com/MaskirovkaOtdel/CoreDeck.git
cd CoreDeck
```

### 2. Configure Database Connection
Inspect `appsettings.json` and adjust the connection string if needed (default is configured for LocalDB):

```json
{
  "ConnectionStrings": {
    "CoreDeckDB": "Server=(localdb)\\mssqllocaldb;Database=CoreDeckDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 3. Restore & Build
```bash
dotnet restore
dotnet build
```

### 4. Run the Application
```bash
dotnet run
```
Navigate to `https://localhost:7001` or `http://localhost:5001` in your browser.

> **Note**: Database schema migrations and seed data will be automatically applied on initial launch!

---

## 🔑 Default Credentials

The automated database initializer provisions default operator accounts for testing:

| Role | Username / Email | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Root Administrator** | `admin@coredeck.local` | `Admin123!` | Full Access (Inventory Control, Order Dispatch) |
| **Enthusiast Customer** | *(Register via UI)* | User-defined (min 8 chars, mixed case + digit) | Customer Access (Browse, Cart, Order, Invoice) |

---

## 🛠️ Technology Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Framework** | ASP.NET Core 10.0 | High-throughput web framework |
| **Language** | C# 14 | Modern, type-safe development |
| **ORM** | Entity Framework Core 10 | Object-relational mapping with SQL Server |
| **Auth** | ASP.NET Core Identity | Secure authentication, password hashing & authorization |
| **Design System** | Bootstrap 5.3 + Custom CSS | Cyber-grade neon/slate glassmorphism interface |
| **Icons & Typography** | FontAwesome 6, Inter, JetBrains Mono | Enthusiast monospace & UI iconography |

---

## 🗺️ Open Core Roadmap

CoreDeck is actively developed. For an overview of upcoming milestones and feature waves between Community and Pro editions, refer to [ROADMAP.md](ROADMAP.md).

- **Wave 1 (Pro)**: Interactive Custom PC Compatibility Builder (Socket, Form Factor, TDP/Wattage rules) — *Released in CoreDeck-Pro*
- **Wave 2 (Pro)**: Executive Analytics & Sales Radar Dashboard (Revenue analytics, category breakdown, low-stock radar, CSV exports) — *Released in CoreDeck-Pro*
- **Wave 3 (Dual)**: Payment Gateway Integrations & Automated Inventory Sync Hub

---

## 📄 License

CoreDeck Community Edition is open-source software licensed under the **[MIT License](LICENSE)**.  
For commercial proprietary features in CoreDeck-Pro, refer to the license agreement provided with the Pro distribution.
