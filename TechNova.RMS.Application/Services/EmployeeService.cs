namespace TechNova.RMS.Application.Services;

public class EmployeeService(IRmsRepository repository)
{
	/// <summary>
	/// Returns formatted employee summary lines for display in the UI.
	/// </summary>
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
