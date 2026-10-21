namespace TechNova.RMS.Infrastructure.Tests;

public sealed class JsonRmsRepositoryTests : IDisposable
{
	private readonly string _dataDirectory;

	public JsonRmsRepositoryTests()
	{
		_dataDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Data"));
		Directory.CreateDirectory(_dataDirectory);
	}

	[Fact]
	public async Task SavingAndLoadingAllEntityTypes_PreservesDomainValues()
	{
		var repository = new JsonRmsRepository();
		var employee = new Employee("E1", "Alice", "alice@example.com", "Developer", "Platform", 125m, EmploymentType.Contract);
		employee.AddSkill("C#", 5);
		employee.Deactivate();
		var project = new Project("P1", "Project", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), ProjectStatus.Active);
		project.AddRequiredSkill("C#", 4);
		var allocation = new Allocation("A1", employee.Id, project.Id, project.StartDate, project.EndDate, 75m);

		await repository.SaveEmployeesAsync([employee]);
		await repository.SaveProjectsAsync([project]);
		await repository.SaveAllocationsAsync([allocation]);

		var loadedEmployee = Assert.Single(await repository.GetEmployeesAsync());
		var loadedProject = Assert.Single(await repository.GetProjectsAsync());
		var loadedAllocation = Assert.Single(await repository.GetAllocationsAsync());
		Assert.Equal(employee.Id, loadedEmployee.Id);
		Assert.Equal(employee.FullName, loadedEmployee.FullName);
		Assert.Equal(employee.EmploymentType, loadedEmployee.EmploymentType);
		Assert.False(loadedEmployee.IsActive);
		Assert.Equal(("C#", 5), (loadedEmployee.Skills[0].SkillName, loadedEmployee.Skills[0].ProficiencyLevel));
		Assert.Equal(project.Status, loadedProject.Status);
		Assert.Equal(("C#", 4), (loadedProject.RequiredSkills[0].SkillName, loadedProject.RequiredSkills[0].MinimumProficiencyLevel));
		Assert.Equal(allocation.AllocationPercentage, loadedAllocation.AllocationPercentage);
	}

	[Fact]
	public async Task MissingFiles_ReturnEmptyCollections()
	{
		DeleteDataFiles();
		var repository = new JsonRmsRepository();

		Assert.Empty(await repository.GetEmployeesAsync());
		Assert.Empty(await repository.GetProjectsAsync());
		Assert.Empty(await repository.GetAllocationsAsync());
	}

	[Fact]
	public async Task EmptyJsonArrays_ReturnEmptyCollections()
	{
		WriteDataFiles("[]", "[]", "[]");
		var repository = new JsonRmsRepository();

		Assert.Empty(await repository.GetEmployeesAsync());
		Assert.Empty(await repository.GetProjectsAsync());
		Assert.Empty(await repository.GetAllocationsAsync());
	}

	[Fact]
	public async Task MalformedJson_ThrowsJsonException()
	{
		WriteDataFiles("{not-json", "[]", "[]");
		var repository = new JsonRmsRepository();

		await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => repository.GetEmployeesAsync());
	}

	public void Dispose() => DeleteDataFiles();

	private void WriteDataFiles(string employees, string projects, string allocations)
	{
		File.WriteAllText(Path.Combine(_dataDirectory, "employees.json"), employees);
		File.WriteAllText(Path.Combine(_dataDirectory, "projects.json"), projects);
		File.WriteAllText(Path.Combine(_dataDirectory, "allocations.json"), allocations);
	}

	private void DeleteDataFiles()
	{
		foreach (var fileName in new[] { "employees.json", "projects.json", "allocations.json" })
		{
			var path = Path.Combine(_dataDirectory, fileName);
			if (File.Exists(path))
				File.Delete(path);
		}
	}
}
