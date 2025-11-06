using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rnwood.Dataverse.Data.PowerShell.McpServer.Tools;

// Define command line options
var connectionNameOption = new Option<string?>(
    name: "--connection",
    description: "The name of the saved Dataverse connection to use")
{
    IsRequired = false
};
connectionNameOption.AddAlias("-c");

var enableProvidersOption = new Option<bool>(
    name: "--enable-providers",
    description: "Enable PowerShell providers (FileSystem, Registry, etc.)",
    getDefaultValue: () => false);
enableProvidersOption.AddAlias("-p");

var httpModeOption = new Option<bool>(
    name: "--http",
    description: "Run in HTTP mode instead of STDIO mode (uses ASP.NET environment variables and command line args for bindings)",
    getDefaultValue: () => false);
httpModeOption.AddAlias("-h");

var rootCommand = new RootCommand("Dataverse PowerShell MCP Server - Execute PowerShell scripts with Dataverse module via Model Context Protocol")
{
    connectionNameOption,
    enableProvidersOption,
    httpModeOption
};

rootCommand.SetHandler(async (connectionName, enableProviders, httpMode) =>
{
    // Check environment variable if connection name not provided
    connectionName ??= Environment.GetEnvironmentVariable("DATAVERSE_CONNECTION_NAME");

    var config = new PowerShellExecutorConfig
    {
        ConnectionName = connectionName,
        EnableProviders = enableProviders
    };

    if (httpMode)
    {
        // HTTP mode - use ASP.NET Core with MCP HTTP transport
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddConsole();

        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton<PowerShellExecutor>();

        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithTools<PowerShellTools>();

        var app = builder.Build();
        
        app.MapMcp();

        await app.RunAsync();
    }
    else
    {
        // STDIO mode (default)
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<PowerShellTools>();

        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton<PowerShellExecutor>();

        await builder.Build().RunAsync();
    }
}, connectionNameOption, enableProvidersOption, httpModeOption);

return await rootCommand.InvokeAsync(args);
