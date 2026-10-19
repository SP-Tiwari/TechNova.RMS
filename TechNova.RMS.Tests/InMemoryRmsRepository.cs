namespace TechNova.RMS.Tests;

public class InMemoryRmsRepository : IRmsRepository
{
	private readonly List<Employee> _employees = new();
	private readonly List<Project> _projects = new();
	private readonly List<Allocation> _allocations = new();

	public InMemoryRmsRepository()
	{
	}

	public Task<List<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(_employees.Select(e => e).ToList());

	public Task<List<Project>> GetProjectsAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(_projects.Select(p => p).ToList());

	public Task<List<Allocation>> GetAllocationsAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(_allocations.Select(a => a).ToList());

	public Task SaveEmployeesAsync(IEnumerable<Employee> employees, CancellationToken cancellationToken = default)
	{
		_employees.Clear();
		_employees.AddRange(employees);
		return Task.CompletedTask;
	}

	public Task SaveProjectsAsync(IEnumerable<Project> projects, CancellationToken cancellationToken = default)
	{
		_projects.Clear();
		_projects.AddRange(projects);
		return Task.CompletedTask;
	}

	public Task SaveAllocationsAsync(IEnumerable<Allocation> allocations, CancellationToken cancellationToken = default)
	{
		_allocations.Clear();
		_allocations.AddRange(allocations);
		return Task.CompletedTask;
	}

	// Helpers for tests
	public Task SeedEmployeesAsync(IEnumerable<Employee> employees)
	{
		_employees.Clear();
		_employees.AddRange(employees);
		return Task.CompletedTask;
	}

	public Task SeedProjectsAsync(IEnumerable<Project> projects)
	{
		_projects.Clear();
		_projects.AddRange(projects);
		return Task.CompletedTask;
	}

	public Task SeedAllocationsAsync(IEnumerable<Allocation> allocations)
	{
		_allocations.Clear();
		_allocations.AddRange(allocations);
		return Task.CompletedTask;
	}
}
