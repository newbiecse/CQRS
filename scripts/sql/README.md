# Database scripts

SQL scripts and tooling to create all CQRS demo databases and tables.

## Quick start (recommended)

From repo root:

```powershell
.\scripts\initialize-databases.ps1
```

Or:

```bash
dotnet run --project tools/CqrsDemo.DatabaseInitializer/CqrsDemo.DatabaseInitializer.csproj -c Release
```

Uses **EF Core `EnsureCreated`** against the same models as the microservices (`*DbContext`).

## Per-database SQL files

Each `{DatabaseName}.sql` contains:

1. `CREATE DATABASE` (if missing)
2. `USE [database]`
3. `CREATE TABLE` / indexes from EF `GenerateCreateScript()`

Run individually in SSMS or:

```powershell
sqlcmd -S localhost -E -i scripts\sql\CqrsDemo_Product_Write.sql
```

Export scripts again after model changes:

```bash
dotnet run --project tools/CqrsDemo.DatabaseInitializer -- --export-sql
```

## Databases

| Database | Tables |
|----------|--------|
| `CqrsDemo_Product_Write`, `CqrsDemo_Cart_Write`, `CqrsDemo_Order_Write`, `CqrsDemo_Payment_Write`, `CqrsDemo_User_Write` | `StoredEvents`, `OutboxMessages` |
| `CqrsDemo_Product_Read` | `Products` |
| `CqrsDemo_Cart_Read` | `Carts`, `CartLines` |
| `CqrsDemo_Order_Read` | `Orders`, `OrderLines` |
| `CqrsDemo_Payment_Read` | `Payments` |
| `CqrsDemo_User_Read` | `Users` |
| `CqrsDemo_Saga` | `CheckoutSagas` |
| `CqrsDemo_Reporting` | `UserProfiles`, `OrderFacts` |
| `CqrsDemo_Write`, `CqrsDemo_Read` | Legacy monolith schemas |

Connection string template (appsettings):  
`Server=localhost;database={name};Trusted_Connection=False;persist security info=True;Integrated Security=SSPI;`

The initializer tool also appends `TrustServerCertificate=True` for local SQL Server encryption.
