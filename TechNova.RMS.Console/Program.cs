using Microsoft.Extensions.DependencyInjection;
using TechNova.RMS.Application.Persistence;
using TechNova.RMS.Application.Services;
using TechNova.RMS.ConsoleApp.UI;
using TechNova.RMS.Domain.Services;
using TechNova.RMS.Infrastructure.Persistence;

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