namespace TechNova.RMS.Application.Services;

public sealed class ReportingService
{
	/// <summary>
	/// Produces a bench report listing active employees whose allocated percentage on a specific date is below the threshold.
	/// </summary>
	/// <param name="employees">All employees to consider.</param>
	/// <param name="allocations">All allocations to calculate percentages from.</param>
	/// <param name="reportDate">The date for which allocated percentage is calculated.</param>
	/// <param name="thresholdPercentage">Employees with allocated percentage below this value are returned.</param>
	public IReadOnlyList<BenchReportRow> GetBenchReport(
		IEnumerable<Employee> employees,
		IEnumerable<Allocation> allocations,
		DateOnly reportDate,
		decimal thresholdPercentage)
	{
		if (thresholdPercentage is < 0 or > 100)
		{
			throw new ArgumentOutOfRangeException(
				nameof(thresholdPercentage),
				"Threshold must be between 0 and 100.");
		}

		var employeeList = employees.ToList();
		var allocationList = allocations.ToList();

		var report = employeeList
			.Where(employee => employee.IsActive)
			.Select(employee =>
			{
				decimal allocatedPercentage = allocationList
					.Where(allocation =>
						string.Equals(
							allocation.EmployeeId,
							employee.Id,
							StringComparison.OrdinalIgnoreCase))
					.Where(allocation =>
						allocation.StartDate <= reportDate &&
						allocation.EndDate >= reportDate)
					.Sum(allocation => allocation.AllocationPercentage);

				return new BenchReportRow(
					employee.Id,
					employee.FullName,
					employee.Department,
					allocatedPercentage,
					Math.Max(0m, 100m - allocatedPercentage));
			})
			.Where(row =>
				row.AllocatedPercentage < thresholdPercentage)
			.ToList();

		return report;
	}


	/// <summary>
	/// Returns staffing summary for each project including assigned employee count and total allocation percentage.
	/// </summary>
	public IReadOnlyList<ProjectStaffingRow> GetProjectStaffingSummary(
		IEnumerable<TechNova.RMS.Domain.Projects.Project> projects,
		IEnumerable<TechNova.RMS.Domain.Allocations.Allocation> allocations)
	{
		var projectList = projects.ToList();
		var allocationList = allocations.ToList();

		return projectList
			.Select(project =>
			{
				var projectAllocations = allocationList
					.Where(allocation =>
						string.Equals(
							allocation.ProjectId,
							project.Id,
							StringComparison.OrdinalIgnoreCase))
					.ToList();

				var employeeCount = projectAllocations
					.Select(allocation => allocation.EmployeeId)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.Count();

				var totalAllocation = projectAllocations
					.Sum(allocation => allocation.AllocationPercentage);

				return new ProjectStaffingRow(
					project.Id,
					project.Name,
					project.ClientName,
					project.Status.ToString(),
					project.StartDate,
					project.EndDate,
					employeeCount,
					totalAllocation);
			}).ToList();

	}

	/// <summary>
	/// Computes department-level utilisation for the given report date.
	/// </summary>
	public IReadOnlyList<DepartmentUtilisationRow> GetDepartmentUtilisation(
	IEnumerable<Employee> employees,
	IEnumerable<Allocation> allocations,
	DateOnly reportDate)
	{
		var activeEmployees = employees
			.Where(employee => employee.IsActive)
			.ToList();

		var allocationList = allocations.ToList();

		return activeEmployees
			.GroupBy(employee => employee.Department,
				StringComparer.OrdinalIgnoreCase)
			.Select(group =>
			{
				var employeeIds = group
					.Select(employee => employee.Id)
					.ToHashSet(StringComparer.OrdinalIgnoreCase);

				decimal allocated = allocationList
					.Where(allocation => employeeIds.Contains(allocation.EmployeeId))
					.Where(allocation =>
						allocation.StartDate <= reportDate &&
						allocation.EndDate >= reportDate)
					.Sum(allocation => allocation.AllocationPercentage);

				int employeeCount = group.Count();
				decimal capacity = employeeCount * 100m;

				return new DepartmentUtilisationRow(
					group.Key,
					employeeCount,
					allocated,
					capacity == 0 ? 0 : allocated / capacity * 100m);
			})
			.OrderBy(row => row.Department)
			.ToList();
	}

}

public sealed record DepartmentUtilisationRow(
	string Department,
	int ActiveEmployeeCount,
	decimal TotalAllocatedPercentage,
	decimal UtilisationPercentage);

public sealed record BenchReportRow(
	string EmployeeId,
	string EmployeeName,
	string Department,
	decimal AllocatedPercentage,
	decimal AvailablePercentage);


public sealed record ProjectStaffingRow(
	string ProjectId,
	string ProjectName,
	string ClientName,
	string Status,
	DateOnly StartDate,
	DateOnly EndDate,
	int AssignedEmployeeCount,
	decimal TotalAllocationPercentage);

