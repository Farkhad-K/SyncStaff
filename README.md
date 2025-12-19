# StaffSync

StaffSync is a small ASP.NET Core MVC application for importing, viewing and managing staff records.
It demonstrates a pragmatic, production-ready approach to CSV imports, EF Core data mapping, and a lightweight UI
for CRUD operations on employee records.

## Features
- Import employee data from CSV with robust parsing and duplicate handling.
- List and search employees in a responsive table (DataTables integration).
- View and edit employee details (AJAX update for a snappy UX).
- Delete employee records via AJAX.
- EF Core model configuration including DateOnly conversions and datetimeoffset audit fields.

## Repository layout

- `src/SyncStaff.MVC/` – ASP.NET MVC app (controllers, models, views, services, data layer)
- `tests/SyncStaff.MVC.Tests/` – unit tests (currently covers CSV import behavior)
- `dataset.csv` – (example/test CSV file area)

## Getting started (run locally)

Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, Docker SQL Server, or full SQL Server instance)

Clone the repo:

```bash
git clone https://github.com/Farkhad-K/SyncStaff.git
cd SyncStaff
```

Restore and build:

```bash
dotnet restore
dotnet build
```

Configure the database

The app expects a connection string named `SyncStaff` in `appsettings.json` or user secrets.
Example LocalDB connection string (SQL Server Express LocalDB):

```json
"ConnectionStrings": {
	"SyncStaff": "Server=(localdb)\\mssqllocaldb;Database=StaffSync;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Apply EF Core migrations and create the database (requires `dotnet-ef` tool):

```bash
dotnet tool install --global dotnet-ef
cd src/SyncStaff.MVC
dotnet ef database update
```

Run the application

```bash
dotnet run --project src/SyncStaff.MVC
```

By default the app will be available at `https://localhost:5011` (or as printed by the runtime).

### Importing data

Open `Employees -> Import & Manage` and upload a CSV file matching the expected columns. The import
process validates required fields, ignores duplicate payroll numbers, and reports inserted/failed counts.

## Tests

Run unit tests from the repo root:

```bash
dotnet test tests/SyncStaff.MVC.Tests
```

## Contributing

We welcome contributions. Suggested workflow:

1. Fork the repository and create a feature branch: `feature/your-change`.
2. Add tests for new behavior or bug fixes.
3. Run `dotnet build` and `dotnet test` to verify everything passes.
4. Open a pull request describing the change and rationale.

Code style and notes
- The app uses an `ISyncStaffDbContext` abstraction to make services testable and to avoid leaking EF Core types
	into higher layers.
- CSV import attempts a bulk insert and falls back to per-row inserts to maximize salvageable data on partial failures.
