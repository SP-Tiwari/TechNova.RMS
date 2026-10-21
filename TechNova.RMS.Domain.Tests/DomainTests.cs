using TechNova.RMS.Domain.Allocations;
using TechNova.RMS.Domain.Employees;
using TechNova.RMS.Domain.Projects;
using TechNova.RMS.Domain.Services;

namespace TechNova.RMS.Domain.Tests;

public sealed class EmployeeTests
{
	[Fact]
	public void Constructor_TrimsValuesAndSetsDetails()
	{
		var employee = new Employee(" E1 ", " Alice ", " alice@example.com ", " Developer ", " Engineering ", 100m, EmploymentType.Contract);

		Assert.Equal("E1", employee.Id);
		Assert.Equal("Alice", employee.FullName);
		Assert.Equal("alice@example.com", employee.Email);
		Assert.Equal("Developer", employee.Designation);
		Assert.Equal("Engineering", employee.Department);
		Assert.Equal(100m, employee.CostRate);
		Assert.Equal(EmploymentType.Contract, employee.EmploymentType);
		Assert.True(employee.IsActive);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public void Constructor_RequiredValues_Throw(string value)
	{
		Assert.Throws<ArgumentException>(() => new Employee(value, "Name", "email", "Dev", "Dept", 1m, EmploymentType.Permanent));
		Assert.Throws<ArgumentException>(() => new Employee("E1", value, "email", "Dev", "Dept", 1m, EmploymentType.Permanent));
		Assert.Throws<ArgumentException>(() => new Employee("E1", "Name", value, "Dev", "Dept", 1m, EmploymentType.Permanent));
		Assert.Throws<ArgumentException>(() => new Employee("E1", "Name", "email", value, "Dept", 1m, EmploymentType.Permanent));
		Assert.Throws<ArgumentException>(() => new Employee("E1", "Name", "email", "Dev", value, 1m, EmploymentType.Permanent));
	}

	[Fact]
	public void Constructor_NegativeCostRate_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new Employee("E1", "Name", "email", "Dev", "Dept", -1m, EmploymentType.Permanent));
	}

	[Fact]
	public void Skills_ValidateProficiencyAndPreventDuplicates()
	{
		var employee = CreateEmployee();

		employee.AddSkill(" C# ", 1);
		employee.AddSkill("SQL", 5);

		Assert.Equal(2, employee.Skills.Count);
		Assert.Equal("C#", employee.Skills[0].SkillName);
		Assert.Throws<ArgumentOutOfRangeException>(() => employee.AddSkill("Java", 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => employee.AddSkill("Java", 6));
		Assert.Throws<InvalidOperationException>(() => employee.AddSkill("c#", 3));
	}

	[Fact]
	public void Deactivate_SetsEmployeeInactive()
	{
		var employee = CreateEmployee();

		employee.Deactivate();

		Assert.False(employee.IsActive);
	}

	private static Employee CreateEmployee() => new("E1", "Name", "email", "Dev", "Dept", 1m, EmploymentType.Permanent);
}

public sealed class ProjectTests
{
	[Fact]
	public void Constructor_StoresDatesAndDefaultsToPlanned()
	{
		var project = new Project(" P1 ", " Project ", " Client ", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

		Assert.Equal("P1", project.Id);
		Assert.Equal("Project", project.Name);
		Assert.Equal("Client", project.ClientName);
		Assert.Equal(ProjectStatus.Planned, project.Status);
		Assert.Equal(new DateOnly(2026, 1, 1), project.StartDate);
		Assert.Equal(new DateOnly(2026, 1, 31), project.EndDate);
	}

	[Fact]
	public void Constructor_EndBeforeStart_Throws()
	{
		Assert.Throws<ArgumentException>(() => new Project("P1", "Project", "Client", new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 31)));
	}

