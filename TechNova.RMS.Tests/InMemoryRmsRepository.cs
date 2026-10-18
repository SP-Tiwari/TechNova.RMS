namespace TechNova.RMS.Tests;

public class InMemoryRmsRepository : IRmsRepository
{
	private readonly List<Employee> _employees = new();
	private readonly List<Project> _projects = new();
	private readonly List<Allocation> _allocations = new();

	public InMemoryRmsRepository()
	{
	}

	public List<Employee> GetEmployees() => _employees.Select(e => e).ToList();

	public List<Project> GetProjects() => _projects.Select(p => p).ToList();

	public List<Allocation> GetAllocations() => _allocations.Select(a => a).ToList();

	public void SaveEmployees(IEnumerable<Employee> employees)
	{
		_employees.Clear();
		_employees.AddRange(employees);
	}

	public void SaveProjects(IEnumerable<Project> projects)
	{
		_projects.Clear();
		_projects.AddRange(projects);
	}

	public void SaveAllocations(IEnumerable<Allocation> allocations)
	{
		_allocations.Clear();
		_allocations.AddRange(allocations);
	}

	// Helpers for tests
	public void SeedEmployees(IEnumerable<Employee> employees) => SaveEmployees(employees);
	public void SeedProjects(IEnumerable<Project> projects) => SaveProjects(projects);
	public void SeedAllocations(IEnumerable<Allocation> allocations) => SaveAllocations(allocations);
}
