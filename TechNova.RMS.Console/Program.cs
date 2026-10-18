using TechNova.RMS.ConsoleApp.UI;

var services = new ServiceCollection();

services.AddSingleton<IRmsRepository, JsonRmsRepository>();
services.AddSingleton<EmployeeMatchingService>();
services.AddSingleton<ReportingService>();
services.AddSingleton<EmployeeService>();
services.AddSingleton<AllocationService>();
services.AddSingleton<AllocationApplicationService>();
services.AddSingleton<EmployeeManagementService>();
services.AddSingleton<ProjectManagementService>();
services.AddSingleton<ConsoleMenu>();

using var serviceProvider = services.BuildServiceProvider();

var menu = serviceProvider.GetRequiredService<ConsoleMenu>();
menu.Run();