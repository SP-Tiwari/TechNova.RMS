# ADR-004: Persistence using JSON files (JsonRmsRepository)

- **Status:** Accepted

## Context

The application requires a persistence strategy for employees, projects, and allocations. For a Week 1 lab the goals were simplicity, transparency for students, and minimal operational dependencies.

The repository contains `IRmsRepository` (persistence abstraction) and `JsonRmsRepository` in `TechNova.RMS.Infrastructure.Persistence` which serializes DTOs to JSON files.

## Decision

Persist data as JSON files using `JsonRmsRepository`:

- Use `System.Text.Json` for serialization with `JsonStringEnumConverter` and indented output.
- Store files named `employees.json`, `projects.json`, and `allocations.json` in a `Data` folder packaged with the console project (the console project includes `Data\*.json` as content with `CopyToOutputDirectory` = `PreserveNewest`).
- At runtime `JsonRmsRepository` resolves the data directory using `Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\Data"))` and reads/writes the JSON DTOs, mapping them to domain objects.

This is the implemented approach in the codebase.

## Alternatives Considered

1. Relational database (e.g., SQLite or SQL Server)
   - Pros: ACID guarantees, concurrency, query capabilities, realistic production behavior.
   - Cons: adds operational complexity, connection setup, and is heavy for a beginner lab.

2. Embedded document store or binary serialization
   - Pros: faster I/O, potentially simpler programmatic model.
   - Cons: adds a dependency or yields binary blobs that are not human-readable for teaching; JSON is more instructive for learning and debugging.

## Consequences

- Benefits
  - Human-readable seed data the students can open and modify.
  - No external service required; easy to run locally and to inspect persisted state.
  - Straightforward mapping between DTOs and domain types.

- Trade-offs / Limitations
  - JSON files are not suitable for concurrent multi-user scenarios and have no transactional guarantees.
  - Performance and query capabilities are limited compared to a database.
  - Path resolution depends on how the application is launched; the code uses a relative path from the assembly base directory which works for the console project when run from the repository.

This ADR documents why JSON persistence was chosen for the lab and the practical trade-offs observed in the implementation.