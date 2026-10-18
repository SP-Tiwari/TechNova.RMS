namespace TechNova.RMS.Tests;

public class MatchingTests
{
	private readonly EmployeeMatchingService _matching = new();

	[Fact]
	public void FindMatchingEmployees_MatchesBySkillAndAvailability()
	{
		var project = new Project("PJ", "Name", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		project.AddRequiredSkill("C#", 4);

		var good = new Employee("M1", "Good", "g@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		good.AddSkill("C#", 5);

		var weak = new Employee("M2", "Weak", "w@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		weak.AddSkill("C#", 3);

		var busy = new Employee("M3", "Busy", "b@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		busy.AddSkill("C#", 5);

		// busy has allocations that make them 100% on project dates
		var allocations = new List<Allocation>
		{
			new Allocation("AL1", busy.Id, project.Id, project.StartDate, project.EndDate, 100m)
		};

		var employees = new List<Employee> { good, weak, busy };

		var matches = _matching.FindMatchingEmployees(project, employees, allocations);

		Assert.Contains(matches, e => e.Id == good.Id);
		Assert.DoesNotContain(matches, e => e.Id == weak.Id);
		Assert.DoesNotContain(matches, e => e.Id == busy.Id);
	}

	[Fact]
	public void FindMatchingEmployees_ExactSkillLevel_Matches()
	{
		var project = new Project("PJ2", "Name", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		project.AddRequiredSkill("SQL", 4);

		var emp = new Employee("MS1", "Exact", "e@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		emp.AddSkill("SQL", 4);

		var matches = _matching.FindMatchingEmployees(project, new[] { emp }, Enumerable.Empty<Allocation>());

		Assert.Contains(matches, e => e.Id == emp.Id);
	}

	[Fact]
	public void FindMatchingEmployees_InactiveEmployee_Excluded()
	{
		var project = new Project("PJ3", "Name", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		project.AddRequiredSkill("C#", 3);

		var emp = new Employee("MS2", "Inactive", "i@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		emp.AddSkill("C#", 5);
		emp.Deactivate();

		var matches = _matching.FindMatchingEmployees(project, new[] { emp }, Enumerable.Empty<Allocation>());

		Assert.DoesNotContain(matches, e => e.Id == emp.Id);
	}

	[Fact]
	public void FindMatchingEmployees_MissingOneRequiredSkill_Excluded()
	{
		var project = new Project("PJ4", "Name", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		project.AddRequiredSkill("C#", 3);
		project.AddRequiredSkill("SQL", 3);

		var emp = new Employee("MS3", "Partial", "p@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		emp.AddSkill("C#", 4);
		// missing SQL

		var matches = _matching.FindMatchingEmployees(project, new[] { emp }, Enumerable.Empty<Allocation>());

		Assert.DoesNotContain(matches, e => e.Id == emp.Id);
	}

	[Fact]
	public void FindMatchingEmployees_NoAvailability_Excluded()
	{
		var project = new Project("PJ5", "Name", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		project.AddRequiredSkill("C#", 3);

		var emp = new Employee("MS4", "BusyAll", "b@x.com", "Dev", "Eng", 1000m, EmploymentType.Permanent);
		emp.AddSkill("C#", 4);

		var allocations = new List<Allocation>
		{
			new Allocation("B1", emp.Id, project.Id, project.StartDate, project.EndDate, 100m)
		};

		var matches = _matching.FindMatchingEmployees(project, new[] { emp }, allocations);

		Assert.DoesNotContain(matches, e => e.Id == emp.Id);
	}

	[Fact]
	public void FindMatchingEmployees_EmptyEmployeeList_ReturnsNoMatches()
	{
		var project = new Project("PJ6", "Name", "Client", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
		project.AddRequiredSkill("C#", 1);

		var matches = _matching.FindMatchingEmployees(project, Array.Empty<Employee>(), Enumerable.Empty<Allocation>());

		Assert.Empty(matches);
	}
}
