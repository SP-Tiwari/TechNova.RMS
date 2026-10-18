namespace TechNova.RMS.Application.Services;

public sealed class EmployeeMatchingService
{
	public IReadOnlyList<Employee> FindMatchingEmployees(
		Project project,
		IEnumerable<Employee> employees,
		IEnumerable<Allocation> allocations)
	{
		var employeeList = employees.ToList();
		var allocationList = allocations.ToList();

		return employeeList
			.Where(employee => employee.IsActive)
			.Where(employee => HasRequiredSkills(employee, project))
			.Where(employee => IsAvailable(
				employee,
				project,
				allocationList))
			.ToList();
	}

	private static bool HasRequiredSkills(
		Employee employee,
		Project project)
	{
		return project.RequiredSkills.All(requiredSkill =>
			employee.Skills.Any(employeeSkill =>
				string.Equals(
					employeeSkill.SkillName,
					requiredSkill.SkillName,
					StringComparison.OrdinalIgnoreCase)
				&& employeeSkill.ProficiencyLevel
					>= requiredSkill.MinimumProficiencyLevel));
	}

	private static bool IsAvailable(
		Employee employee,
		Project project,
		IEnumerable<Allocation> allocations)
	{
		var employeeAllocations = allocations
			.Where(allocation =>
				string.Equals(
					allocation.EmployeeId,
					employee.Id,
					StringComparison.OrdinalIgnoreCase))
			.Where(allocation =>
				allocation.StartDate <= project.EndDate
				&& allocation.EndDate >= project.StartDate)
			.ToList();

		for (var date = project.StartDate;
			 date <= project.EndDate;
			 date = date.AddDays(1))
		{
			decimal allocatedPercentage = employeeAllocations
				.Where(allocation =>
					allocation.StartDate <= date
					&& allocation.EndDate >= date)
				.Sum(allocation => allocation.AllocationPercentage);

			if (allocatedPercentage >= 100m)
			{
				return false;
			}
		}

		return true;
	}
}
