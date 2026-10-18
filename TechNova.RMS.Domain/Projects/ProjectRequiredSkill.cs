namespace TechNova.RMS.Domain.Projects;

public sealed class ProjectRequiredSkill
{
	public string SkillName { get; }

	public int MinimumProficiencyLevel { get; }

	public ProjectRequiredSkill(
		string skillName,
		int minimumProficiencyLevel)
	{
		if (string.IsNullOrWhiteSpace(skillName))
		{
			throw new ArgumentException(
				"Skill name is required.",
				nameof(skillName));
		}

		if (minimumProficiencyLevel is < 1 or > 5)
		{
			throw new ArgumentOutOfRangeException(
				nameof(minimumProficiencyLevel),
				"Minimum proficiency must be between 1 and 5.");
		}

		SkillName = skillName.Trim();
		MinimumProficiencyLevel = minimumProficiencyLevel;
	}
}