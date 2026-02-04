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

var rootCommand = new RootCommand("Dataverse PowerShell MCP Server - Execute PowerShell scripts with Dataverse module via Model Context Protocol")
{
    allowedUrlsOption
};

rootCommand.SetHandler(async (allowedUrls) =>
{
    // Normalize URLs (remove trailing slashes)
    var normalizedUrls = allowedUrls.Select(url => url.TrimEnd('/')).ToArray();

    var config = new PowerShellExecutorConfig
    {
        AllowedUrls = normalizedUrls
    };

    // STDIO mode
    var builder = Host.CreateApplicationBuilder();

    builder.Services.AddSingleton(config);
    builder.Services.AddSingleton<PowerShellExecutor>();
    builder.Services.AddSingleton<PowerShellTools>();

    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithTools<PowerShellTools>();

    builder.Logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });

    await builder.Build().RunAsync();
}, allowedUrlsOption);

return await rootCommand.InvokeAsync(args);
