namespace TechNova.RMS.Domain.Employees;

public sealed class EmployeeSkill
{
	public string SkillName { get; }

	public int ProficiencyLevel { get; }

	public EmployeeSkill(string skillName, int proficiencyLevel)
	{
		if (string.IsNullOrWhiteSpace(skillName))
		{
			throw new ArgumentException(
				"Skill name is required.",
				nameof(skillName));
		}

		if (proficiencyLevel is < 1 or > 5)
		{
			throw new ArgumentOutOfRangeException(
				nameof(proficiencyLevel),
				"Proficiency level must be between 1 and 5.");
		}

		SkillName = skillName.Trim();
		ProficiencyLevel = proficiencyLevel;
	}
}
