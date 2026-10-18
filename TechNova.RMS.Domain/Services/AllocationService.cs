namespace TechNova.RMS.Domain.Services;

public sealed class AllocationService
{
	/// <summary>
	/// Validates an allocation against business rules.
	/// Checks that the employee is active, that allocation IDs match the supplied employee and project,
	/// that allocation dates fall within the project dates, and that the per-day allocation totals do not exceed 100%.
	/// </summary>
	/// <param name="employee">The employee to allocate.</param>
	/// <param name="project">The project being allocated to.</param>
	/// <param name="newAllocation">The allocation to validate.</param>
	/// <param name="existingAllocations">Existing allocations to consider when checking overlaps.</param>
	/// <exception cref="InvalidOperationException">Thrown when a business rule is violated.</exception>
	public void ValidateAllocation(
		Employee employee,
		Project project,
		Allocation newAllocation,
		IEnumerable<Allocation> existingAllocations)
	{
		if (!employee.IsActive)
		{
			throw new InvalidOperationException(
				"Inactive employees cannot be allocated.");
		}

		if (!string.Equals(
				employee.Id,
				newAllocation.EmployeeId,
				StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException(
				"Allocation employee ID does not match the employee.");
		}

		if (!string.Equals(
				project.Id,
				newAllocation.ProjectId,
				StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException(
				"Allocation project ID does not match the project.");
		}

		if (newAllocation.StartDate < project.StartDate ||
			newAllocation.EndDate > project.EndDate)
		{
			throw new InvalidOperationException(
				"Allocation dates must fall within the project dates.");
		}


		var employeeAllocations = existingAllocations
			.Where(allocation =>
				string.Equals(
					allocation.EmployeeId,
					employee.Id,
					StringComparison.OrdinalIgnoreCase))
			.ToList();

		for (var date = newAllocation.StartDate;
			 date <= newAllocation.EndDate;
			 date = date.AddDays(1))
		{
			decimal existingPercentage = employeeAllocations
				.Where(allocation =>
					allocation.StartDate <= date &&
					allocation.EndDate >= date)
				.Sum(allocation => allocation.AllocationPercentage);

			decimal totalPercentage =
				existingPercentage + newAllocation.AllocationPercentage;

			if (totalPercentage > 100m)
			{
				throw new InvalidOperationException(
					$"Allocation would exceed 100% on {date:yyyy-MM-dd}. " +
					$"Total would be {totalPercentage}%.");
			}
		}
	}
}