# Rnwood.Dataverse.Data.PowerShell.McpServer

A Model Context Protocol (MCP) server that exposes PowerShell with the Dataverse Data PowerShell module pre-loaded via STDIO transport.

## Overview

This MCP server allows AI assistants and other MCP clients to execute PowerShell scripts with the Rnwood.Dataverse.Data.PowerShell module pre-loaded. The server provides a configurable PowerShell environment with:

- Dataverse Data PowerShell module pre-loaded
- **Persistent sessions** - create sessions and run multiple scripts sequentially
- **Restricted language mode** by default (can be disabled)
- **Providers disabled** by default (can be enabled)
- Incremental output retrieval

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

The server supports several command-line options:

### Options

- `-c, --connection <name>` - Name of the saved Dataverse connection (or use `DATAVERSE_CONNECTION_NAME` env var)
- `-u, --unrestricted-mode` - Disable PowerShell restricted language mode (default: restricted mode enabled)
- `-p, --enable-providers` - Enable PowerShell providers like FileSystem, Registry, etc. (default: providers disabled)
- `--help` - Display help information

### Examples

**Basic usage with restricted mode (default):**
```bash
dotnet run --project McpServer.csproj -- --connection MyConnection
```

**Allow unrestricted PowerShell:**
```bash
dotnet run --project McpServer.csproj -- -c MyConnection --unrestricted-mode
```

**Enable filesystem and other providers:**
```bash
dotnet run --project McpServer.csproj -- -c MyConnection --enable-providers
```

**Full access (unrestricted mode + providers):**
```bash
dotnet run --project McpServer.csproj -- -c MyConnection -u -p
```

**Using environment variable for connection:**
```bash
export DATAVERSE_CONNECTION_NAME=MyConnection
dotnet run --project McpServer.csproj
```

## Security Modes

### Restricted Language Mode (Default)
- Limits PowerShell functionality for security
- Prevents access to .NET types and methods
- Best for untrusted script execution
- Use `--unrestricted-mode` to disable

### Provider Restrictions (Default)
- Disables FileSystem, Registry, and other providers
- Prevents file system access and modifications
- Use `--enable-providers` to enable providers

**⚠️ Warning**: Using `--unrestricted-mode` and `--enable-providers` removes safety restrictions. Only use in trusted environments.

## MCP Tools

The server exposes five MCP tools for session management and script execution:

### GetCmdletList

Returns a list of all available Dataverse PowerShell cmdlets with their synopsis.

**Parameters:** None

**Returns:**
- JSON array with objects containing:
  - `Name`: Cmdlet name
  - `Synopsis`: Brief description of what the cmdlet does

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

### CreateSession

Creates a new persistent PowerShell session with the Dataverse module and connection pre-loaded.

**Parameters:** None

**Returns:**
- JSON object with:
  - `sessionId`: Unique identifier for the session
  - `message`: Status message

**Usage:**
This creates a runspace that persists across multiple script executions. Variables and state are maintained between scripts.

### RunScriptInSession

Executes a PowerShell script in an existing persistent session.

**Parameters:**
- `sessionId` (string, required): The session ID from CreateSession
- `script` (string, required): The PowerShell script to execute

**Returns:**
- JSON object with:
  - `sessionId`: The session ID
  - `scriptExecutionId`: Unique ID for this script execution
  - `message`: Status message

**Usage:**
Scripts run in the same runspace, so variables and state persist. The `$connection` variable is pre-loaded.

### StartScript

Starts executing a PowerShell script with the Dataverse module pre-loaded and the default connection available as `$connection`.

**Parameters:**
- `script` (string, required): The PowerShell script to execute

**Returns:**
- JSON object with:
  - `sessionId`: Unique identifier for this script execution session
  - `message`: Status message


### GetScriptOutput

Retrieves output from a script execution within a persistent session.

**Parameters:**
- `sessionId` (string, required): The session ID from CreateSession
- `scriptExecutionId` (string, required): The script execution ID from RunScriptInSession
- `onlyNew` (boolean, optional): If true, returns only new output since the last call. If false, returns all output. Default: false

**Returns:**
- JSON object with:
  - `sessionId`: The session ID
  - `scriptExecutionId`: The script execution ID
  - `output`: The script output (stdout, stderr, warnings, verbose, etc.)
  - `isComplete`: Boolean indicating if the script has finished executing
  - `hasError`: Boolean indicating if the script encountered errors
  - `message`: Status message

