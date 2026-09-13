# CoreDeck Dual-Edition Engineering Roadmap & Architecture Specification

> **Target Editions:** CoreDeck Community (Public GitHub) vs. CoreDeck-Pro (Local / Proprietary)  
> **Base Version:** v0.1.0 Alpha (Synchronized across both repositories)

---

## 1. Executive Strategy & Boundary Model

CoreDeck follows an **Open Core** architecture. The Public repository serves as a fully functional, modern e-commerce storefront for enthusiast hardware, while the Local Pro repository introduces high-value, professional-grade tools, automation, and business intelligence.

```
┌────────────────────────────────────────────────────────────────────────┐
│                   CoreDeck Community (Public GitHub)                   │
│                                                                        │
│  - Modern Slate/Cyan Enthusiast UI     - Dynamic Cart & Checkout       │
│  - Catalog Filtering & Sorting         - ASP.NET Core Identity         │
│  - Order Management & Invoicing        - Admin Product CRUD            │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                 git pull upstream main (One-way sync)
                                   │
                                   ▼
┌────────────────────────────────────────────────────────────────────────┐
│                     CoreDeck-Pro (Local Edition)                       │
│                                                                        │
│  [Extension / Proprietary Modules]                                     │
│  ├── 1. Interactive PC Builder with Hardware Compatibility Engine      │
│  ├── 2. Executive Analytics & Sales Radar Dashboard                    │
│  ├── 3. Stock Drop & Price Watch Real-Time Webhook/Email Alerts        │
│  ├── 4. Stripe / PayPal Multi-Currency Payment Gateway                 │
│  └── 5. Supplier CSV / Inventory Synchronization Hub                   │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Feature Matrix: Community vs. Pro

| Functional Area | Community Edition (Public) | Pro Edition (Local) | Value Justification |
| :--- | :--- | :--- | :--- |
| **Catalog & Browsing** | Categories, Price Filters, Search, In-Stock toggle, Sorting | All Community features + **Live Price History Charts** & **Hardware Comparison Matrix** | Gives tech shoppers deep hardware benchmarking data. |
| **System Building** | Standard individual part ordering | **Interactive Custom PC Builder** with Socket, TDP/Wattage, Form-Factor, and PCIe clearance rules | Flagship enthusiast feature; prevents buying incompatible parts. |
| **Cart & Checkout** | Local checkout with persisted order history & printable invoice | Community checkout + **Stripe / PayPal Gateway** with simulated or live payments and promo code discounts | Commercial transaction readiness. |
| **Inventory & Notifications** | Static stock count on product card | **Price Drop & Restock Radar** (User email alerts / notifications when scarce parts drop) | High engagement for GPU / Handheld drops. |
| **Admin Operations** | Product CRUD & Order Status updater (`Pending` -> `Delivered`) | **Executive Analytics Dashboard** (Revenue trends, profit margins, inventory velocity, top-selling chipsets) | Essential business visibility for store operators. |
| **Data Management** | Standard EF Core Seed data | **Bulk CSV / JSON Import & Export** for product batches, price adjustments, and orders | Eliminates manual item-by-item admin entry. |

---

## 3. Technical Roadmap

### Wave 1: CoreDeck-Pro Flagship Feature — Interactive PC Compatibility Builder
- **Domain Model**: `CompatibilityRule`, `ComponentSlot` (CPU, Motherboard, GPU, RAM, PSU, Cooler, Case).
- **Rules Engine**:
  - CPU Socket vs. Motherboard Socket match (e.g., LGA1700, AM5).
  - Form Factor fit (ATX, Micro-ATX, Mini-ITX vs. Case capacity).
  - Estimated Power Draw vs. PSU Wattage headroom calculation (+20% safety margin).
  - Cooler TDP rating vs. CPU TDP.
- **UI/UX**: Step-by-step visual rig builder with dynamic compatibility status bar and "Add Entire Rig to Cart" action.

### Wave 2: Executive Analytics & Sales Radar (Pro Admin)
- **Controller / Views**: `AdminAnalyticsController` with Chart.js visualization.
- **Metrics**: Total Revenue, Average Order Value (AOV), Order Fulfillment Velocity, Low-Stock Radar (< 5 units remaining), and Category breakdown.
- **Exporting**: Instant CSV export of sales reports.

### Wave 3: Community Maintenance & Continuous Sync
- Continuous bug fixes, security audits, and framework updates committed first to `CoreDeck` public repo.
- Tested and tagged on GitHub releases (`v0.1.1`, `v0.2.0`).
- Seamlessly pulled into `CoreDeck-Pro` with zero merge conflicts using standard upstream branch tracking.

---

## 4. Git Operational Workflow & Push Safeguards

```
                      +---------------------------------------+
                      |   GitHub Remote (Public)              |
                      |   https://github.com/.../CoreDeck.git |
                      +---------------------------------------+
                                  ▲               │
                       git push   │               │ git fetch/pull
                      (from base) │               │ (upstream)
                                  │               ▼
                +───────────────────+   +───────────────────+
                |    CoreDeck       |   |   CoreDeck-Pro    |
                |  (Public Local)   |   |  (Private Local)  |
                +───────────────────+   +───────────────────+
                                          Push URL: DO_NOT_PUSH_PUBLIC
```

1. **Working on Community Features**:
   - Work in `c:\Users\User\Desktop\CoreDeck`
   - Make edits, run `dotnet build`
   - Commit & push: `git push origin main`
2. **Synchronizing Pro with Public Base**:
   - Work in `c:\Users\User\Desktop\CoreDeck-Pro`
   - Run: `git pull upstream main`
3. **Working on Pro Features**:
   - Work in `c:\Users\User\Desktop\CoreDeck-Pro`
   - Commit locally. Push remote is safeguarded with `DO_NOT_PUSH_PUBLIC` to prevent accidental public publication.
