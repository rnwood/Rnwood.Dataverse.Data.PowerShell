using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rnwood.Dataverse.Data.PowerShell.McpServer.Tools;

// Define command line options
var allowedUrlsOption = new Option<string[]>(
    name: "--allowed-urls",
    description: "List of allowed Dataverse URLs for connections (required). Connections can only be made to these URLs. Server will auto-connect to first URL.")
{
    IsRequired = true,
    AllowMultipleArgumentsPerToken = true
};
allowedUrlsOption.AddAlias("-u");

var enableProvidersOption = new Option<bool>(
    name: "--enable-providers",
    description: "Enable PowerShell providers (FileSystem, Registry, etc.)",
    getDefaultValue: () => false);
enableProvidersOption.AddAlias("-p");

var unrestrictedModeOption = new Option<bool>(
    name: "--unrestricted-mode",
    description: "Disable restricted language mode (enables full PowerShell features)",
    getDefaultValue: () => false);
unrestrictedModeOption.AddAlias("-r");

var rootCommand = new RootCommand("Dataverse PowerShell MCP Server - Execute PowerShell scripts with Dataverse module via Model Context Protocol")
{
    allowedUrlsOption,
    enableProvidersOption,
    unrestrictedModeOption
};

rootCommand.SetHandler(async (allowedUrls, enableProviders, unrestrictedMode) =>
{
    // Normalize URLs (remove trailing slashes)
    var normalizedUrls = allowedUrls.Select(url => url.TrimEnd('/')).ToArray();

    var config = new PowerShellExecutorConfig
    {
        AllowedUrls = normalizedUrls,
        EnableProviders = enableProviders,
        UnrestrictedMode = unrestrictedMode
    };

    // STDIO mode
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
}, allowedUrlsOption, enableProvidersOption, unrestrictedModeOption);

return await rootCommand.InvokeAsync(args);