**Example:**
```json
{
  "sessionId": "abc123...",
  "scriptExecutionId": "xyz789...",
  "onlyNew": false
}
```

### EndSession

Ends a PowerShell session and releases all associated resources.

**Parameters:**
- `sessionId` (string, required): The session ID to end

**Returns:**
- JSON object with:
  - `sessionId`: The ended session ID
  - `message`: Status message

**Usage:**
Always end sessions when done to free up resources.

## Usage with MCP Clients

### Claude Desktop Configuration

**Default configuration (restricted mode, providers disabled):**
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj",
        "--",
        "--connection",
        "MyConnection"
      ]
    }
  }
}
```

**Unrestricted mode with providers enabled:**
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj",
        "--",
        "-c",
        "MyConnection",
        "--unrestricted-mode",
        "--enable-providers"
      ]
    }
  }
}
```

**Using environment variable:**
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj"
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
      "args": ["--connection", "MyConnection"]
    }
  }
}
```

## Example Workflows

### Single Script Execution (Old Pattern - Still Supported)

For backward compatibility, you can use sessions for one-off script execution:

1. Create a session: `CreateSession`
2. Run a script: `RunScriptInSession` with your script
3. Get output: `GetScriptOutput` 
4. End session: `EndSession`

### Persistent Session (Recommended Pattern)

For interactive work with state preservation:

1. Save a connection using PowerShell:
   ```powershell
   Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -Name "MyConnection" -SetAsDefault
   ```

2. Configure and start the MCP server

3. AI assistant discovers available cmdlets with `GetCmdletList`

4. AI assistant gets help for specific cmdlets with `GetCmdletHelp`

5. AI assistant creates a persistent session with `CreateSession`

6. AI assistant runs multiple scripts in sequence in the same session:
   - First script: `RunScriptInSession` - e.g., `$accounts = Get-DataverseRecord -Connection $connection -TableName account -Top 10`
   - Get results: `GetScriptOutput`
   - Second script: `RunScriptInSession` - e.g., `$accounts | Select-Object name, accountnumber` (uses $accounts from previous script)
   - Get results: `GetScriptOutput`

7. AI assistant ends the session with `EndSession` when done

7. AI assistant ends the session with `EndSession` when done

## Security Considerations

### Default Security (Recommended)
- **Restricted Language Mode**: Limits PowerShell functionality, prevents access to .NET types
- **Providers Disabled**: No FileSystem, Registry, or other provider access
- **Module Restriction**: Only the Dataverse Data PowerShell module is pre-loaded
- **Session Isolation**: Each session is isolated from others

### With --unrestricted-mode
- Allows full PowerShell language features
- Access to .NET types and methods
- **Use only in fully trusted environments**

### With --enable-providers
- Enables FileSystem, Registry, and other providers
- Allows file system access and modifications
- **Use only when file operations are required and trusted**

⚠️ **Warning**: This server executes arbitrary PowerShell code. Using `--unrestricted-mode` and `--enable-providers` removes safety restrictions. Only use in trusted environments with trusted clients.

## Architecture

The server consists of:

- **Program.cs**: Entry point using System.CommandLine for argument parsing, configures the MCP server with STDIO transport
- **PowerShellTools.cs**: MCP tool definitions (GetCmdletList, GetCmdletHelp, CreateSession, RunScriptInSession, GetScriptOutput, EndSession)
- **PowerShellExecutor.cs**: Manages PowerShell sessions and script execution
  - **PowerShellExecutorConfig**: Configuration for language mode and provider restrictions
  - **PersistentSession**: Maintains a PowerShell runspace across multiple script executions
  - **ScriptExecution**: Tracks individual script execution within a session
  - Validates named connection on startup
  - Creates PowerShell runspaces with configurable restrictions
  - Loads the Dataverse module and default connection
  - Provides cmdlet discovery and help retrieval
  - Captures output, errors, warnings, and verbose messages
  - Tracks script completion (not runspace completion)

## Limitations

- Sessions are stored in memory and lost on server restart
- No support for interactive input (prompts, confirmations)
- No support for UI elements (progress bars, etc.)
- File system operations require `--enable-providers` flag
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
