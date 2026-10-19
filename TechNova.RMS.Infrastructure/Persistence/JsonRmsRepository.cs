
namespace TechNova.RMS.Infrastructure.Persistence;

public sealed class JsonRmsRepository : IRmsRepository
{
	/// <summary>
	/// JSON-backed implementation of <see cref="IRmsRepository"/> that reads and writes DTOs from a local Data folder.
	/// </summary>
	private readonly string _dataDirectory;

	private readonly JsonSerializerOptions _options = new()
	{
		WriteIndented = true,
		Converters = { new JsonStringEnumConverter() }
	};

	public JsonRmsRepository()
	{
		_dataDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Data"));

		Directory.CreateDirectory(_dataDirectory);
	}

	private readonly System.Threading.SemaphoreSlim _semaphore = new(1, 1);

	public async Task<List<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default)
	{
		var data = await ReadAsync<EmployeeData>("employees.json", cancellationToken).ConfigureAwait(false);
		var employees = new List<Employee>();

		foreach (var item in data)
		{
			var employee = new Employee(
				item.Id,
				item.FullName,
				item.Email,
				item.Designation,
				item.Department,
				item.CostRate,
				item.EmploymentType);

			foreach (var skill in item.Skills)
				employee.AddSkill(skill.SkillName, skill.ProficiencyLevel);

			if (!item.IsActive)
				employee.Deactivate();

			employees.Add(employee);
		}

		return employees;
	}

	public async Task<List<Project>> GetProjectsAsync(CancellationToken cancellationToken = default)
	{
		var data = await ReadAsync<ProjectData>("projects.json", cancellationToken).ConfigureAwait(false);
		var projects = new List<Project>();

		foreach (var item in data)
		{
			var project = new Project(
				item.Id,
				item.Name,
				item.ClientName,
				item.StartDate,
				item.EndDate,
				item.Status);

			foreach (var skill in item.RequiredSkills)
			{
				project.AddRequiredSkill(
					skill.SkillName,
					skill.MinimumProficiencyLevel);
			}

			projects.Add(project);
		}

		return projects;
	}

	public async Task<List<Allocation>> GetAllocationsAsync(CancellationToken cancellationToken = default)
	{
		var data = await ReadAsync<AllocationData>("allocations.json", cancellationToken).ConfigureAwait(false);
		return data.Select(item => new Allocation(
			item.Id,
			item.EmployeeId,
			item.ProjectId,
			item.StartDate,
			item.EndDate,
			item.AllocationPercentage)).ToList();
	}

	public async Task SaveEmployeesAsync(IEnumerable<Employee> employees, CancellationToken cancellationToken = default)
	{
		var data = employees.Select(employee => new EmployeeData
		{
			Id = employee.Id,
			FullName = employee.FullName,
			Email = employee.Email,
			Designation = employee.Designation,
			Department = employee.Department,
			CostRate = employee.CostRate,
			IsActive = employee.IsActive,
			EmploymentType = employee.EmploymentType,
			Skills = employee.Skills.Select(skill => new SkillData
			{
				SkillName = skill.SkillName,
				ProficiencyLevel = skill.ProficiencyLevel
			}).ToList()
		});

		await WriteAsync("employees.json", data, cancellationToken).ConfigureAwait(false);
	}

	public async Task SaveProjectsAsync(IEnumerable<Project> projects, CancellationToken cancellationToken = default)
	{
		var data = projects.Select(project => new ProjectData
		{
			Id = project.Id,
			Name = project.Name,
			ClientName = project.ClientName,
			StartDate = project.StartDate,
			EndDate = project.EndDate,
			Status = project.Status,
			RequiredSkills = project.RequiredSkills
				.Select(skill => new RequiredSkillData
				{
					SkillName = skill.SkillName,
					MinimumProficiencyLevel =
						skill.MinimumProficiencyLevel
				})
				.ToList()
		});

		await WriteAsync("projects.json", data, cancellationToken).ConfigureAwait(false);
	}

	public async Task SaveAllocationsAsync(IEnumerable<Allocation> allocations, CancellationToken cancellationToken = default)
	{
		var data = allocations.Select(allocation => new AllocationData
		{
			Id = allocation.Id,
			EmployeeId = allocation.EmployeeId,
			ProjectId = allocation.ProjectId,
			StartDate = allocation.StartDate,
			EndDate = allocation.EndDate,
			AllocationPercentage = allocation.AllocationPercentage
		});

		await WriteAsync("allocations.json", data, cancellationToken).ConfigureAwait(false);
	}

	private async Task<List<T>> ReadAsync<T>(string fileName, CancellationToken cancellationToken)
	{
		var path = Path.Combine(_dataDirectory, fileName);

		await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			if (!File.Exists(path))
				return new List<T>();

			var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);

			return JsonSerializer.Deserialize<List<T>>(json, _options)
				?? new List<T>();
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private async Task WriteAsync<T>(string fileName, IEnumerable<T> data, CancellationToken cancellationToken)
	{
		var path = Path.Combine(_dataDirectory, fileName);
		var json = JsonSerializer.Serialize(data, _options);

		await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			await File.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public sealed class EmployeeData
	{
		public string Id { get; set; } = "";
		public string FullName { get; set; } = "";
		public string Email { get; set; } = "";
		public string Designation { get; set; } = "";
		public string Department { get; set; } = "";
		public decimal CostRate { get; set; }
		public bool IsActive { get; set; }
		public EmploymentType EmploymentType { get; set; }
		public List<SkillData> Skills { get; set; } = [];
	}

	public sealed class SkillData
	{
		public string SkillName { get; set; } = "";
		public int ProficiencyLevel { get; set; }
	}

	public sealed class ProjectData
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public string ClientName { get; set; } = "";
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public ProjectStatus Status { get; set; }
		public List<RequiredSkillData> RequiredSkills { get; set; } = [];
	}

	public sealed class RequiredSkillData
	{
		public string SkillName { get; set; } = "";
		public int MinimumProficiencyLevel { get; set; }
	}

	public sealed class AllocationData
	{
		public string Id { get; set; } = "";
		public string EmployeeId { get; set; } = "";
		public string ProjectId { get; set; } = "";
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public decimal AllocationPercentage { get; set; }
	}
}