	[Fact]
	public void RequiredSkills_ValidateLevelAndPreventDuplicates()
	{
		var project = new Project("P1", "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

		project.AddRequiredSkill(" C# ", 1);

		Assert.Equal("C#", project.RequiredSkills[0].SkillName);
		Assert.Throws<ArgumentOutOfRangeException>(() => project.AddRequiredSkill("SQL", 6));
		Assert.Throws<InvalidOperationException>(() => project.AddRequiredSkill("c#", 3));
	}
}

public sealed class AllocationTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(100.1)]
	public void Constructor_InvalidPercentage_Throws(decimal percentage)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => CreateAllocation(percentage));
	}

	[Fact]
	public void Constructor_BoundaryPercentageAndDates_AreAccepted()
	{
		var allocation = CreateAllocation(100m);

		Assert.Equal(100m, allocation.AllocationPercentage);
		Assert.Equal(allocation.StartDate, allocation.EndDate);
	}

	[Fact]
	public void Constructor_EndBeforeStart_Throws()
	{
		Assert.Throws<ArgumentException>(() => new Allocation("A1", "E1", "P1", new DateOnly(2026, 2, 2), new DateOnly(2026, 2, 1), 50m));
	}

	private static Allocation CreateAllocation(decimal percentage) =>
		new("A1", "E1", "P1", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1), percentage);
}

public sealed class AllocationServiceTests
{
	private readonly AllocationService _service = new();

	[Fact]
	public void ValidateAllocation_InactiveEmployee_Throws()
	{
		var employee = CreateEmployee();
		employee.Deactivate();

		Assert.Throws<InvalidOperationException>(() => _service.ValidateAllocation(employee, CreateProject(), CreateAllocation(employee.Id, "P1", 50m), []));
	}

	[Fact]
	public void ValidateAllocation_MismatchedIds_Throws()
	{
		var employee = CreateEmployee();
		var project = CreateProject();

		Assert.Throws<ArgumentException>(() => _service.ValidateAllocation(employee, project, CreateAllocation("other", project.Id, 50m), []));
		Assert.Throws<ArgumentException>(() => _service.ValidateAllocation(employee, project, CreateAllocation(employee.Id, "other", 50m), []));
	}

	[Fact]
	public void ValidateAllocation_OutsideProjectDates_Throws()
	{
		var employee = CreateEmployee();
		var project = CreateProject();
		var allocation = new Allocation("A1", employee.Id, project.Id, project.StartDate.AddDays(-1), project.EndDate, 50m);

		Assert.Throws<InvalidOperationException>(() => _service.ValidateAllocation(employee, project, allocation, []));
	}

	[Fact]
	public void ValidateAllocation_Exactly100PercentOnBoundary_IsAccepted()
	{
		var employee = CreateEmployee();
		var project = CreateProject();
		var existing = new[] { new Allocation("Existing", employee.Id, "Other", project.StartDate, project.EndDate, 60m) };
		var allocation = CreateAllocation(employee.Id, project.Id, 40m);

		_service.ValidateAllocation(employee, project, allocation, existing);
	}

	[Fact]
	public void ValidateAllocation_OverlappingTotalOver100_Throws()
	{
		var employee = CreateEmployee();
		var project = CreateProject();
		var existing = new[] { new Allocation("Existing", employee.Id, "Other", project.StartDate, project.EndDate, 60m) };
		var allocation = CreateAllocation(employee.Id, project.Id, 41m);

		Assert.Throws<InvalidOperationException>(() => _service.ValidateAllocation(employee, project, allocation, existing));
	}

	[Fact]
	public void ValidateAllocation_NonOverlappingAllocationDoesNotCount()
	{
		var employee = CreateEmployee();
		var project = CreateProject();
		var existing = new[] { new Allocation("Existing", employee.Id, "Other", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 2), 100m) };
		var allocation = CreateAllocation(employee.Id, project.Id, 100m);

		_service.ValidateAllocation(employee, project, allocation, existing);
	}

	private static Employee CreateEmployee() => new("E1", "Name", "email", "Dev", "Dept", 1m, EmploymentType.Permanent);
	private static Project CreateProject() => new("P1", "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
	private static Allocation CreateAllocation(string employeeId, string projectId, decimal percentage) => new("A1", employeeId, projectId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), percentage);
}
