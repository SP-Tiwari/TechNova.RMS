namespace TechNova.RMS.Application.Services;

public sealed class EmployeeManagementService(IRmsRepository repository)
{
	/// <summary>
	/// Adds a new employee to the repository after checking for duplicate IDs.
	/// </summary>
	public async Task AddEmployeeAsync(Employee employee, CancellationToken cancellationToken = default)
	{
		var employees = await repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);

		if (employees.Any(item =>
			string.Equals(item.Id, employee.Id,
				StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException(
				$"Employee '{employee.Id}' already exists.");
		}

		employees.Add(employee);
		await repository.SaveEmployeesAsync(employees, cancellationToken).ConfigureAwait(false);
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
	public async Task UpdateEmployeeAsync(
		string employeeId,
		string fullName,
		string email,
		string designation,
		string department,
		decimal costRate,
		EmploymentType employmentType)
	{
		var employees = await repository.GetEmployeesAsync().ConfigureAwait(false);

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

		await repository.SaveEmployeesAsync(employees).ConfigureAwait(false);
	}

	public async Task DeactivateEmployeeAsync(string employeeId, CancellationToken cancellationToken = default)
	{
		/// <summary>
		/// Deactivates the specified employee and persists the change.
		/// Throws <see cref="InvalidOperationException"/> if the employee does not exist.
		/// </summary>
		var employees = await repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);

		var employee = employees.FirstOrDefault(item =>
			string.Equals(item.Id, employeeId,
				StringComparison.OrdinalIgnoreCase));

		if (employee is null)
		{
			throw new InvalidOperationException(
				$"Employee '{employeeId}' was not found.");
		}

		employee.Deactivate();
		await repository.SaveEmployeesAsync(employees, cancellationToken).ConfigureAwait(false);
	}
}
