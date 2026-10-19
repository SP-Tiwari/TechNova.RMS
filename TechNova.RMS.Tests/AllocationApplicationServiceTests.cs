namespace TechNova.RMS.Tests;

public class AllocationApplicationServiceTests
{
	[Fact]
	public async Task Allocate_Success_PersistsAllocation()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("A1", "Alloc", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		var proj = new Project("PR1", "P", "C", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		await repo.SeedEmployeesAsync(new[] { emp });
		await repo.SeedProjectsAsync(new[] { proj });
		await repo.SeedAllocationsAsync(new Allocation[] { });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		var alloc = new Allocation("AL-A", emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50m);

		await app.AllocateAsync(alloc);

		var saved = await repo.GetAllocationsAsync();
		Assert.Contains(saved, a => a.Id == alloc.Id);
	}

	[Fact]
	public async Task Allocate_MissingEmployee_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var proj = new Project("PR2", "P", "C", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		await repo.SeedProjectsAsync(new[] { proj });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		var alloc = new Allocation("AL-B", "NoSuch", proj.Id, proj.StartDate, proj.EndDate, 50m);

		await Assert.ThrowsAsync<InvalidOperationException>(() => app.AllocateAsync(alloc));
	}

	[Fact]
	public async Task Allocate_MissingProject_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("A2", "Alloc", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		await repo.SeedEmployeesAsync(new[] { emp });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		var alloc = new Allocation("AL-C", emp.Id, "NoProj", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 50m);

		await Assert.ThrowsAsync<InvalidOperationException>(() => app.AllocateAsync(alloc));
	}

	[Fact]
	public async Task Release_RemovesAllocation()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("A3", "Alloc", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		var proj = new Project("PR3", "P", "C", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		var allocation = new Allocation("REL1", emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50m);

		await repo.SeedEmployeesAsync(new[] { emp });
		await repo.SeedProjectsAsync(new[] { proj });
		await repo.SeedAllocationsAsync(new[] { allocation });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		await app.ReleaseAsync(allocation.Id);

		var saved = await repo.GetAllocationsAsync();
		Assert.DoesNotContain(saved, a => a.Id == allocation.Id);
	}

	[Fact]
	public async Task Release_MissingAllocation_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		await Assert.ThrowsAsync<InvalidOperationException>(() => app.ReleaseAsync("NoSuch"));
	}
}
