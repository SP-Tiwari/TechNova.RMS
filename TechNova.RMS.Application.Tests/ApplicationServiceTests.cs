namespace TechNova.RMS.Application.Tests;

public sealed class EmployeeManagementServiceTests
{
	[Fact]
	public async Task AddUpdateAndDeactivateEmployee_PersistChanges()
	{
		var repository = new InMemoryRmsRepository();
		var service = new EmployeeManagementService(repository);
		var employee = CreateEmployee("E1");

		await service.AddEmployeeAsync(employee);
		await service.UpdateEmployeeAsync("E1", "Updated", "updated@example.com", "Lead", "Platform", 200m, EmploymentType.Contract);
		await service.DeactivateEmployeeAsync("E1");

		var saved = Assert.Single(await repository.GetEmployeesAsync());
		Assert.Equal("Updated", saved.FullName);
		Assert.Equal("Platform", saved.Department);
		Assert.Equal(EmploymentType.Contract, saved.EmploymentType);
		Assert.False(saved.IsActive);
	}

	[Fact]
	public async Task AddEmployee_DuplicateId_Throws()
	{
		var repository = new InMemoryRmsRepository();
		var service = new EmployeeManagementService(repository);
		await service.AddEmployeeAsync(CreateEmployee("E1"));

		await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddEmployeeAsync(CreateEmployee("e1")));
	}

	[Fact]
	public async Task UpdateEmployee_MissingEmployee_Throws()
	{
		var service = new EmployeeManagementService(new InMemoryRmsRepository());

		await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateEmployeeAsync("missing", "Name", "email", "Dev", "Dept", 1m, EmploymentType.Permanent));
	}

	[Fact]
	public async Task EmployeeService_ReturnsFormattedSummaries()
	{
		var repository = new InMemoryRmsRepository();
		await repository.SeedEmployeesAsync(CreateEmployee("E1"));
		var employee = CreateEmployee("E2");
		employee.Deactivate();
		await repository.SeedEmployeesAsync(CreateEmployee("E1"), employee);

		var summaries = await new EmployeeService(repository).GetEmployeeSummariesAsync();

		Assert.Equal(["E1 | Name | Dev | Dept | Active", "E2 | Name | Dev | Dept | Inactive"], summaries);
	}

	private static Employee CreateEmployee(string id) => new(id, "Name", $"{id}@example.com", "Dev", "Dept", 100m, EmploymentType.Permanent);
}

public sealed class ProjectManagementServiceTests
{
	[Fact]
	public async Task AddProject_PersistsProject()
	{
		var repository = new InMemoryRmsRepository();
		var project = CreateProject("P1");

		await new ProjectManagementService(repository).AddProjectAsync(project);

		Assert.Same(project, Assert.Single(await repository.GetProjectsAsync()));
	}

	[Fact]
	public async Task AddProject_DuplicateId_Throws()
	{
		var repository = new InMemoryRmsRepository();
		await repository.SeedProjectsAsync(CreateProject("P1"));

		await Assert.ThrowsAsync<InvalidOperationException>(() => new ProjectManagementService(repository).AddProjectAsync(CreateProject("p1")));
	}

