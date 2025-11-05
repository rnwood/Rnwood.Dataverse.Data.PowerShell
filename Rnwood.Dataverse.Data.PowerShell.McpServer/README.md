# Rnwood.Dataverse.Data.PowerShell.McpServer

A Model Context Protocol (MCP) server that exposes PowerShell with the Dataverse Data PowerShell module pre-loaded via STDIO transport.

## Overview

This MCP server allows AI assistants and other MCP clients to execute PowerShell scripts with the Rnwood.Dataverse.Data.PowerShell module pre-loaded. The server provides a sandboxed PowerShell environment where:

- Only the Dataverse Data PowerShell module is available
- File system, registry, and other default PowerShell providers are disabled
- Scripts run in isolated sessions with unique identifiers
- Output can be retrieved incrementally or in full

## Requirements

- .NET 8.0 or later
- PowerShell 7.4.6 or later (provided via Microsoft.PowerShell.SDK)
- Built Rnwood.Dataverse.Data.PowerShell module

## Building

```bash
dotnet build Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj
```

## Running

The server uses STDIO transport, which means it communicates over standard input/output:

```bash
dotnet run --project Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj
```

## MCP Tools

The server exposes two MCP tools:

### StartScript

Starts executing a PowerShell script with the Dataverse module pre-loaded.

**Parameters:**
- `script` (string, required): The PowerShell script to execute

**Returns:**
- JSON object with:
  - `sessionId`: Unique identifier for this script execution session
  - `message`: Status message

**Example:**
```json
{
  "script": "Get-Command -Module Rnwood.Dataverse.Data.PowerShell"
}
```

### GetScriptOutput

Retrieves output from a running or completed PowerShell script session.

**Parameters:**
- `sessionId` (string, required): The session ID returned from StartScript
- `onlyNew` (boolean, optional): If true, returns only new output since the last call. If false, returns all output. Default: false

**Returns:**
- JSON object with:
  - `sessionId`: The session ID
  - `output`: The script output (stdout, stderr, warnings, verbose, etc.)
  - `isComplete`: Boolean indicating if the script has finished executing
  - `hasError`: Boolean indicating if the script encountered errors
  - `message`: Status message

**Example:**
```json
{
  "sessionId": "abc123...",
  "onlyNew": false
}
```

## Usage with MCP Clients

### Claude Desktop Configuration

Add to your Claude Desktop configuration (`~/Library/Application Support/Claude/claude_desktop_config.json` on macOS):

```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj"
      ]
    }
  }
}
```

### Using the Published Binary

If you publish the server as a standalone executable:

```bash
dotnet publish -c Release -r linux-x64 --self-contained
```

Then update the configuration:

```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer"
    }
  }
}
```

## Example Workflow

1. Client calls `StartScript` with a PowerShell script
2. Server returns a session ID
3. Client polls `GetScriptOutput` with the session ID to retrieve results
4. When `isComplete` is true, the script has finished executing

## Security Considerations

- **Sandboxing**: The PowerShell environment has most providers disabled (FileSystem, Registry, etc.)
- **Module Restriction**: Only the Dataverse Data PowerShell module is pre-loaded
- **Isolation**: Each script runs in its own session
- **No Persistence**: Sessions are not persisted across server restarts

⚠️ **Warning**: This server executes arbitrary PowerShell code. Only use it in trusted environments and with trusted clients.

## Architecture

The server consists of:

- **Program.cs**: Entry point that configures the MCP server with STDIO transport
- **PowerShellTools.cs**: MCP tool definitions that expose StartScript and GetScriptOutput
- **PowerShellExecutor.cs**: Manages PowerShell sessions and script execution
  - Creates minimal PowerShell runspaces with providers disabled
  - Loads the Dataverse module
  - Captures output, errors, warnings, and verbose messages
  - Tracks session state and completion

## Limitations

- Sessions are stored in memory and lost on server restart
- No support for interactive input (prompts, confirmations)
- No support for UI elements (progress bars, etc.)
- File system operations are disabled by default

## Troubleshooting

### Module Not Found

If the Dataverse module fails to load, ensure:
1. The main solution has been built
2. The module manifest exists at `Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1`

### Server Not Responding

Check stderr for error messages. The server logs to stderr (not stdout) to avoid interfering with MCP protocol messages.

## Development

To debug the server:

1. Set breakpoints in Visual Studio or VS Code
2. Start debugging the McpServer project
3. Provide test input via stdin or use a test harness

## License

Same as the main Rnwood.Dataverse.Data.PowerShell project.
