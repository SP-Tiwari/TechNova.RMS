namespace TechNova.RMS.ConsoleApp.UI;

public class ConsoleMenu
{
	private readonly EmployeeService _employeeService;
	private readonly EmployeeMatchingService _matchingService;
	private readonly IRmsRepository _repository;
	private readonly ReportingService _reportingService;
	private readonly AllocationApplicationService _allocationApplicationService;
	private readonly EmployeeManagementService _employeeManagementService;
	private readonly ProjectManagementService _projectManagementService;

	public ConsoleMenu(
		EmployeeService employeeService,
		EmployeeMatchingService matchingService,
		IRmsRepository repository,
		ReportingService reportingService,
		AllocationApplicationService allocationApplicationService,
		EmployeeManagementService employeeManagementService,
		ProjectManagementService projectManagementService)
	{
		_employeeService = employeeService;
		_matchingService = matchingService;
		_repository = repository;
		_reportingService = reportingService;
		_allocationApplicationService = allocationApplicationService;
		_employeeManagementService = employeeManagementService;
		_projectManagementService = projectManagementService;
	}

	bool exit = false;
	public async Task RunAsync(CancellationToken cancellationToken = default)
	{
		/// <summary>
		/// Main interactive menu loop for the console application. Presents options and routes user commands
		/// to application services. Blocking call until the user selects Exit.
		/// </summary>
		while (!exit)
		{
			global::System.Console.WriteLine();
			global::System.Console.WriteLine("=== Main Menu ===");
			global::System.Console.WriteLine("1. View Employees");
			global::System.Console.WriteLine("2. View Projects");
			global::System.Console.WriteLine("3. View Allocations");
			global::System.Console.WriteLine("4. Find Matching Employees for a Project");
			global::System.Console.WriteLine("5. View Project Staffing Summary");
			global::System.Console.WriteLine("6. View Bench Report");
			global::System.Console.WriteLine("7. View Department Utilisation");
			global::System.Console.WriteLine("8. Allocate Employee to Project");
			global::System.Console.WriteLine("9. Add Employee");
			global::System.Console.WriteLine("10. Update Employee");
			global::System.Console.WriteLine("11. Deactivate Employee");
			global::System.Console.WriteLine("12. Create Project");
			global::System.Console.WriteLine("13. Release Allocation");
			global::System.Console.WriteLine("0. Exit");
			global::System.Console.Write("Select an option: ");

			var choice = global::System.Console.ReadLine();

			switch (choice)
			{
				case "1":
					global::System.Console.WriteLine();
					global::System.Console.WriteLine("=== Employee List ===");

				var summaries = await _employeeService.GetEmployeeSummariesAsync(cancellationToken).ConfigureAwait(false);
				foreach (var employee in summaries)
				{
					global::System.Console.WriteLine(employee);
				}
					break;

				case "2":
					global::System.Console.WriteLine();
					global::System.Console.WriteLine("=== Project List ===");


				var projects = await _repository.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
				foreach (var project in projects)
				{
					global::System.Console.WriteLine($"{project.Id} | {project.Name} | {project.Status}");
				}
					break;

				case "3":
					global::System.Console.WriteLine();
					global::System.Console.WriteLine("=== Allocation List ===");


				var allocations = await _repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);
				foreach (var allocation in allocations)
				{
					global::System.Console.WriteLine(
						$"{allocation.EmployeeId} | {allocation.ProjectId}");
				}
					break;

				case "4":
					global::System.Console.WriteLine();
					global::System.Console.WriteLine("=== Find Matching Employees ===");


				var projectsList = await _repository.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
				foreach (var project in projectsList)
				{
					global::System.Console.WriteLine($"{project.Id} | {project.Name}");
				}

					Console.Write("Enter project ID: ");
					var projectId = Console.ReadLine();

				var selectedProject = projectsList.FirstOrDefault(
				p => string.Equals(p.Id, projectId, StringComparison.OrdinalIgnoreCase));

					if (selectedProject is null)
					{
						Console.WriteLine("Project not found.");
						break;
					}

				var employees = await _repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);
				var allocationsList = await _repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);
				var matchingEmployees = _matchingService.FindMatchingEmployees(
		  selectedProject, employees, allocationsList);

					Console.WriteLine();
					Console.WriteLine($"Matching employees for: {selectedProject.Name}");

					if (matchingEmployees.Count == 0)
					{
						Console.WriteLine("No matching employees found.");
					}
					else
					{
						foreach (var employee in matchingEmployees)
						{
							Console.WriteLine(
								$"{employee.Id} | {employee.FullName} | " +
								$"{employee.Designation} | {employee.Department}");
						}
					}

