# ADR-001: Layered project architecture (Domain / Application / Infrastructure / Console)

- **Status:** Accepted

## Context

The solution is a small educational Resource Management System. During project setup the codebase was split across multiple projects: `TechNova.RMS.Domain`, `TechNova.RMS.Application`, `TechNova.RMS.Infrastructure`, and `TechNova.RMS.Console`.

The concern was how to organise code so domain rules, use-case logic, persistence, and the console UI are separated and easy to understand for students while keeping the solution executable as a simple console app.

## Decision

Use a layered project structure with four projects:

- `TechNova.RMS.Domain` — contains domain models and domain services (entities such as `Employee`, `Project`, `Allocation` and `AllocationService`).
- `TechNova.RMS.Application` — implements application / use-case services and defines the persistence abstraction `IRmsRepository` (services such as `EmployeeService`, `EmployeeManagementService`, `AllocationApplicationService`, `ReportingService`, and `EmployeeMatchingService`).
- `TechNova.RMS.Infrastructure` — provides concrete infrastructure, currently `JsonRmsRepository` that implements `IRmsRepository` and maps DTOs to domain types.
- `TechNova.RMS.Console` — presentation and composition root (`Program.cs` and `ConsoleMenu`), plus sample JSON data under `Data/`.

This is the structure implemented in the repository and it is the chosen architecture for Week 1.

## Alternatives Considered

1. Single-project structure (all code in one project)
   - Pros: simpler to navigate for very small demos; fewer project references to manage.
   - Cons: domain, persistence, and UI code mix together; harder to demonstrate layered separation and responsibilities to learners.

2. Hexagonal / Ports-and-Adapters style with explicit ports and adapters per use case
   - Pros: stronger isolation of side effects and clearer test boundaries for I/O.
   - Cons: adds conceptual overhead and more artifacts (ports, adapters) that are unnecessary for a small lab; the current layered split already achieves separation of concerns appropriate for the scope.

## Consequences

- Benefits
  - Clear separation of responsibilities (domain logic isolated from persistence and UI).
  - Easier to read and teach: students can focus on one layer at a time.
  - Swapping persistence implementations or adding a different UI is straightforward because application services depend on abstractions (`IRmsRepository`).

- Trade-offs / Limitations
  - More projects increases initial setup complexity for newcomers.
  - Some boilerplate (project references, DI registration) is required.
  - The architecture is intentionally lightweight and not a full enterprise-grade layering; further structural patterns (e.g., ports & adapters) could be adopted later.

This ADR documents the implemented decision and the pedagogical reasons for choosing the layered structure for this lab project.