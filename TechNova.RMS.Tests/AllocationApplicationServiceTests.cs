namespace TechNova.RMS.Tests;

public class AllocationApplicationServiceTests
{
	[Fact]
	public void Allocate_Success_PersistsAllocation()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("A1", "Alloc", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		var proj = new Project("PR1", "P", "C", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		repo.SeedEmployees(new[] { emp });
		repo.SeedProjects(new[] { proj });
		repo.SeedAllocations(new Allocation[] { });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		var alloc = new Allocation("AL-A", emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50m);

		app.Allocate(alloc);

		var saved = repo.GetAllocations();
		Assert.Contains(saved, a => a.Id == alloc.Id);
	}

	[Fact]
	public void Allocate_MissingEmployee_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var proj = new Project("PR2", "P", "C", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		repo.SeedProjects(new[] { proj });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		var alloc = new Allocation("AL-B", "NoSuch", proj.Id, proj.StartDate, proj.EndDate, 50m);

		Assert.Throws<InvalidOperationException>(() => app.Allocate(alloc));
	}

	[Fact]
	public void Allocate_MissingProject_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("A2", "Alloc", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		repo.SeedEmployees(new[] { emp });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		var alloc = new Allocation("AL-C", emp.Id, "NoProj", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 50m);

		Assert.Throws<InvalidOperationException>(() => app.Allocate(alloc));
	}

	[Fact]
	public void Release_RemovesAllocation()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("A3", "Alloc", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		var proj = new Project("PR3", "P", "C", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		var allocation = new Allocation("REL1", emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50m);

		repo.SeedEmployees(new[] { emp });
		repo.SeedProjects(new[] { proj });
		repo.SeedAllocations(new[] { allocation });

		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		app.Release(allocation.Id);

		var saved = repo.GetAllocations();
		Assert.DoesNotContain(saved, a => a.Id == allocation.Id);
	}

	[Fact]
	public void Release_MissingAllocation_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var allocationService = new AllocationService();
		var app = new AllocationApplicationService(repo, allocationService);

		Assert.Throws<InvalidOperationException>(() => app.Release("NoSuch"));
	}
}
