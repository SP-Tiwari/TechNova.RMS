namespace TechNova.RMS.Domain.Employees;

public sealed class Employee
{
	public string Id { get; private set; }

	public string FullName { get; private set; } = string.Empty;

	public string Email { get; private set; } = string.Empty;

	public string Designation { get; private set; } = string.Empty;

	public string Department { get; private set; } = string.Empty;

	public decimal CostRate { get; private set; }

	public bool IsActive { get; private set; } = true;

	private readonly List<EmployeeSkill> _skills = [];

	public IReadOnlyList<EmployeeSkill> Skills => _skills.AsReadOnly();

	public EmploymentType EmploymentType { get; private set; }

	public Employee(string id, string fullName, string email, string designation, string department, decimal costRate, EmploymentType employmentType)
	{
		if (string.IsNullOrWhiteSpace(id))
			throw new ArgumentException(
				"Employee ID is required.", nameof(id));

		Id = id.Trim();

		SetDetails(fullName, email, designation, department, costRate);

		if (!Enum.IsDefined(employmentType))
		{
			throw new ArgumentOutOfRangeException(
				nameof(employmentType),
				"Invalid employment type.");
		}

		EmploymentType = employmentType;
	}

	public void UpdateDetails(
	string fullName,
	string email,
	string designation,
	string department,
	decimal costRate,
	EmploymentType employmentType)
	{
		SetDetails(
			fullName,
			email,
			designation,
			department,
			costRate);

		if (!Enum.IsDefined(employmentType))
		{
			throw new ArgumentOutOfRangeException(
				nameof(employmentType),
				"Invalid employment type.");
		}

		EmploymentType = employmentType;
	}


	public void AddSkill(string skillName, int proficiencyLevel)
	{
		var newSkill = new EmployeeSkill(skillName, proficiencyLevel);

		bool alreadyExists = _skills.Any(
			skill => string.Equals(
				skill.SkillName,
				newSkill.SkillName,
				StringComparison.OrdinalIgnoreCase));

		if (alreadyExists)
		{
			throw new InvalidOperationException(
				$"Employee already has the skill '{newSkill.SkillName}'.");
		}

		_skills.Add(newSkill);
	}


	public void RemoveSkill(string skillName)
	{
		if (string.IsNullOrWhiteSpace(skillName))
		{
			throw new ArgumentException(
				"Skill name is required.",
				nameof(skillName));
		}

		var existingSkill = _skills.FirstOrDefault(
			skill => string.Equals(
				skill.SkillName,
				skillName.Trim(),
				StringComparison.OrdinalIgnoreCase));

		if (existingSkill is null)
		{
			throw new InvalidOperationException(
				$"Employee does not have the skill '{skillName}'.");
		}

		_skills.Remove(existingSkill);
	}


	public void Deactivate()
	{
		IsActive = false;
	}

	private void SetDetails(
		string fullName,
		string email,
		string designation,
		string department,
		decimal costRate)
	{
		if (string.IsNullOrWhiteSpace(fullName))
			throw new ArgumentException(
				"Employee name is required.", nameof(fullName));

		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException(
				"Employee email is required.", nameof(email));

		if (string.IsNullOrWhiteSpace(designation))
			throw new ArgumentException(
				"Designation is required.", nameof(designation));

		if (string.IsNullOrWhiteSpace(department))
			throw new ArgumentException(
				"Department is required.", nameof(department));

		if (costRate < 0)
			throw new ArgumentOutOfRangeException(
				nameof(costRate),
				"Cost rate cannot be negative.");

		FullName = fullName.Trim();
		Email = email.Trim();
		Designation = designation.Trim();
		Department = department.Trim();
		CostRate = costRate;
	}
}