					break;


				case "5":
					Console.WriteLine();
					Console.WriteLine("=== Project Staffing Summary ===");

				var projectsForReport = await _repository.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
				var allocationsForReport = await _repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);
				var staffingReport = _reportingService.GetProjectStaffingSummary(projectsForReport, allocationsForReport);

					foreach (var row in staffingReport)
					{
						Console.WriteLine(
							$"{row.ProjectId} | {row.ProjectName} | " +
							$"Client: {row.ClientName} | Status: {row.Status} | " +
							$"Employees: {row.AssignedEmployeeCount} | " +
							$"Total Allocation: {row.TotalAllocationPercentage}%");
					}

					break;


				case "6":
					Console.WriteLine();
					Console.WriteLine("=== Bench Report ===");

					Console.Write("Enter report date (yyyy-MM-dd): ");
					var dateInput = Console.ReadLine();

					if (!DateOnly.TryParseExact(
							dateInput,
							"yyyy-MM-dd",
							out var reportDate))
					{
						Console.WriteLine("Invalid date. Use yyyy-MM-dd format.");
						break;
					}

					Console.Write("Enter allocation threshold (0-100): ");
					var thresholdInput = Console.ReadLine();

					if (!decimal.TryParse(thresholdInput, out var threshold)
						|| threshold < 0
						|| threshold > 100)
					{
						Console.WriteLine("Invalid threshold. Enter a value from 0 to 100.");
						break;
					}

				var employeesForBench = await _repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);
				var allocationsForBench = await _repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);
				var benchReport = _reportingService.GetBenchReport(employeesForBench, allocationsForBench, reportDate, threshold);

					Console.WriteLine();
					Console.WriteLine($"Bench Report for {reportDate}");
					Console.WriteLine($"Employees with allocation below {threshold}%:");

					if (benchReport.Count == 0)
					{
						Console.WriteLine("No employees found.");
					}
					else
					{
						foreach (var row in benchReport)
						{
							Console.WriteLine(
								$"{row.EmployeeId} | {row.EmployeeName} | " +
								$"{row.Department} | Allocated: {row.AllocatedPercentage}% | " +
								$"Available: {row.AvailablePercentage}%");
						}
					}

					break;


				case "7":
					Console.WriteLine();
					Console.WriteLine("=== Department Utilisation Report ===");

					Console.Write("Enter report date (yyyy-MM-dd): ");
					var utilisationDateInput = Console.ReadLine();

					if (!DateOnly.TryParseExact(
							utilisationDateInput,
							"yyyy-MM-dd",
							out var utilisationDate))
					{
						Console.WriteLine("Invalid date. Use yyyy-MM-dd format.");
						break;
					}

					var employeesForUtil = await _repository.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);
					var allocationsForUtil = await _repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);
					var utilisationReport = _reportingService.GetDepartmentUtilisation(employeesForUtil,
						allocationsForUtil,
						utilisationDate);

					Console.WriteLine();
					Console.WriteLine($"Department Utilisation for {utilisationDate}");
					Console.WriteLine();

					foreach (var row in utilisationReport)
					{
						Console.WriteLine(
							$"{row.Department} | " +
							$"Active Employees: {row.ActiveEmployeeCount} | " +
							$"Allocated: {row.TotalAllocatedPercentage}% | " +
							$"Utilisation: {row.UtilisationPercentage:F2}%");
					}

					break;


				case "8":
					try
					{
						Console.Write("Enter Employee ID: ");
						var employeeId = Console.ReadLine()?.Trim();

						Console.Write("Enter Project ID: ");
						var allocationProjectId = Console.ReadLine()?.Trim();

						Console.Write("Enter allocation start date (yyyy-MM-dd): ");
						var startInput = Console.ReadLine();

						Console.Write("Enter allocation end date (yyyy-MM-dd): ");
						var endInput = Console.ReadLine();

						Console.Write("Enter allocation percentage (1-100): ");
						var percentageInput = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(employeeId) ||
							string.IsNullOrWhiteSpace(allocationProjectId) ||
							!DateOnly.TryParseExact(
								startInput,
								"yyyy-MM-dd",
								System.Globalization.CultureInfo.InvariantCulture,
								System.Globalization.DateTimeStyles.None,
								out var startDate) ||
							!DateOnly.TryParseExact(
								endInput,
								"yyyy-MM-dd",
								System.Globalization.CultureInfo.InvariantCulture,
								System.Globalization.DateTimeStyles.None,
								out var endDate) ||
							!decimal.TryParse(
								percentageInput,
								System.Globalization.NumberStyles.Number,
								System.Globalization.CultureInfo.InvariantCulture,
								out var percentage) ||
							percentage <= 0 || percentage > 100 ||
							endDate < startDate)
						{
							Console.WriteLine(
								"Invalid input. Check IDs, dates, and percentage.");
							break;
						}

						var allocation = new TechNova.RMS.Domain.Allocations.Allocation(
							$"ALLOC{DateTime.Now:yyyyMMddHHmmssfff}",
							employeeId,
							allocationProjectId,
							startDate,
							endDate,
							percentage);

					await _allocationApplicationService.AllocateAsync(allocation, cancellationToken).ConfigureAwait(false);

					Console.WriteLine("Allocation saved successfully.");
					}
					catch (InvalidOperationException ex)
					{
						Console.WriteLine($"Allocation rejected: {ex.Message}");
					}
					catch (ArgumentException ex)
					{
						Console.WriteLine($"Invalid allocation: {ex.Message}");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Unable to save allocation: {ex.Message}");
					}

					break;


				case "9":
					try
					{
						Console.Write("Employee ID: ");
						var newId = Console.ReadLine()?.Trim();

						Console.Write("Full name: ");
						var fullName = Console.ReadLine()?.Trim();

						Console.Write("Email: ");
						var email = Console.ReadLine()?.Trim();

						Console.Write("Designation: ");
						var designation = Console.ReadLine()?.Trim();

						Console.Write("Department: ");
						var department = Console.ReadLine()?.Trim();

						Console.Write("Cost rate: ");
						var costRateInput = Console.ReadLine();

						Console.WriteLine("Employment types:");
						foreach (var type in Enum.GetValues<EmploymentType>())
							Console.WriteLine($"{(int)type}. {type}");

						Console.Write("Employment type number: ");
						var typeInput = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(newId) ||
							string.IsNullOrWhiteSpace(fullName) ||
							string.IsNullOrWhiteSpace(email) ||
							string.IsNullOrWhiteSpace(designation) ||
							string.IsNullOrWhiteSpace(department) ||
							!decimal.TryParse(costRateInput, out var costRate) ||
							!int.TryParse(typeInput, out var typeNumber) ||
							!Enum.IsDefined(typeof(EmploymentType), typeNumber))
						{
							Console.WriteLine("Invalid employee details.");
							break;
						}

						var newEmployee = new TechNova.RMS.Domain.Employees.Employee(
							newId, fullName, email, designation, department,
							costRate, (EmploymentType)typeNumber);

					await _employeeManagementService.AddEmployeeAsync(newEmployee, cancellationToken).ConfigureAwait(false);
					Console.WriteLine("Employee added successfully.");
					}
					catch (ArgumentException ex)
					{
						Console.WriteLine($"Invalid employee: {ex.Message}");
					}
					catch (InvalidOperationException ex)
					{
						Console.WriteLine($"Unable to add employee: {ex.Message}");
					}
					break;

				case "10":
					try
					{
						Console.Write("Employee ID to update: ");
						var updateId = Console.ReadLine()?.Trim();

						Console.Write("Full name: ");
						var updateName = Console.ReadLine()?.Trim();

						Console.Write("Email: ");
						var updateEmail = Console.ReadLine()?.Trim();

						Console.Write("Designation: ");
						var updateDesignation = Console.ReadLine()?.Trim();

						Console.Write("Department: ");
						var updateDepartment = Console.ReadLine()?.Trim();

						Console.Write("Cost rate: ");
						var updateCostInput = Console.ReadLine();

						Console.WriteLine("Employment types:");
						foreach (var type in Enum.GetValues<EmploymentType>())
							Console.WriteLine($"{(int)type}. {type}");

						Console.Write("Employment type number: ");
						var updateTypeInput = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(updateId) ||
							string.IsNullOrWhiteSpace(updateName) ||
							string.IsNullOrWhiteSpace(updateEmail) ||
							string.IsNullOrWhiteSpace(updateDesignation) ||
							string.IsNullOrWhiteSpace(updateDepartment) ||
							!decimal.TryParse(updateCostInput, out var updateCost) ||
							!int.TryParse(updateTypeInput, out var updateTypeNumber) ||
							!Enum.IsDefined(typeof(EmploymentType), updateTypeNumber))
						{
							Console.WriteLine("Invalid employee details.");
							break;
						}

				await _employeeManagementService.UpdateEmployeeAsync(
							updateId, updateName, updateEmail,
							updateDesignation, updateDepartment, updateCost,
							(EmploymentType)updateTypeNumber);

						Console.WriteLine("Employee updated successfully.");
					}
					catch (ArgumentException ex)
					{
						Console.WriteLine($"Invalid employee details: {ex.Message}");
					}
					catch (InvalidOperationException ex)
					{
						Console.WriteLine($"Unable to update employee: {ex.Message}");
					}
					break;

				case "11":
					try
					{
						Console.Write("Employee ID to deactivate: ");
						var deactivateId = Console.ReadLine()?.Trim();

						if (string.IsNullOrWhiteSpace(deactivateId))
						{
							Console.WriteLine("Employee ID is required.");
							break;
						}

				await _employeeManagementService.DeactivateEmployeeAsync(deactivateId, cancellationToken).ConfigureAwait(false);
						Console.WriteLine("Employee deactivated successfully.");
					}
					catch (InvalidOperationException ex)
					{
						Console.WriteLine($"Unable to deactivate employee: {ex.Message}");
					}
					break;



				case "12":
					try
					{
						Console.Write("Project ID: ");
						var newProjectId = Console.ReadLine()?.Trim();

						Console.Write("Project name: ");
						var projectName = Console.ReadLine()?.Trim();

						Console.Write("Client name: ");
						var clientName = Console.ReadLine()?.Trim();

						Console.Write("Start date (yyyy-MM-dd): ");
						var startInput = Console.ReadLine();

						Console.Write("End date (yyyy-MM-dd): ");
						var endInput = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(newProjectId) ||
							string.IsNullOrWhiteSpace(projectName) ||
							string.IsNullOrWhiteSpace(clientName) ||
							!DateOnly.TryParseExact(
								startInput, "yyyy-MM-dd",
								out var startDate) ||
							!DateOnly.TryParseExact(
								endInput, "yyyy-MM-dd",
								out var endDate) ||
							endDate < startDate)
						{
							Console.WriteLine("Invalid project details or dates.");
							break;
						}

						Console.WriteLine("Project statuses:");
						foreach (var status in Enum.GetValues<ProjectStatus>())
							Console.WriteLine($"{(int)status}. {status}");

						Console.Write("Project status number: ");
						var statusInput = Console.ReadLine();

						if (!int.TryParse(statusInput, out var statusNumber) ||
							!Enum.IsDefined(typeof(ProjectStatus), statusNumber))
						{
							Console.WriteLine("Invalid project status.");
							break;
						}

						var newProject = new TechNova.RMS.Domain.Projects.Project(
							newProjectId,
							projectName,
							clientName,
							startDate,
							endDate,
							(ProjectStatus)statusNumber);

				await _projectManagementService.AddProjectAsync(newProject, cancellationToken).ConfigureAwait(false);

						Console.WriteLine("Project created successfully.");
					}
					catch (ArgumentException ex)
					{
						Console.WriteLine($"Invalid project: {ex.Message}");
					}
					catch (InvalidOperationException ex)
					{
						Console.WriteLine($"Unable to create project: {ex.Message}");
					}
					break;


				case "13":
					try
					{
						Console.WriteLine("=== Current Allocations ===");


				var currentAllocations = await _repository.GetAllocationsAsync(cancellationToken).ConfigureAwait(false);
				foreach (var allocation in currentAllocations)
				{
					Console.WriteLine(
						$"{allocation.Id} | Employee: {allocation.EmployeeId} | " +
						$"Project: {allocation.ProjectId} | " +
						$"{allocation.AllocationPercentage}%");
				}

				Console.Write("Enter Allocation ID to release: ");
				var allocationId = Console.ReadLine()?.Trim();

				if (string.IsNullOrWhiteSpace(allocationId))
				{
					Console.WriteLine("Allocation ID is required.");
					break;
				}

				await _allocationApplicationService.ReleaseAsync(allocationId, cancellationToken).ConfigureAwait(false);
				Console.WriteLine("Allocation released successfully.");
					}
					catch (InvalidOperationException ex)
					{
						Console.WriteLine($"Unable to release allocation: {ex.Message}");
					}
					break;


				case "0":
					exit = true;
					Console.WriteLine("Exiting TechNova RMS. Goodbye!");
					break;

				default:
					Console.WriteLine("Invalid option. Please select an option from 0 to 11.");
					break;
			}
		}
	}
}