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
- **A saved Dataverse connection** (required for startup)

## Setup: Saving a Connection

Before running the MCP server, you must save a named connection:

```powershell
# Install and import the module
Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
Import-Module Rnwood.Dataverse.Data.PowerShell

# Save a connection with a name
Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -Name "MyConnection" -SetAsDefault
```

To list saved connections:
```powershell
Get-DataverseConnection -List
```

## Building

```bash
dotnet build Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj
```

## Running

The server requires a connection name to be specified either via command line argument or environment variable.

**Option 1: Command line argument**
```bash
dotnet run --project Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj MyConnection
```

**Option 2: Environment variable**
```bash
export DATAVERSE_CONNECTION_NAME=MyConnection
dotnet run --project Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj
```

If no connection name is provided or the connection cannot be loaded, the server will fail with instructions on how to save a connection.

## MCP Tools

The server exposes four MCP tools:

### GetCmdletList

Returns a list of all available Dataverse PowerShell cmdlets with their synopsis.

**Parameters:** None

**Returns:**
- JSON array with objects containing:
  - `Name`: Cmdlet name
  - `Synopsis`: Brief description of what the cmdlet does

**Example usage:**
Get a list of all available cmdlets to discover functionality.

### GetCmdletHelp

Returns detailed help information for a specific cmdlet.

**Parameters:**
- `cmdletName` (string, required): The name of the cmdlet (e.g., "Get-DataverseRecord")

**Returns:**
- JSON object with:
  - `Name`: Cmdlet name
  - `Synopsis`: Brief description
  - `Description`: Detailed description
  - `Syntax`: Command syntax
  - `Parameters`: Array of parameter details (name, type, required, description)
  - `Examples`: Array of usage examples

**Example:**
```json
{
  "cmdletName": "Get-DataverseRecord"
}
```

### StartScript

Starts executing a PowerShell script with the Dataverse module pre-loaded and the default connection available as `$connection`.

**Parameters:**
- `script` (string, required): The PowerShell script to execute

**Returns:**
- JSON object with:
  - `sessionId`: Unique identifier for this script execution session
  - `message`: Status message

**Example:**
```json
{
  "script": "Get-DataverseRecord -Connection $connection -TableName contact -Top 10"
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
        "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj",
        "MyConnection"
      ]
    }
  }
}
```

Or using environment variable:

```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj"
      ],
      "env": {
        "DATAVERSE_CONNECTION_NAME": "MyConnection"
      }
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
      "command": "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer",
      "args": ["MyConnection"]
    }
  }
}
```

## Example Workflow

1. Save a connection using PowerShell:
   ```powershell
   Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -Name "MyConnection" -SetAsDefault
   ```

2. Configure the MCP server with the connection name

3. AI assistant discovers available cmdlets with `GetCmdletList`

4. AI assistant gets help for specific cmdlets with `GetCmdletHelp`

5. AI assistant executes scripts with `StartScript` - the `$connection` variable is pre-loaded

6. AI assistant polls `GetScriptOutput` to retrieve results

7. When `isComplete` is true, the script has finished executing

## Security Considerations

- **Sandboxing**: The PowerShell environment has most providers disabled (FileSystem, Registry, etc.)
- **Module Restriction**: Only the Dataverse Data PowerShell module is pre-loaded
- **Isolation**: Each script runs in its own session
- **No Persistence**: Sessions are not persisted across server restarts

⚠️ **Warning**: This server executes arbitrary PowerShell code. Only use it in trusted environments and with trusted clients.

## Architecture

The server consists of:

- **Program.cs**: Entry point that configures the MCP server with STDIO transport and loads the named connection
- **PowerShellTools.cs**: MCP tool definitions (GetCmdletList, GetCmdletHelp, StartScript, GetScriptOutput)
- **PowerShellExecutor.cs**: Manages PowerShell sessions and script execution
  - Validates named connection on startup
  - Creates minimal PowerShell runspaces with providers disabled
  - Loads the Dataverse module and default connection
  - Provides cmdlet discovery and help retrieval
  - Captures output, errors, warnings, and verbose messages
  - Tracks session state and completion

## Limitations

- Sessions are stored in memory and lost on server restart
- No support for interactive input (prompts, confirmations)
- No support for UI elements (progress bars, etc.)
- File system operations are disabled by default
- Requires a pre-saved named connection to start

## Troubleshooting

### Connection Not Found

If the server fails to start with "Failed to load named connection":
1. Save a connection using: `Get-DataverseConnection -Url <url> -Interactive -Name <name> -SetAsDefault`
2. List saved connections: `Get-DataverseConnection -List`
3. Ensure the connection name matches exactly (case-sensitive)

### Module Not Found

If the Dataverse module fails to load, ensure:
1. The main solution has been built: `dotnet build`
2. The module manifest exists at `Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1`
3. Set `DATAVERSE_MODULE_PATH` environment variable if using a custom location

### Server Not Responding

Check stderr for error messages. The server logs to stderr (not stdout) to avoid interfering with MCP protocol messages.

## Development

To debug the server:

1. Set breakpoints in Visual Studio or VS Code
2. Start debugging the McpServer project
3. Provide test input via stdin or use a test harness

## License

Same as the main Rnwood.Dataverse.Data.PowerShell project.