	private static Project CreateProject(string id) => new(id, "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
}

public sealed class EmployeeMatchingServiceTests
{
	[Fact]
	public void FindMatchingEmployees_RequiresSkillsAndAvailability()
	{
		var project = new Project("P1", "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
		project.AddRequiredSkill("C#", 4);
		var available = CreateEmployee("E1");
		available.AddSkill("C#", 4);
		var busy = CreateEmployee("E2");
		busy.AddSkill("C#", 5);
		var allocations = new[] { new Allocation("A1", busy.Id, project.Id, project.StartDate, project.EndDate, 100m) };

		var matches = new EmployeeMatchingService().FindMatchingEmployees(project, [available, busy], allocations);

		Assert.Equal([available], matches);
	}

	private static Employee CreateEmployee(string id) => new(id, "Name", $"{id}@example.com", "Dev", "Dept", 100m, EmploymentType.Permanent);
}

public sealed class ReportingServiceTests
{
	[Fact]
	public void Reports_CalculateBenchStaffingAndDepartmentUtilisation()
	{
		var reportDate = new DateOnly(2026, 1, 10);
		var employee = new Employee("E1", "Name", "e1@example.com", "Dev", "Platform", 100m, EmploymentType.Permanent);
		var inactive = new Employee("E2", "Inactive", "e2@example.com", "Dev", "Platform", 100m, EmploymentType.Permanent);
		inactive.Deactivate();
		var project = new Project("P1", "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), ProjectStatus.Active);
		var allocation = new Allocation("A1", employee.Id, project.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 40m);
		var service = new ReportingService();

		var bench = service.GetBenchReport([employee, inactive], [allocation], reportDate, 70m);
		var staffing = service.GetProjectStaffingSummary([project], [allocation]);
		var utilisation = service.GetDepartmentUtilisation([employee, inactive], [allocation], reportDate);

		var benchRow = Assert.Single(bench);
		Assert.Equal(40m, benchRow.AllocatedPercentage);
		Assert.Equal(60m, benchRow.AvailablePercentage);
		var staffingRow = Assert.Single(staffing);
		Assert.Equal(1, staffingRow.AssignedEmployeeCount);
		Assert.Equal(40m, staffingRow.TotalAllocationPercentage);
		var utilisationRow = Assert.Single(utilisation);
		Assert.Equal("Platform", utilisationRow.Department);
		Assert.Equal(1, utilisationRow.ActiveEmployeeCount);
		Assert.Equal(40m, utilisationRow.UtilisationPercentage);
	}

	[Fact]
	public void BenchReport_InvalidThreshold_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new ReportingService().GetBenchReport([], [], new DateOnly(2026, 1, 1), 101m));
	}
}

public sealed class AllocationApplicationServiceTests
{
	[Fact]
	public async Task AllocateAndRelease_PersistRepositoryChanges()
	{
		var repository = new InMemoryRmsRepository();
		var employee = new Employee("E1", "Name", "e1@example.com", "Dev", "Dept", 100m, EmploymentType.Permanent);
		var project = new Project("P1", "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
		await repository.SeedEmployeesAsync(employee);
		await repository.SeedProjectsAsync(project);
		var allocation = new Allocation("A1", employee.Id, project.Id, project.StartDate, project.EndDate, 50m);
		var service = new AllocationApplicationService(repository, new AllocationService());

		await service.AllocateAsync(allocation);
		Assert.Same(allocation, Assert.Single(await repository.GetAllocationsAsync()));

		await service.ReleaseAsync(allocation.Id);
		Assert.Empty(await repository.GetAllocationsAsync());
	}

	[Fact]
	public async Task Allocate_MissingEmployee_ThrowsAndDoesNotPersist()
	{
		var repository = new InMemoryRmsRepository();
		await repository.SeedProjectsAsync(new Project("P1", "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)));
		var allocation = new Allocation("A1", "missing", "P1", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 50m);

		await Assert.ThrowsAsync<InvalidOperationException>(() => new AllocationApplicationService(repository, new AllocationService()).AllocateAsync(allocation));
		Assert.Empty(await repository.GetAllocationsAsync());
	}

	[Fact]
	public async Task CancellationToken_IsPropagatedToRepository()
	{
		var repository = new CancellationAwareRepository();
		var service = new EmployeeService(repository);
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		await Assert.ThrowsAsync<OperationCanceledException>(() => service.GetEmployeeSummariesAsync(cancellation.Token));
		Assert.Equal(cancellation.Token, repository.ReceivedToken);
	}

	private sealed class CancellationAwareRepository : IRmsRepository
	{
		public CancellationToken ReceivedToken { get; private set; }
		public Task<List<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default)
		{
			ReceivedToken = cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(new List<Employee>());
		}
		public Task<List<Project>> GetProjectsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Project>());
		public Task<List<Allocation>> GetAllocationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Allocation>());
		public Task SaveEmployeesAsync(IEnumerable<Employee> employees, CancellationToken cancellationToken = default) => Task.CompletedTask;
		public Task SaveProjectsAsync(IEnumerable<Project> projects, CancellationToken cancellationToken = default) => Task.CompletedTask;
		public Task SaveAllocationsAsync(IEnumerable<Allocation> allocations, CancellationToken cancellationToken = default) => Task.CompletedTask;
	}
}
