# LeaveMgmt

A modern **Leave Management System** built with **.NET 9**, **Blazor Server**, and **Entity Framework Core**.  
It provides employees with an easy way to request leave, managers with tools to approve/reject requests, and admins with oversight of all activities.

---

## 🚀 Features

- **Authentication & Authorization**
  - JWT-based login and role-based access control (`Employee`, `Manager`, `Admin`, `Support`).
- **Employee Dashboard**
  - Submit leave requests with type, date range, and automatic day calculations (skips weekends & holidays).
  - View current and past requests.
  - Retract pending requests.
- **Manager Dashboard**
  - View requests from team members.
  - Approve or reject leave requests with reason tracking.
- **Admin Functions**
  - Access to all requests and user management endpoints.
- **Calendar Integration**
  - Leave days visually highlighted.
- **Validation & Policy**
  - Prevents invalid date ranges (e.g., `To Date` earlier than `From Date`).
  - Enforces leave type rules (e.g., max days per request).
  - **LeavePolicy** prevents overlaps with already approved leave ranges.
- **Seeding**
  - Initial **Leave Types**, **Users**, and **Roles** automatically seeded from JSON/TXT files in `Seeds/`.
- **Logging**
  - Centralized logging using **Serilog** with rolling file logs.
- **Messaging**
  - Event-driven outbox dispatcher with **Redis Pub/Sub** integration.

---

## 🛠️ Tech Stack

- **Backend**
  - [.NET 9](https://dotnet.microsoft.com/) (ASP.NET Core Web API with [FastEndpoints](https://fast-endpoints.com/))
  - [Entity Framework Core](https://learn.microsoft.com/en-us/ef/) with SQL Server
  - Custom Mediator for CQRS pattern
  - Domain-Driven Design (DDD) principles
  - **LeavePolicy** domain service for business rules
  - **Serilog** for structured logging
  - **Redis Pub/Sub** via `StackExchange.Redis`

- **Frontend**
  - [Blazor Server](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
 
- **Authentication**
  - JSON Web Tokens (JWT)
  - Role-based authorization (`Employee`, `Manager`, `Admin`, `Support`)

- **Database**
  - Microsoft SQL Server
  - Migrations via EF Core
  - Automatic seeding (`LeaveTypeSeeder`, `UserRosterSeeder`, `IdentitySeeder`)

---

## 📂 Project Structure

```
LeaveMgmt.sln
│
├── src
│   ├── LeaveMgmt.Api              # ASP.NET Core Web API (endpoints, auth, CQRS, Serilog, Redis)
│   ├── LeaveMgmt.Application      # Application layer (commands, queries, DTOs, mediator)
│   ├── LeaveMgmt.Domain           # Domain models, business rules, LeavePolicy
│   ├── LeaveMgmt.Infrastructure   # EF Core persistence, repositories, seeders, event bus
│   └── LeaveMgmt.Website          # Blazor Server frontend
│
└── tests
    ├── LeaveMgmt.Api.FunctionalTests         # API functional tests (endpoints)
    ├── LeaveMgmt.Application.UnitTests       # Application-layer tests
    ├── LeaveMgmt.Domain.UnitTests            # Domain entity & LeavePolicy tests
    ├── LeaveMgmt.Infrastructure.UnitTests    # Repository tests
    ├── LeaveMgmt.Infrastructure.FunctionalTests
    └── LeaveMgmt.Infrastructure.IntegrationTests
```

---

## 🔑 Roles and Permissions

- **Employee**
  - Submit and view their own requests
  - Retract pending requests
- **Manager**
  - Approve or reject requests from employees
  - View team requests
- **Admin**
  - Manage users and view all requests
- **Support**
  - Manage any support users needed. 

---

## ▶️ Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Goldin123/LeaveManagement.git
cd LeaveManagement
```

### 2. Configure Database

- Update **connection string** in `appsettings.json` (API and Website).
- Run EF Core migrations:

```bash
dotnet ef database update --project LeaveMgmt.Infrastructure --startup-project LeaveMgmt.Api
```

### 3. Enable Seeding (optional)

In `appsettings.json`, toggle seeding options:

```json
"Seed": {
  "LeaveTypes": true,
  "Users": true,
  "DefaultPassword": "ChangeMe123!"
},
"Teams": {
  "Dev": "Seeds/Dev Team.txt",
  "Management": "Seeds/Managment Team.txt",
  "Support": "Seeds/Support Team.txt"
}
```

Seeders included:
- `LeaveTypeSeeder` → seeds default leave types from `Seeds/LeaveTypes.json`
- `IdentitySeeder` → seeds Employee/Manager roles
- `UserRosterSeeder` → seeds users from team roster TXT files (`Dev`, `Management`, `Support`)

### 4. Configure Redis (for Event Bus)

Add Redis connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "Redis": "localhost:6379"
}
```

This enables:
- **RedisEventBus** for publishing domain events
- **OutboxDispatcher** background service to flush events from DB to Redis

### 5. Run the Solution

```bash
dotnet build
dotnet run --project LeaveMgmt.Api
dotnet run --project LeaveMgmt.Website
```

- API Swagger available at: `https://localhost:7186/swagger`
- Blazor UI available at: `https://localhost:7089`
- Logs written to `Logs/log-*.txt`

---

## 🧪 Testing

Tests are organized into:

- **Unit Tests** → Core domain logic (e.g., LeaveRequest, LeavePolicy)
- **Functional Tests** → CQRS commands/queries and API endpoints
- **Integration Tests** → Persistence and external systems (DB, Redis)

Run all tests:

```bash
dotnet test
```

---

## 📌 Roadmap

- [ ] Add email notifications on approval/rejection
- [ ] Add multi-level approval flow
- [ ] Add reporting and analytics dashboards
- [ ] Dockerize solution

---

## 📄 License

This project is licensed under the MIT License.  
See the [LICENSE.txt](LICENSE.txt) file for details.

---

## 👤 Author

**Goldin Baloyi**   
🚀 Focused on modern enterprise applicationss.
