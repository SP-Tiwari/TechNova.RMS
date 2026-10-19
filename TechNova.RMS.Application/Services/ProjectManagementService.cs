namespace TechNova.RMS.Application.Services;

public sealed class ProjectManagementService(IRmsRepository repository)
{
	/// <summary>
	/// Adds a new project after checking for duplicate project IDs and persists it.
	/// </summary>
	/// <param name="project">Project to add.</param>
	public async Task AddProjectAsync(Project project, CancellationToken cancellationToken = default)
	{
		var projects = await repository.GetProjectsAsync(cancellationToken).ConfigureAwait(false);

		if (projects.Any(item =>
			string.Equals(item.Id, project.Id,
				StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException(
				$"Project '{project.Id}' already exists.");
		}

		projects.Add(project);
		await repository.SaveProjectsAsync(projects, cancellationToken).ConfigureAwait(false);
	}
}
