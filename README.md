# TechNova Resource Management System (RMS) - Week 1 Lab

## Project Overview

`TechNova.RMS` is a C# console application for managing employees, skills, projects, and allocations. It is a small educational Resource Management System that demonstrates domain modelling, application services, simple reporting, and file-based persistence.

## Key Features (implemented)

- View employees, projects, and allocations.
- Add, update, and deactivate employees.
- Create projects.
- Find matching employees for a project (by required skills and availability).
- View project staffing summaries, bench reports, and department utilisation.
- Allocate employees to projects and release allocations.
- Enforce allocation validation rules (see Business Rules).
- Persist data using JSON files in the `TechNova.RMS.Console/Data` folder.
- Simulated allocation/release notifications implemented as console messages.

## Technology Stack

- .NET Target Framework: `net10.0` (confirmed in project files).
- Nullable reference types: `Nullable` enabled in project files.
- Implicit usings are enabled.
- Package: `Microsoft.Extensions.DependencyInjection` (console project references version `10.0.13`).
- C# language version: not explicitly set in the project files (uses SDK default).

## Solution Structure

- `TechNova.RMS.Domain` — domain models and domain services (employees, projects, allocations, validation logic).
- `TechNova.RMS.Application` — application services and persistence abstraction (`IRmsRepository`).
- `TechNova.RMS.Infrastructure` — concrete persistence (`JsonRmsRepository`) and DTO mapping.
- `TechNova.RMS.Console` — console UI, `Program.cs` composition root, and sample JSON data in `Data/`.

For design details and diagrams see `DESIGN.md`.

## Prerequisites

- .NET SDK 10 (install the matching SDK that supports `net10.0`).

Verify installation:

```bash
dotnet --version
```

(Expect a .NET 10.x SDK version.)

## Build and Run (from solution root)

Restore packages and build the solution:

```bash
dotnet restore TechNova.RMS.slnx
dotnet build TechNova.RMS.slnx -c Debug
```

Run the console application from the solution root:

```bash
dotnet run --project TechNova.RMS.Console/TechNova.RMS.Console.csproj
```

Or run the built executable (after build) from the console project output folder.

## How to Use the Application (console menu)

When running, the interactive menu presents these options (enter the number and press Enter):

1. View Employees
2. View Projects
3. View Allocations
4. Find Matching Employees for a Project — select a project ID and the app lists employees who meet required skills and are available.
5. View Project Staffing Summary — shows employee counts and total allocation per project.
6. View Bench Report — enter a date (yyyy-MM-dd) and threshold to list under-utilised employees.
7. View Department Utilisation — enter a date to see utilisation by department.
8. Allocate Employee to Project — create an allocation (validated by domain rules).
9. Add Employee — interactively add a new employee and skills.
10. Update Employee — update existing employee details.
11. Deactivate Employee — mark an employee inactive.
12. Create Project — interactively create a project and required skills.
13. Release Allocation — remove an existing allocation by ID.
0. Exit

Follow prompts for each option. Input validation is performed where applicable.

## Business Rules (enforced by code)

The application enforces these rules (from `AllocationService`, domain constructors and methods):

- An inactive employee cannot be allocated. (`AllocationService.ValidateAllocation` checks `employee.IsActive`.)
- An employee's total allocation percentage must not exceed 100% on any day across overlapping allocations. (`AllocationService.ValidateAllocation` sums allocations per day and throws if total would exceed 100%.)
- Allocation dates must fall within the associated project's start and end dates. (Validation in `AllocationService.ValidateAllocation`.)

Additional domain rules:

- `Employee` requires a non-empty ID and non-empty name/email/designation/department; skill proficiency levels must be between 1 and 5.
- `Project` requires non-empty ID and name/client and enforces `EndDate >= StartDate`.
- `Allocation` constructor enforces a positive allocation percentage (greater than 0 and <= 100) and valid dates.

## Data Persistence and Seed Data

- Seed data files are under: `TechNova.RMS.Console/Data`.
- Files included in the repository (copied to output on build):
  - `employees.json` (15+ employees in the seed data)
  - `projects.json` (5+ projects in the seed data)
  - `allocations.json` (10+ allocations in the seed data)

- Runtime location: `JsonRmsRepository` resolves the data directory using:

```csharp
Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\Data"))
```

That means the repository expects a `Data` folder relative to the running assembly. The console project includes the JSON files as content and uses `CopyToOutputDirectory` so they are available when running from the project.

## Architecture and Design

This solution follows a layered architecture with separate Domain, Application, Infrastructure, and Console projects. See `DESIGN.md` for class diagrams, SOLID observations, and more design notes:

- `./DESIGN.md`

## Known Limitations (confirmed)

- The console UI is single-user and interactive — not suitable for concurrent multi-user scenarios.
- JSON file persistence is simple and human-readable but has limited concurrency and no transactional guarantees.
- Notifications are simulated using `Console.WriteLine` in `AllocationApplicationService` rather than an event-driven or external notification mechanism.

---

Created/updated file:

- `C:\DotNetTraining\Day01\TechNova.RMS\README.md`

If you want the README adjusted (different setup commands, more examples, or badges), tell me which parts to change.