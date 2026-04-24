# Vehicle Parts Management — Backend
ASP.NET Core + Clean Architecture backend for the Vehicle Parts Selling & Inventory Management System.

## Tech Stack
| Concern | Library / Framework |
|---|---|
| Framework | ASP.NET Core Web API |
| Language | C# |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Authentication | ASP.NET Core Identity + JWT |
| API Docs | OpenAPI |

## Getting Started
### Prerequisites
- .NET SDK (latest stable supported by project)
- PostgreSQL

### Restore
```bash
dotnet restore
```

### Configuration
Update:
- `VehiclePartsManagement.Presentation/appsettings.json`

Required values:
- `ConnectionStrings:DefaultConnection`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Secret`
- `Jwt:ExpiryHours`
- `Jwt:RefreshTokenExpiryDays`

### Run Locally
```bash
dotnet run --project VehiclePartsManagement.Presentation
```

## Common Commands
| Command | Description |
|---|---|
| `dotnet build VehiclePartsManagement.slnx` | Build solution |
| `dotnet run --project VehiclePartsManagement.Presentation` | Run API |
| `dotnet ef migrations add <Name> --project VehiclePartsManagement.Infrastructure --startup-project VehiclePartsManagement.Presentation` | Add migration |
| `dotnet ef database update --project VehiclePartsManagement.Infrastructure --startup-project VehiclePartsManagement.Presentation` | Apply migration |

## Project Structure
```
VehiclePartsManagement.Presentation/
├── Controllers/           # API endpoints
├── Middlewares/           # Exception middleware
└── Program.cs             # App pipeline + startup config

VehiclePartsManagement.Application/
├── Common/                # ApiResponse, PagedResponse
├── DTOs/
│   └── Auth/              # Auth DTOs (one class per file)
├── Interfaces/            # Service/repository contracts
└── Services/              # Business services

VehiclePartsManagement.Domain/
└── Entities/              # Domain entities (ApplicationUser, etc.)

VehiclePartsManagement.Infrastructure/
├── Config/                # JWT options/service
├── Data/                  # DbContext + Identity seeder
├── Repositories/          # Generic repository implementation
└── DependencyInjection.cs # Infrastructure registrations
```

## API Response Contract
All API responses follow a unified shape:

```json
{
  "success": true,
  "message": "Request completed successfully",
  "errors": {},
  "data": {}
}
```

Contract rules:
- success/failure always use the same envelope
- validation errors map to field-wise `errors`
- unhandled exceptions return standardized failure response
- JSON response naming is camelCase

## Seed Data
Identity seeding ensures baseline roles and users exist.

Seeded roles:
- `Admin`
- `Staff`
- `Customer`

Seeded users (password: `Hello@123`):
- `admin@gmail.com` (`Admin`)
- `staff@gmail.com` (`Staff`)
- `customer@gmail.com` (`Customer`)

## Architecture
Flow:
`Controller -> Service -> Repository -> DbContext`

Rules:
- controllers stay thin and return standardized `ApiResponse` types
- business logic stays in services
- data access goes through repositories/EF Core
- no raw database logic inside controllers
