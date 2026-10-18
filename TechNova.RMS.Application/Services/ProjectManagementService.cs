
using TechNova.RMS.Application.Persistence;
using TechNova.RMS.Domain.Projects;

namespace TechNova.RMS.Application.Services;

public sealed class ProjectManagementService(IRmsRepository repository)
{
	public void AddProject(Project project)
	{
		var projects = repository.GetProjects();

		if (projects.Any(item =>
			string.Equals(item.Id, project.Id,
				StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException(
				$"Project '{project.Id}' already exists.");
		}

		projects.Add(project);
		repository.SaveProjects(projects);
	}
}
