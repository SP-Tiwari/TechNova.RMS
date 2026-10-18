namespace TechNova.RMS.Application.Services;

public sealed class AllocationApplicationService(
	IRmsRepository repository,
	AllocationService allocationService)
{
	public void Allocate(Allocation newAllocation)
	{
		var employees = repository.GetEmployees();
		var projects = repository.GetProjects();
		var allocations = repository.GetAllocations();

		var employee = employees.FirstOrDefault(item =>
			string.Equals(
				item.Id,
				newAllocation.EmployeeId,
				StringComparison.OrdinalIgnoreCase));

		if (employee is null)
		{
			throw new InvalidOperationException(
				$"Employee '{newAllocation.EmployeeId}' was not found.");
		}

		var project = projects.FirstOrDefault(item =>
			string.Equals(
				item.Id,
				newAllocation.ProjectId,
				StringComparison.OrdinalIgnoreCase));

		if (project is null)
		{
			throw new InvalidOperationException(
				$"Project '{newAllocation.ProjectId}' was not found.");
		}

		allocationService.ValidateAllocation(
			employee,
			project,
			newAllocation,
			allocations);

		allocations.Add(newAllocation);
		repository.SaveAllocations(allocations);

		Console.WriteLine(
			$"Notification: {employee.FullName} allocated to {project.Name}.");
	}


	public void Release(string allocationId)
	{
		var allocations = repository.GetAllocations();

		var allocation = allocations.FirstOrDefault(item =>
			string.Equals(
				item.Id,
				allocationId,
				StringComparison.OrdinalIgnoreCase));

		if (allocation is null)
		{
			throw new InvalidOperationException(
				$"Allocation '{allocationId}' was not found.");
		}

		var employees = repository.GetEmployees();

		var employee = employees.FirstOrDefault(item =>
			string.Equals(
				item.Id,
				allocation.EmployeeId,
				StringComparison.OrdinalIgnoreCase));

		allocations.Remove(allocation);
		repository.SaveAllocations(allocations);

		Console.WriteLine(
			$"Notification: Allocation released for " +
			$"{employee?.FullName ?? allocation.EmployeeId} " +
			$"from project {allocation.ProjectId}.");
	}
}
