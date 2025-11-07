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

    [McpServerTool, Description("Get a list of all available Dataverse PowerShell cmdlets with their synopsis.")]
    public string GetCmdletList()
    {
        return _executor.GetCmdletList();
    }

    [McpServerTool, Description("Get detailed help information for a specific Dataverse PowerShell cmdlet.")]
    public string GetCmdletHelp(
        [Description("The name of the cmdlet to get help for (e.g., 'Get-DataverseRecord')")] string cmdletName)
    {
        if (string.IsNullOrWhiteSpace(cmdletName))
        {
            throw new McpException("Cmdlet name cannot be empty");
        }

        return _executor.GetCmdletHelp(cmdletName);
    }

    [McpServerTool, Description("Create a new persistent PowerShell session with the Dataverse module pre-loaded. Returns a session ID.")]
    public string CreateSession()
    {
        var sessionId = _executor.CreateSession();
        return JsonSerializer.Serialize(new
        {
            sessionId,
            message = "Session created. Use RunScriptInSession to execute scripts in this session."
        });
    }

    [McpServerTool, Description("Run a PowerShell script in an existing session. The session persists between script executions, maintaining variables and state.")]
    public string RunScriptInSession(
        [Description("The session ID returned from CreateSession")] string sessionId,
        [Description("The PowerShell script to execute in the session")] string script)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new McpException("SessionId cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(script))
        {
            throw new McpException("Script cannot be empty");
        }

        var scriptExecutionId = _executor.RunScriptInSession(sessionId, script);
        return JsonSerializer.Serialize(new
        {
            sessionId,
            scriptExecutionId,
            message = "Script execution started. Use GetScriptOutput to retrieve results."
        });
    }

    [McpServerTool, Description("Get the output from a script execution within a session.")]
    public string GetScriptOutput(
        [Description("The session ID")] string sessionId,
        [Description("The script execution ID returned from RunScriptInSession")] string scriptExecutionId,
        [Description("If true, only return new output since the last call. If false, return all output.")] bool onlyNew = false)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new McpException("SessionId cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(scriptExecutionId))
        {
            throw new McpException("ScriptExecutionId cannot be empty");
        }

        var result = _executor.GetScriptOutput(sessionId, scriptExecutionId, onlyNew);
        return JsonSerializer.Serialize(new
        {
            sessionId,
            scriptExecutionId = result.SessionId,
            result.Output,
            result.IsComplete,
            result.HasError,
            message = result.IsComplete
                ? (result.HasError ? "Script completed with errors" : "Script completed successfully")
                : "Script is still running"
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("End a PowerShell session and release all associated resources.")]
    public string EndSession(
        [Description("The session ID to end")] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new McpException("SessionId cannot be empty");
        }

        _executor.EndSession(sessionId);
        return JsonSerializer.Serialize(new
        {
            sessionId,
            message = "Session ended successfully."
        });
    }
}
