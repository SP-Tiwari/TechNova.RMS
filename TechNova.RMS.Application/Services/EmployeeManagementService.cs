
using TechNova.RMS.Application.Persistence;
using TechNova.RMS.Domain.Employees;

namespace TechNova.RMS.Application.Services;

public sealed class EmployeeManagementService(IRmsRepository repository)
{
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
