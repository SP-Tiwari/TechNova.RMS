# ADR-002: Domain model choices (Employee, Project, Allocation, Skills)

- **Status:** Accepted

## Context

The project models employees, projects, and allocations. Domain types must capture identity, required validation rules, and simple behaviours appropriate for allocation logic and matching.

The repository contains the following domain types (implemented under `TechNova.RMS.Domain`):

- `Employee` and `EmployeeSkill` (with `EmploymentType` enum)
- `Project` and `ProjectRequiredSkill` (with `ProjectStatus` enum)
- `Allocation`
- `AllocationService` (domain validation)

Decisions were needed about what each domain object owns, what behaviours belong to them, and how to represent skills and proficiencies.

## Decision

The implemented domain model follows these choices:

- `Employee` is an aggregate that maintains its own state and behaviours: identity, full name, email, designation, department, cost rate, employment type, an `IsActive` flag, and an internal list of `EmployeeSkill` instances. `Employee` provides methods to add/remove skills, update details, and deactivate itself. It validates required fields (non-empty id, name, email, etc.).

- `EmployeeSkill` is a value-like object carrying `SkillName` and `ProficiencyLevel`. The implementation enforces proficiency between 1 and 5.

- `Project` is an aggregate that stores identity, client, dates, status, and required skills. It provides `AddRequiredSkill` and `UpdateDetails` with validations (non-empty fields, `EndDate` not before `StartDate`).

- `ProjectRequiredSkill` mirrors `EmployeeSkill` for required skills and enforces minimum proficiency bounds.

- `Allocation` is a small immutable object carrying allocation metadata (ids, dates, allocation percentage); its constructor enforces valid date ranges and allocation percentage > 0 and <= 100.

- `AllocationService` encapsulates allocation validation rules that require knowledge of an employee, a project, the new allocation, and existing allocations; it throws exceptions when business rules are violated.

These implementations are present in the `TechNova.RMS.Domain` project and used by application services.

## Alternatives Considered

1. Make skills simple strings (no typed `EmployeeSkill`)
   - Pros: simpler structure, less code.
   - Cons: loses validation of proficiency and skill-specific behaviour; harder to enforce rules when matching.

2. Model allocations as entities inside `Employee` or `Project` aggregates
   - Pros: closer coupling of allocations to the owning aggregate.
   - Cons: complicates in-memory composition and persistence; the chosen approach keeps `Allocation` as a first-class domain type and lets application services coordinate persistence and listing.

## Consequences

- Benefits
  - Domain objects encapsulate their own invariants and validation (e.g., skill ranges, required fields, date checks).
  - Clear separation of concerns: `AllocationService` contains cross-entity allocation rules while `Employee` and `Project` maintain their internal consistency.
  - Easy to reason about when matching employees to projects since required skills and proficiencies are modelled explicitly.

- Trade-offs / Limitations
  - The model is intentionally compact for a lab: aggregates are sealed and designed for correctness rather than extensibility via inheritance.
  - Keeping allocations as separate entities requires application services to coordinate lookups and persistence.

This ADR records the implemented domain model choices and the rationale appropriate to the lab's objectives.