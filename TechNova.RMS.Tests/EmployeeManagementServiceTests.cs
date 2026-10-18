namespace TechNova.RMS.Tests;

public class EmployeeManagementServiceTests
{
	[Fact]
	public void AddEmployee_SavesEmployee()
	{
		var repo = new InMemoryRmsRepository();
		var svc = new EmployeeManagementService(repo);

		var emp = new Employee("T1", "Test One", "t1@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		svc.AddEmployee(emp);

		var saved = repo.GetEmployees();
		Assert.Contains(saved, e => e.Id == emp.Id);
	}

	[Fact]
	public void AddEmployee_Duplicate_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var existing = new Employee("T2", "Existing", "e@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		repo.SeedEmployees(new[] { existing });

		var svc = new EmployeeManagementService(repo);
		var dup = new Employee("T2", "Dup", "d@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		Assert.Throws<InvalidOperationException>(() => svc.AddEmployee(dup));
	}

	[Fact]
	public void UpdateEmployee_ChangesDetails()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("T3", "Old", "old@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		repo.SeedEmployees(new[] { emp });

		var svc = new EmployeeManagementService(repo);

		svc.UpdateEmployee(emp.Id, "New Name", "new@x.com", "Senior Dev", "R&D", 2000m, EmploymentType.Contract);

		var updated = repo.GetEmployees().First(e => e.Id == emp.Id);
		Assert.Equal("New Name", updated.FullName);
		Assert.Equal("new@x.com", updated.Email);
		Assert.Equal("Senior Dev", updated.Designation);
		Assert.Equal("R&D", updated.Department);
		Assert.Equal(2000m, updated.CostRate);
		Assert.Equal(EmploymentType.Contract, updated.EmploymentType);
	}

	[Fact]
	public void UpdateEmployee_NotFound_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var svc = new EmployeeManagementService(repo);

		Assert.Throws<InvalidOperationException>(() =>
			svc.UpdateEmployee("Missing", "Name", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent));
	}

	[Fact]
	public void DeactivateEmployee_SetsIsActiveFalse()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("T4", "ToDeactivate", "d@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		repo.SeedEmployees(new[] { emp });

		var svc = new EmployeeManagementService(repo);

		svc.DeactivateEmployee(emp.Id);

		var updated = repo.GetEmployees().First(e => e.Id == emp.Id);
		Assert.False(updated.IsActive);
	}
}
