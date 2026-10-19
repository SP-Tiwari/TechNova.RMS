namespace TechNova.RMS.Application.Services;

public sealed class AllocationApplicationService(
	IRmsRepository repository,
	AllocationService allocationService)
{
	/// <summary>
	/// Applies a new allocation by validating domain rules and persisting the allocation.
	/// Writes a console notification on success.
	/// </summary>
	/// <param name="newAllocation">The allocation to apply.</param>
	public async Task AllocateAsync(Allocation newAllocation, CancellationToken cancellationToken = default)
	{
		var employees = await repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);
		var projects = await repository.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
		var allocations = await repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);

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
		await repository.SaveAllocationsAsync(allocations, cancellationToken).ConfigureAwait(false);

		Console.WriteLine(
			$"Notification: {employee.FullName} allocated to {project.Name}.");
	}


	public async Task ReleaseAsync(string allocationId, CancellationToken cancellationToken = default)
	{
		/// <summary>
		/// Releases (removes) an allocation by identifier and persists the change.
		/// Writes a console notification on success.
		/// </summary>
		/// <param name="allocationId">Identifier of the allocation to release.</param>
		var allocations = await repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);

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

		var employees = await repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);

		var employee = employees.FirstOrDefault(item =>
			string.Equals(
				item.Id,
				allocation.EmployeeId,
				StringComparison.OrdinalIgnoreCase));

		allocations.Remove(allocation);
		await repository.SaveAllocationsAsync(allocations, cancellationToken).ConfigureAwait(false);

		Console.WriteLine(
			$"Notification: Allocation released for " +
			$"{employee?.FullName ?? allocation.EmployeeId} " +
			$"from project {allocation.ProjectId}.");
	}
}
