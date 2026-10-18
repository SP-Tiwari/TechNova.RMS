namespace TechNova.RMS.Tests;

public class AllocationTests
{
	private readonly AllocationService _allocationService = new();

	[Fact]
	public void ValidateAllocation_InactiveEmployee_ThrowsInvalidOperationException()
	{
		var employee = new Employee("E1", "Test", "t@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		employee.Deactivate();

		var project = new Project("P1", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 31));

		var allocation = new Allocation("A1", employee.Id, project.Id, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 50m);

		var existing = new List<Allocation>();

		Assert.Throws<InvalidOperationException>(() =>
			_allocationService.ValidateAllocation(employee, project, allocation, existing));
	}

	[Fact]
	public void ValidateAllocation_OutOfProjectDates_ThrowsInvalidOperationException()
	{
		var employee = new Employee("E2", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P2", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		// allocation starts before project start
		var allocation = new Allocation("A2", employee.Id, project.Id, new DateOnly(2026, 10, 25), new DateOnly(2026, 11, 10), 50m);

		var existing = new List<Allocation>();

		Assert.Throws<InvalidOperationException>(() =>
			_allocationService.ValidateAllocation(employee, project, allocation, existing));
	}

	[Fact]
	public void ValidateAllocation_OverAllocation_ThrowsInvalidOperationException()
	{
		var employee = new Employee("E3", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P3", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		// existing allocation 60%
		var existing = new List<Allocation>
		{
			new Allocation("EX1", employee.Id, "P0", new DateOnly(2026,11,1), new DateOnly(2026,11,30), 60m)
		};

		// new allocation 50% overlaps -> total 110%
		var allocation = new Allocation("A3", employee.Id, project.Id, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 50m);

		Assert.Throws<InvalidOperationException>(() =>
			_allocationService.ValidateAllocation(employee, project, allocation, existing));
	}

	[Fact]
	public void ValidateAllocation_Valid_AllowsAllocation()
	{
		var employee = new Employee("E4", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P4", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		// existing allocation 40%
		var existing = new List<Allocation>
		{
			new Allocation("EX2", employee.Id, "P0", new DateOnly(2026,11,1), new DateOnly(2026,11,30), 40m)
		};

		// new allocation 50% -> total 90%
		var allocation = new Allocation("A4", employee.Id, project.Id, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 50m);

		// should not throw
		_allocationService.ValidateAllocation(employee, project, allocation, existing);
	}

	[Fact]
	public void ValidateAllocation_Exactly100_AllowsAllocation()
	{
		var employee = new Employee("E5", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P5", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		var existing = new List<Allocation>
		{
			new Allocation("EX3", employee.Id, "P0", new DateOnly(2026,11,1), new DateOnly(2026,11,30), 60m)
		};

		var allocation = new Allocation("A5", employee.Id, project.Id, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 40m);

		// total 100% should be allowed
		_allocationService.ValidateAllocation(employee, project, allocation, existing);
	}

	[Fact]
	public void ValidateAllocation_ExceedsByOnePercent_ThrowsInvalidOperationException()
	{
		var employee = new Employee("E6", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P6", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		var existing = new List<Allocation>
		{
			new Allocation("EX4", employee.Id, "P0", new DateOnly(2026,11,1), new DateOnly(2026,11,30), 60m)
		};

		var allocation = new Allocation("A6", employee.Id, project.Id, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 41m);

		Assert.Throws<InvalidOperationException>(() =>
			_allocationService.ValidateAllocation(employee, project, allocation, existing));
	}

	[Fact]
	public void ValidateAllocation_NonOverlappingExistingDoesNotCount()
	{
		var employee = new Employee("E7", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P7", "Proj", "Client", new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 20));

		// existing allocation outside project dates
		var existing = new List<Allocation>
		{
			new Allocation("EX5", employee.Id, "P0", new DateOnly(2026,10,1), new DateOnly(2026,10,31), 100m)
		};

		var allocation = new Allocation("A7", employee.Id, project.Id, new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 20), 100m);

		// should be allowed since existing does not overlap
		_allocationService.ValidateAllocation(employee, project, allocation, existing);
	}

	[Fact]
	public void ValidateAllocation_OtherEmployeeAllocationsDoNotCount()
	{
		var employee = new Employee("E8", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		var other = new Employee("E9", "Other", "o@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P8", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		var existing = new List<Allocation>
		{
			// allocation for other employee should not affect this employee
			new Allocation("EX6", other.Id, project.Id, project.StartDate, project.EndDate, 100m)
		};

		var allocation = new Allocation("A8", employee.Id, project.Id, project.StartDate, project.EndDate, 100m);

		// should be allowed
		_allocationService.ValidateAllocation(employee, project, allocation, existing);
	}

	[Fact]
	public void ValidateAllocation_PartialOverlap_HandledCorrectly()
	{
		var employee = new Employee("E10", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P9", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		// existing covers middle of month
		var existing = new List<Allocation>
		{
			new Allocation("EX7", employee.Id, "P0", new DateOnly(2026,11,10), new DateOnly(2026,11,20), 60m)
		};

		// new allocation overlaps partially (1st-15th) -> overlap 10th-15th => sum 110 on those days
		var allocation = new Allocation("A9", employee.Id, project.Id, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 15), 50m);

		Assert.Throws<InvalidOperationException>(() =>
			_allocationService.ValidateAllocation(employee, project, allocation, existing));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-10)]
	[InlineData(150)]
	public void Allocation_Constructor_InvalidPercentage_ThrowsArgumentOutOfRange(decimal percent)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new Allocation("INV", "E0", "P0", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), percent));
	}

	[Fact]
	public void ValidateAllocation_ProjectIdMismatch_ThrowsArgumentException()
	{
		var employee = new Employee("E11", "Active", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		var project = new Project("P10", "Proj", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

		var allocation = new Allocation("A10", employee.Id, "DifferentProject", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 50m);

		var existing = new List<Allocation>();

		Assert.Throws<ArgumentException>(() =>
			_allocationService.ValidateAllocation(employee, project, allocation, existing));
	}
}
