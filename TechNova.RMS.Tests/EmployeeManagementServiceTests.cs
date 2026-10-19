namespace TechNova.RMS.Tests;

public class EmployeeManagementServiceTests
{
	[Fact]
	public async Task AddEmployee_SavesEmployee()
	{
		var repo = new InMemoryRmsRepository();
		var svc = new EmployeeManagementService(repo);

		var emp = new Employee("T1", "Test One", "t1@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		await svc.AddEmployeeAsync(emp);

		var saved = await repo.GetEmployeesAsync();
		Assert.Contains(saved, e => e.Id == emp.Id);
	}

	[Fact]
	public async Task AddEmployee_Duplicate_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var existing = new Employee("T2", "Existing", "e@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		await repo.SeedEmployeesAsync(new[] { existing });

		var svc = new EmployeeManagementService(repo);
		var dup = new Employee("T2", "Dup", "d@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);

		await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AddEmployeeAsync(dup));
	}

	[Fact]
	public async Task UpdateEmployee_ChangesDetails()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("T3", "Old", "old@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		await repo.SeedEmployeesAsync(new[] { emp });

		var svc = new EmployeeManagementService(repo);

		await svc.UpdateEmployeeAsync(emp.Id, "New Name", "new@x.com", "Senior Dev", "R&D", 2000m, EmploymentType.Contract);

		var updated = (await repo.GetEmployeesAsync()).First(e => e.Id == emp.Id);
		Assert.Equal("New Name", updated.FullName);
		Assert.Equal("new@x.com", updated.Email);
		Assert.Equal("Senior Dev", updated.Designation);
		Assert.Equal("R&D", updated.Department);
		Assert.Equal(2000m, updated.CostRate);
		Assert.Equal(EmploymentType.Contract, updated.EmploymentType);
	}

	[Fact]
	public async Task UpdateEmployee_NotFound_ThrowsInvalidOperationException()
	{
		var repo = new InMemoryRmsRepository();
		var svc = new EmployeeManagementService(repo);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			svc.UpdateEmployeeAsync("Missing", "Name", "a@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent));
	}

	[Fact]
	public async Task DeactivateEmployee_SetsIsActiveFalse()
	{
		var repo = new InMemoryRmsRepository();
		var emp = new Employee("T4", "ToDeactivate", "d@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		await repo.SeedEmployeesAsync(new[] { emp });

		var svc = new EmployeeManagementService(repo);

		await svc.DeactivateEmployeeAsync(emp.Id);

		var updated = (await repo.GetEmployeesAsync()).First(e => e.Id == emp.Id);
		Assert.False(updated.IsActive);
	}
}
