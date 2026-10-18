
namespace TechNova.RMS.Application.Persistence;

public interface IRmsRepository
{
	List<Employee> GetEmployees();

	List<Project> GetProjects();

	List<Allocation> GetAllocations();

	void SaveEmployees(IEnumerable<Employee> employees);

	void SaveProjects(IEnumerable<Project> projects);

	void SaveAllocations(IEnumerable<Allocation> allocations);
}
