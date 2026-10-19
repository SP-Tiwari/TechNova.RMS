namespace TechNova.RMS.Application.Services;

public class EmployeeService(IRmsRepository repository)
{
	/// <summary>
	/// Returns formatted employee summary lines for display in the UI.
	/// </summary>
	public async Task<List<string>> GetEmployeeSummariesAsync(CancellationToken cancellationToken = default)
	{
		var employees = await repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);
		return employees
			.Select(employee =>
				$"{employee.Id} | {employee.FullName} | " +
				$"{employee.Designation} | {employee.Department} | " +
				$"{(employee.IsActive ? "Active" : "Inactive")}")
			.ToList();
	}
}
