namespace TechNova.RMS.Domain.Projects;

public sealed class Project
{
	private readonly List<ProjectRequiredSkill> _requiredSkills = [];

	public string Id { get; private set; }

	public string Name { get; private set; } = string.Empty;

	public string ClientName { get; private set; } = string.Empty;

	public DateOnly StartDate { get; private set; }

	public DateOnly EndDate { get; private set; }

	public ProjectStatus Status { get; private set; }

	public IReadOnlyList<ProjectRequiredSkill> RequiredSkills
		=> _requiredSkills.AsReadOnly();

	public Project(
		string id,
		string name,
		string clientName,
		DateOnly startDate,
		DateOnly endDate,
		ProjectStatus status = ProjectStatus.Planned)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException(
				"Project ID is required.",
				nameof(id));
		}

		Id = id.Trim();

		UpdateDetails(name, clientName, startDate, endDate);

		Status = status;
	}

	public void UpdateDetails(
		string name,
		string clientName,
		DateOnly startDate,
		DateOnly endDate)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(
				"Project name is required.",
				nameof(name));
		}

		if (string.IsNullOrWhiteSpace(clientName))
		{
			throw new ArgumentException(
				"Client name is required.",
				nameof(clientName));
		}

		if (endDate < startDate)
		{
			throw new ArgumentException(
				"Project end date cannot be before its start date.");
		}

		Name = name.Trim();
		ClientName = clientName.Trim();
		StartDate = startDate;
		EndDate = endDate;
	}

	public void ChangeStatus(ProjectStatus status)
	{
		/// <summary>
		/// Change the project's status to a new <see cref="ProjectStatus"/> value.
		/// </summary>
		/// <param name="status">New status value.</param>
		Status = status;
	}

	public void AddRequiredSkill(
		string skillName,
		int minimumProficiencyLevel)
	{
		/// <summary>
		/// Adds a required skill with a minimum proficiency level to the project.
		/// Throws when the skill already exists or the level is out of range.
		/// </summary>
		/// <param name="skillName">Skill name (required).</param>
		/// <param name="minimumProficiencyLevel">Minimum proficiency required (1-5).</param>
		var newSkill = new ProjectRequiredSkill(
			skillName,
			minimumProficiencyLevel);

		bool alreadyExists = _requiredSkills.Any(
			skill => string.Equals(
				skill.SkillName,
				newSkill.SkillName,
				StringComparison.OrdinalIgnoreCase));

		if (alreadyExists)
		{
			throw new InvalidOperationException(
				$"Project already requires the skill '{newSkill.SkillName}'.");
		}

		_requiredSkills.Add(newSkill);
	}
}