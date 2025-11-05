using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rnwood.Dataverse.Data.PowerShell.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Get connection name from environment variable or command line argument
string? connectionName = Environment.GetEnvironmentVariable("DATAVERSE_CONNECTION_NAME");
if (args.Length > 0)
{
    connectionName = args[0];
}

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<PowerShellTools>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<PowerShellExecutor>(sp => new PowerShellExecutor(connectionName));

await builder.Build().RunAsync();
