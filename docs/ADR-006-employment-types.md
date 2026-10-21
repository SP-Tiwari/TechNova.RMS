# ADR-006: Employment type represented as a domain enum

- **Status:** Accepted

## Context

The Resource Management System needs to record how each employee is engaged, for example as a permanent or contract employee. This value is part of an employee's domain state and is persisted with employee data. The representation must be constrained to supported values, easy to validate, readable in application code, and compatible with the existing JSON format.

## Decision

Represent employment type with the `EmploymentType` enum in `TechNova.RMS.Domain` and store it as part of the `Employee` domain model.

The enum is used by the `Employee` constructor, update operations, console input validation, and JSON serialization. The JSON repository uses `JsonStringEnumConverter`, so persisted values remain readable names rather than numeric enum values.

The current supported values are the values defined by the domain enum. Invalid numeric values entered through the console are rejected before an employee is created or updated.

## Alternatives Considered

1. Store employment type as a free-form string
   - Pros: simple to extend without changing the enum.
   - Cons: allows inconsistent values and spelling variations, making validation and reporting less reliable.

2. Store employment type as an integer
   - Pros: compact representation and easy numeric input.
   - Cons: less readable in source code and JSON, and invalid numeric values are easier to persist accidentally.

3. Create a separate employment-type entity or lookup table
   - Pros: supports dynamically managed types and additional metadata.
   - Cons: introduces unnecessary persistence and relationship complexity for the current small, fixed set of domain values.

## Consequences

- Benefits
  - The domain model accepts only the defined employment categories.
  - Business code is more readable than comparisons against string or numeric literals.
  - Console validation can reject unsupported values before persistence.
  - JSON values remain human-readable through `JsonStringEnumConverter`.

- Trade-offs / Limitations
  - Adding a new employment category requires a code change and updated seed or persisted data where applicable.
  - Enums are appropriate for a stable set of values but would be less suitable if employment types needed to be configured by users at runtime.

This ADR records the implemented representation of employment type and its validation and persistence implications.
