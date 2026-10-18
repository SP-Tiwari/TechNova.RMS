# ADR-005: Dependency injection and composition root (Program.cs)

- **Status:** Accepted

## Context

The console application must compose services, wire concrete implementations to abstractions, and provide instances to `ConsoleMenu` and application services. A minimal, standard approach was required that is familiar to students and integrates with the SDK.

The project contains a `Program.cs` in `TechNova.RMS.Console` that configures services using `Microsoft.Extensions.DependencyInjection`.

## Decision

Use `Microsoft.Extensions.DependencyInjection` with constructor injection and `Program.cs` as the composition root. The implemented registrations (observed in `Program.cs`) include:

- `services.AddSingleton<IRmsRepository, JsonRmsRepository>();`
- `services.AddSingleton<EmployeeMatchingService>();`
- `services.AddSingleton<ReportingService>();`
- `services.AddSingleton<EmployeeService>();`
- `services.AddSingleton<AllocationService>();`
- `services.AddSingleton<AllocationApplicationService>();`
- `services.AddSingleton<EmployeeManagementService>();`
- `services.AddSingleton<ProjectManagementService>();`
- `services.AddSingleton<ConsoleMenu>();`

`Program.cs` builds the `ServiceProvider`, resolves `ConsoleMenu`, and calls `menu.Run()` to start the interactive UI. Services are injected via constructors (for example `ConsoleMenu` expects `EmployeeService`, `EmployeeMatchingService`, `IRmsRepository`, and other services in its constructor).

## Alternatives Considered

1. Manual composition (newing up dependencies in `Program.cs` without a DI container)
   - Pros: no external package required and simple for very small programs.
   - Cons: manual wiring can become verbose and error-prone as the number of services grows; harder to support flexible lifetimes or swap implementations for tests.

2. Use a different DI container (Autofac, SimpleInjector)
   - Pros: additional features (modules, advanced lifetimes) and richer integration points.
   - Cons: adds extra dependency and conceptual overhead for a lab; `Microsoft.Extensions.DependencyInjection` is standard, lightweight, and adequate for the project's needs.

## Consequences

- Benefits
  - Clear, standard composition pattern that students are likely to encounter in real .NET projects.
  - Constructor injection makes dependencies explicit and aids testability.
  - Using the DI container simplifies swapping `IRmsRepository` or other implementations in the future.

- Trade-offs / Limitations
  - All services are registered as singletons for simplicity; this is acceptable for a single-process console app but might be inappropriate for stateful services or web scenarios.
  - The code uses concrete service types (no interfaces for every service) which is simple but may limit mocking or interception in some tests.

This ADR records the implemented DI approach and the reasons it was chosen for the lab.