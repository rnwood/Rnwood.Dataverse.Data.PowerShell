using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Rnwood.Dataverse.Data.PowerShell.McpServer.Tools;

[McpServerToolType]
public class PowerShellTools
{
    private readonly PowerShellExecutor _executor;

    public PowerShellTools(PowerShellExecutor executor)
    {
        _executor = executor;
    }

    [McpServerTool, Description("Start executing a PowerShell script with the Dataverse module pre-loaded. Returns a session ID to retrieve output later.")]
    public string StartScript(
        [Description("The PowerShell script to execute")] string script)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            throw new McpException("Script cannot be empty");
        }

        var sessionId = _executor.StartScript(script);
        return JsonSerializer.Serialize(new
        {
            sessionId,
            message = "Script execution started. Use GetScriptOutput to retrieve results."
        });
    }

    [McpServerTool, Description("Get the output from a running or completed PowerShell script session.")]
    public string GetScriptOutput(
        [Description("The session ID returned from StartScript")] string sessionId,
        [Description("If true, only return new output since the last call. If false, return all output.")] bool onlyNew = false)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new McpException("SessionId cannot be empty");
        }

        var result = _executor.GetOutput(sessionId, onlyNew);
        return JsonSerializer.Serialize(new
        {
            result.SessionId,
            result.Output,
            result.IsComplete,
            result.HasError,
            message = result.IsComplete
                ? (result.HasError ? "Script completed with errors" : "Script completed successfully")
                : "Script is still running"
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
