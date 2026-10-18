# ADR-003: Validation and business rules (allocations, active employees, dates)

- **Status:** Accepted

## Context

Allocation rules are critical to prevent overallocation and invalid assignments. The system must ensure allocations respect employee activity status, do not exceed capacity, and fall within project dates.

The repository contains `AllocationService` in `TechNova.RMS.Domain.Services` and application-level callers (`AllocationApplicationService`) that use it.

## Decision

Validation responsibilities are split as follows:

- Domain-level validation
  - Constructors and methods on domain types (`Employee`, `Project`, `Allocation`, `EmployeeSkill`, `ProjectRequiredSkill`) validate basic invariants (non-empty IDs, valid dates, valid skill proficiency ranges, allocation percentage bounds).

- Cross-entity business rules
  - `AllocationService.ValidateAllocation(Employee employee, Project project, Allocation newAllocation, IEnumerable<Allocation> existingAllocations)` enforces:
	- The employee must be active (`employee.IsActive`).
	- The allocation's `EmployeeId` and `ProjectId` must match the supplied `Employee` and `Project`.
	- The allocation's dates must fall within the project's start and end dates.
	- For each date in the allocation range, the sum of existing allocations for that employee plus the new allocation's percentage must not exceed 100% (per-day check).

- Application-level coordination
  - `AllocationApplicationService` locates employee/project/allocations from `IRmsRepository`, then calls `allocationService.ValidateAllocation(...)` and persists allocations if validation passes. It reports via `Console.WriteLine` for notification.

This is the actual behaviour implemented in the project.

## Alternatives Considered

1. Validate only at persistence time (repository)
   - Pros: single place for enforcing allocation constraints before saving.
   - Cons: breaks domain-driven validation; callers would not get consistent exceptions tied to domain intent and it would mix persistence concerns with business rules.

2. Use time-window aggregation instead of per-day checks (e.g., weeks or months)
   - Pros: more efficient for long allocations.
   - Cons: less precise and could allow temporary daily over-allocation; the per-day check implemented is conservative and clear for the lab.

## Consequences

- Benefits
  - Clear separation: domain types guard local invariants; `AllocationService` enforces cross-entity rules.
  - Predictable, testable validation points: callers get exceptions with clear messages when validation fails.

- Trade-offs / Limitations
  - Per-day validation iterates dates and may be inefficient for long multi-year allocations, but it is acceptable for the lab's scale and seed data.
  - The validation throws exceptions to signal errors; a production system might prefer a richer error model or validation result objects.

This ADR documents the chosen, implemented validation strategy and why it suits the project scope.