
namespace TechNova.RMS.Application.Persistence;

public interface IRmsRepository
{
	/// <summary>
	/// Returns the list of employees from persistent storage.
	/// </summary>
	List<Employee> GetEmployees();

	/// <summary>
	/// Returns the list of projects from persistent storage.
	/// </summary>
	List<Project> GetProjects();

	/// <summary>
	/// Returns the list of allocations from persistent storage.
	/// </summary>
	List<Allocation> GetAllocations();

	/// <summary>
	/// Persist the employee list to storage.
	/// </summary>
	void SaveEmployees(IEnumerable<Employee> employees);

	/// <summary>
	/// Persist the project list to storage.
	/// </summary>
	void SaveProjects(IEnumerable<Project> projects);

	/// <summary>
	/// Persist the allocation list to storage.
	/// </summary>
	void SaveAllocations(IEnumerable<Allocation> allocations);
}
