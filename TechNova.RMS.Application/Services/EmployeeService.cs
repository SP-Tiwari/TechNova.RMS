
using TechNova.RMS.Application.Persistence;

namespace TechNova.RMS.Application.Services;

public class EmployeeService(IRmsRepository repository)
{
	public List<string> GetEmployeeSummaries()
	{
		return repository.GetEmployees()
			.Select(employee =>
				$"{employee.Id} | {employee.FullName} | " +
				$"{employee.Designation} | {employee.Department} | " +
				$"{(employee.IsActive ? "Active" : "Inactive")}")
			.ToList();
	}
}
