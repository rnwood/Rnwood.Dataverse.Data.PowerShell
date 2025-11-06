using System;
using System.CommandLine;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

var unrestrictedModeOption = new Option<bool>(
    name: "--unrestricted-mode",
    description: "Disable PowerShell restricted language mode (allows unrestricted script execution)",
    getDefaultValue: () => false);
unrestrictedModeOption.AddAlias("-u");

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
    unrestrictedModeOption,
    enableProvidersOption,
    httpModeOption
};

rootCommand.SetHandler(async (connectionName, unrestrictedMode, enableProviders, httpMode) =>
{
    // Check environment variable if connection name not provided
    connectionName ??= Environment.GetEnvironmentVariable("DATAVERSE_CONNECTION_NAME");

    var config = new PowerShellExecutorConfig
    {
        ConnectionName = connectionName,
        UseRestrictedLanguageMode = !unrestrictedMode,
        EnableProviders = enableProviders
    };

    if (httpMode)
    {
        // HTTP mode - use ASP.NET Core web host with direct tool invocation
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddConsole();

        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton<PowerShellExecutor>();
        builder.Services.AddSingleton<PowerShellTools>();

        var app = builder.Build();
        
        // Map JSON-RPC style HTTP endpoint
        app.MapPost("/mcp", async (HttpContext context) =>
        {
            var tools = context.RequestServices.GetRequiredService<PowerShellTools>();
            
            // Simple JSON-RPC handler - parse request and call appropriate tool method
            var request = await context.Request.ReadFromJsonAsync<JsonRpcRequest>();
            
            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid request" });
                return;
            }

            object result;
            try
            {
                result = request.Method switch
                {
                    "initialize" => new
                    {
                        protocolVersion = "2024-11-05",
                        capabilities = new { tools = new { } },
                        serverInfo = new { name = "dataverse-powershell-mcp", version = "1.0.0" }
                    },
                    "tools/list" => tools.GetCmdletList(),
                    "tools/call" => HandleToolCall(tools, request),
                    _ => new { error = $"Unknown method: {request.Method}" }
                };
            }
            catch (Exception ex)
            {
                result = new { error = ex.Message };
            }

            var response = new
            {
                jsonrpc = "2.0",
                id = request.Id,
                result
            };
            
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        });

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
}, connectionNameOption, unrestrictedModeOption, enableProvidersOption, httpModeOption);

return await rootCommand.InvokeAsync(args);

static object HandleToolCall(PowerShellTools tools, JsonRpcRequest request)
{
    var toolName = request.Params?.GetProperty("name").GetString();
    var arguments = request.Params?.GetProperty("arguments");

    return toolName switch
    {
        "GetCmdletList" => tools.GetCmdletList(),
        "GetCmdletHelp" => tools.GetCmdletHelp(arguments?.GetProperty("cmdletName").GetString() ?? ""),
        "CreateSession" => tools.CreateSession(),
        "RunScriptInSession" => tools.RunScriptInSession(
            arguments?.GetProperty("sessionId").GetString() ?? "",
            arguments?.GetProperty("script").GetString() ?? ""),
        "GetScriptOutput" => tools.GetScriptOutput(
            arguments?.GetProperty("sessionId").GetString() ?? "",
            arguments?.GetProperty("scriptExecutionId").GetString() ?? "",
            arguments?.TryGetProperty("onlyNew", out var onlyNewVal) == true && onlyNewVal.GetBoolean()),
        "EndSession" => tools.EndSession(arguments?.GetProperty("sessionId").GetString() ?? ""),
        _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" })
    };
}

record JsonRpcRequest(string Jsonrpc, object? Id, string Method, System.Text.Json.JsonElement? Params);
