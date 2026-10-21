# TechNova RMS Architecture

This diagram reflects the current project references and runtime composition. Solid arrows show compile-time project dependencies; the dashed arrow shows the runtime call from the console UI through the application services to the repository abstraction and JSON implementation.

```mermaid
flowchart TD
	Console[TechNova.RMS.Console<br/>Program and ConsoleMenu]
	Application[TechNova.RMS.Application<br/>Application services and IRmsRepository]
	Domain[TechNova.RMS.Domain<br/>Employee, Project, Allocation, domain validation]
	Infrastructure[TechNova.RMS.Infrastructure<br/>JsonRmsRepository and DTO mapping]
	Data[(TechNova.RMS.Console/Data<br/>employees.json, projects.json, allocations.json)]

	Console -->|project reference| Application
	Console -->|project reference| Infrastructure
	Application -->|project reference| Domain
	Infrastructure -->|project reference| Application
	Infrastructure -->|project reference| Domain
	Infrastructure -->|runtime file I/O| Data
	Console -.->|resolves services and calls RunAsync| Application
	Application -.->|IRmsRepository calls| Infrastructure
```

## Dependency and lifetime notes

- `Program.cs` is the composition root and registers `JsonRmsRepository`, application services, the domain `AllocationService`, and `ConsoleMenu`.
- The application layer owns `IRmsRepository`; Infrastructure implements it.
- All registrations are singleton because this is a single-process console application, the repository owns one in-process `SemaphoreSlim`, and the services are stateless coordinators. A web application would normally use scoped lifetimes and a database-backed repository instead.
- `JsonRmsRepository` resolves its data directory from `AppContext.BaseDirectory`, so it does not depend on the process current working directory.
