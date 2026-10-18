namespace TechNova.RMS.Domain.Allocations;

public sealed class Allocation
{
	public string Id { get; }

	public string EmployeeId { get; }

	public string ProjectId { get; }

	public DateOnly StartDate { get; }

	public DateOnly EndDate { get; }

	public decimal AllocationPercentage { get; }

	public Allocation(
		string id,
		string employeeId,
		string projectId,
		DateOnly startDate,
		DateOnly endDate,
		decimal allocationPercentage)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException(
				"Allocation ID is required.",
				nameof(id));
		}

		if (string.IsNullOrWhiteSpace(employeeId))
		{
			throw new ArgumentException(
				"Employee ID is required.",
				nameof(employeeId));
		}

		if (string.IsNullOrWhiteSpace(projectId))
		{
			throw new ArgumentException(
				"Project ID is required.",
				nameof(projectId));
		}

		if (endDate < startDate)
		{
			throw new ArgumentException(
				"Allocation end date cannot be before its start date.");
		}

		if (allocationPercentage is <= 0 or > 100)
		{
			throw new ArgumentOutOfRangeException(
				nameof(allocationPercentage),
				"Allocation percentage must be greater than 0 and no more than 100.");
		}

		Id = id.Trim();
		EmployeeId = employeeId.Trim();
		ProjectId = projectId.Trim();
		StartDate = startDate;
		EndDate = endDate;
		AllocationPercentage = allocationPercentage;
	}
}