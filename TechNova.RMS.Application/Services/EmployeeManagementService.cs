namespace TechNova.RMS.Application.Services;

public sealed class EmployeeManagementService(IRmsRepository repository)
{
	/// <summary>
	/// Adds a new employee to the repository after checking for duplicate IDs.
	/// </summary>
	public void AddEmployee(Employee employee)
	{
		var employees = repository.GetEmployees();

		if (employees.Any(item =>
			string.Equals(item.Id, employee.Id,
				StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException(
				$"Employee '{employee.Id}' already exists.");
		}

		employees.Add(employee);
		repository.SaveEmployees(employees);
	}

	/// <summary>
	/// Updates an existing employee's details and persists changes.
	/// Throws <see cref="InvalidOperationException"/> if the employee is not found.
	/// </summary>
	/// <param name="employeeId">Identifier of the employee to update.</param>
	/// <param name="fullName">New full name.</param>
	/// <param name="email">New email.</param>
	/// <param name="designation">New designation.</param>
	/// <param name="department">New department.</param>
	/// <param name="costRate">New cost rate.</param>
	/// <param name="employmentType">New employment type.</param>
	public void UpdateEmployee(
		string employeeId,
		string fullName,
		string email,
		string designation,
		string department,
		decimal costRate,
		EmploymentType employmentType)
	{
		var employees = repository.GetEmployees();

		var employee = employees.FirstOrDefault(item =>
			string.Equals(item.Id, employeeId,
				StringComparison.OrdinalIgnoreCase));

		if (employee is null)
		{
			throw new InvalidOperationException(
				$"Employee '{employeeId}' was not found.");
		}

		employee.UpdateDetails(
			fullName,
			email,
			designation,
			department,
			costRate,
			employmentType);

		repository.SaveEmployees(employees);
	}

	public void DeactivateEmployee(string employeeId)
	{
		/// <summary>
		/// Deactivates the specified employee and persists the change.
		/// Throws <see cref="InvalidOperationException"/> if the employee does not exist.
		/// </summary>
		var employees = repository.GetEmployees();

		var employee = employees.FirstOrDefault(item =>
			string.Equals(item.Id, employeeId,
				StringComparison.OrdinalIgnoreCase));

		if (employee is null)
		{
			throw new InvalidOperationException(
				$"Employee '{employeeId}' was not found.");
		}

		employee.Deactivate();
		repository.SaveEmployees(employees);
	}
}
