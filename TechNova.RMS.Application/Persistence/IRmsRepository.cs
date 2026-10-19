
namespace TechNova.RMS.Application.Persistence;

public interface IRmsRepository
{
	/// <summary>
	/// Returns the list of employees from persistent storage.
	/// </summary>
	Task<List<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the list of projects from persistent storage.
	/// </summary>
	Task<List<Project>> GetProjectsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the list of allocations from persistent storage.
	/// </summary>
	Task<List<Allocation>> GetAllocationsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Persist the employee list to storage.
	/// </summary>
	Task SaveEmployeesAsync(IEnumerable<Employee> employees, CancellationToken cancellationToken = default);

	/// <summary>
	/// Persist the project list to storage.
	/// </summary>
	Task SaveProjectsAsync(IEnumerable<Project> projects, CancellationToken cancellationToken = default);

	/// <summary>
	/// Persist the allocation list to storage.
	/// </summary>
	Task SaveAllocationsAsync(IEnumerable<Allocation> allocations, CancellationToken cancellationToken = default);
}
