# Rnwood.Dataverse.Data.PowerShell.McpServer

A Model Context Protocol (MCP) server that exposes PowerShell with the Dataverse Data PowerShell module pre-loaded via STDIO transport.

## Overview

This MCP server allows AI assistants and other MCP clients to execute PowerShell scripts with the Rnwood.Dataverse.Data.PowerShell module pre-loaded. The server provides a configurable PowerShell environment with:

- Dataverse Data PowerShell module pre-loaded
- **Persistent sessions** - create sessions and run multiple scripts sequentially
- **URL allowlist** - restricts connections to specified Dataverse URLs only
- **Auto-connection** - automatically connects to the first allowed URL on session creation
- Incremental output retrieval

## Requirements

- .NET 8.0 or later
- PowerShell 7.4.6 or later (provided via Microsoft.PowerShell.SDK)
- Built Rnwood.Dataverse.Data.PowerShell module
- **One or more allowed Dataverse URLs** (required parameter)

## Installation

### Using `dnx` (Recommended - No Installation Required)

Run the MCP server directly from NuGet without installing it globally using the new `dnx` command (available in .NET 10 SDK):

```bash
dnx rnwood-dataverse-mcp --allowed-urls https://myorg.crm.dynamics.com
```

This approach:
- **No installation needed** - downloads and runs the tool on-demand
- **Always uses the latest version** - automatically fetches updates from NuGet
- **Simpler syntax** - streamlined command compared to `dotnet exec`
- **No global PATH pollution** - doesn't install anything permanently

> **Note**: The `dnx` command is available in .NET 10 SDK and later. For earlier versions of .NET, use the global tool installation method below.

### As a .NET Global Tool (Alternative)

Install the MCP server as a global tool from NuGet.org:

```bash
dotnet tool install --global Rnwood.Dataverse.Data.PowerShell.McpServer
```

To update to the latest version:
```bash
dotnet tool update --global Rnwood.Dataverse.Data.PowerShell.McpServer
```

To uninstall:
```bash
dotnet tool uninstall --global Rnwood.Dataverse.Data.PowerShell.McpServer
```

### From Source

```bash
dotnet build Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj
```

## Running

The server requires specifying allowed Dataverse URLs and supports several command-line options:

### Required Options

- `-u, --allowed-urls <url1> <url2> ...` - List of allowed Dataverse URLs for connections. Connections can only be made to these URLs. The server will auto-connect to the first URL on session creation.

### Optional Flags

- `--help` - Display help information

### Examples

**Basic usage (auto-connect to first URL):**
```bash
rnwood-dataverse-mcp --allowed-urls https://myorg.crm.dynamics.com
```

**Multiple allowed URLs:**
```bash
rnwood-dataverse-mcp --allowed-urls https://dev.crm.dynamics.com https://prod.crm.dynamics.com
```

## URL Restrictions and Auto-Connection

### How it Works

1. **URL Allowlist**: The server wraps the `Get-DataverseConnection` cmdlet to enforce the list of allowed URLs. Any attempt to connect to a URL not in the allowlist will fail with an error message.

2. **Auto-Connection**: On session creation, the server automatically creates an interactive connection to the first allowed URL and sets it as the default.

## Claude Desktop Integration

Configure the server in Claude Desktop's `claude_desktop_config.json`:

### Using `dnx` (Recommended - No Installation Required)

**Basic configuration:**
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "dnx",
      "args": [
        "rnwood-dataverse-mcp",
        "--allowed-urls",
        "https://myorg.crm.dynamics.com"
      ]
    }
  }
}
```

**With multiple environments:**
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "dnx",
      "args": [
        "rnwood-dataverse-mcp",
        "--allowed-urls",
        "https://dev.crm.dynamics.com",
        "https://test.crm.dynamics.com",
        "https://prod.crm.dynamics.com"
      ]
    }
  }
}
```

### Using Global Tool (Alternative)

If you prefer to install the tool globally first:

```bash
dotnet tool install --global Rnwood.Dataverse.Data.PowerShell.McpServer
```

Then configure:
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "rnwood-dataverse-mcp",
      "args": [
        "--allowed-urls",
        "https://myorg.crm.dynamics.com"
      ]
    }
  }
}
```

## Available MCP Tools

The server exposes the following MCP tools:

### GetCmdletList

Returns a list of all available Dataverse cmdlets with their synopsis.

**Returns:** JSON array of cmdlets with name and synopsis

### GetCmdletHelp

Returns detailed help for a specific cmdlet including description, parameters, and examples.

**Parameters:**
- `cmdletName` (string) - Name of the cmdlet

**Returns:** JSON object with detailed help information

### CreateSession

Creates a new persistent PowerShell session with the Dataverse module pre-loaded and auto-connects to the first allowed URL.

**Returns:** Session ID (string)

**Notes:**
- Session is initialized with `$connection` variable containing the default connection
- Variables and state persist across script executions within the session

### RunScriptInSession

Executes a PowerShell script in an existing session.

**Parameters:**
- `sessionId` (string) - Session ID from CreateSession
- `script` (string) - PowerShell script to execute

**Returns:** Script execution ID (string)

**Notes:**
- Script runs asynchronously
- Use GetScriptOutput to retrieve results

### GetScriptOutput

Retrieves output from a script execution.

**Parameters:**
- `sessionId` (string) - Session ID
- `scriptExecutionId` (string) - Script execution ID from RunScriptInSession
- `onlyNew` (boolean) - If true, returns only output since last call; if false, returns all output

**Returns:** JSON object with:
- `isComplete` (boolean) - Whether script execution has finished
- `output` (string) - Script output
- `error` (string) - Error output if any

### EndSession

Closes and cleans up a persistent session.

**Parameters:**
- `sessionId` (string) - Session ID to end
