# Rnwood.Dataverse.Data.PowerShell.McpServer

A Model Context Protocol (MCP) server that exposes PowerShell with the Dataverse Data PowerShell module pre-loaded via STDIO transport.

## Overview

This MCP server allows AI assistants and other MCP clients to execute PowerShell scripts with the Rnwood.Dataverse.Data.PowerShell module pre-loaded. The server provides a configurable PowerShell environment with:

- Dataverse Data PowerShell module pre-loaded
- **Persistent sessions** - create sessions and run multiple scripts sequentially
- **Restricted language mode** by default (can be disabled with `--unrestricted-mode`)
- **Providers disabled** by default (can be enabled with `--enable-providers`)
- **URL allowlist** - restricts connections to specified Dataverse URLs only
- **Auto-connection** - automatically connects to the first allowed URL on session creation
- Incremental output retrieval

## Requirements

- .NET 8.0 or later
- PowerShell 7.4.6 or later (provided via Microsoft.PowerShell.SDK)
- Built Rnwood.Dataverse.Data.PowerShell module
- **One or more allowed Dataverse URLs** (required parameter)

## Installation

### As a .NET Global Tool (Recommended)

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

- `-r, --unrestricted-mode` - Disable PowerShell restricted language mode (default: restricted mode enabled)
- `-p, --enable-providers` - Enable PowerShell providers like FileSystem, Registry, etc. (default: providers disabled)
- `--help` - Display help information

### Examples

**Basic usage (restricted mode, no providers, auto-connect to first URL):**
```bash
rnwood-dataverse-mcp --allowed-urls https://myorg.crm.dynamics.com
```

**Multiple allowed URLs:**
```bash
rnwood-dataverse-mcp --allowed-urls https://dev.crm.dynamics.com https://prod.crm.dynamics.com
```

**With unrestricted mode and providers enabled:**
```bash
rnwood-dataverse-mcp -u https://myorg.crm.dynamics.com --unrestricted-mode --enable-providers
```

## URL Restrictions and Auto-Connection

### How it Works

1. **URL Allowlist**: The server wraps the `Get-DataverseConnection` cmdlet to enforce the list of allowed URLs. Any attempt to connect to a URL not in the allowlist will fail with an error message.

2. **Auto-Connection**: On session creation, the server automatically creates an interactive connection to the first allowed URL and sets it as the default. The `$connection` variable is pre-loaded and ready to use.

3. **Default Connection Handling**: If a script tries to get the default connection and none exists, the server automatically creates an interactive connection to the first allowed URL.

### Security Benefits

- **Prevents data exfiltration**: Scripts cannot connect to arbitrary URLs
- **Organizational control**: Administrators can restrict which Dataverse environments are accessible
- **Audit trail**: All connections are limited to known, approved URLs

## Claude Desktop Integration

Configure the server in Claude Desktop's `claude_desktop_config.json`:

**Basic configuration:**
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

**With multiple environments:**
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "rnwood-dataverse-mcp",
      "args": [
        "--allowed-urls",
        "https://dev.crm.dynamics.com",
        "https://test.crm.dynamics.com",
        "https://prod.crm.dynamics.com"
      ]
    }
  }
}
```

**With unrestricted mode and providers:**
```json
{
  "mcpServers": {
    "dataverse-powershell": {
      "command": "rnwood-dataverse-mcp",
      "args": [
        "--allowed-urls",
        "https://myorg.crm.dynamics.com",
        "--unrestricted-mode",
        "--enable-providers"
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

## Security Considerations

### Default Security Posture

- **Restricted Language Mode**: Prevents access to .NET types and methods, limiting what scripts can do
- **Providers Disabled**: No access to FileSystem, Registry, or other PowerShell providers
- **URL Allowlist**: Connections restricted to specified Dataverse environments only
- **Isolated Sessions**: Each session runs in its own isolated runspace

### Relaxed Security (Use with Caution)

- **`--unrestricted-mode`**: Enables full PowerShell language features including .NET type access
- **`--enable-providers`**: Enables filesystem and registry access

⚠️ **Warning**: Only use unrestricted mode and enabled providers in trusted environments with trusted clients, as they significantly expand the attack surface.

### Recommended Best Practices

1. **Minimal URL List**: Only include necessary Dataverse environments in the allowed URLs list
2. **Least Privilege**: Keep restricted mode and disabled providers unless specifically needed
3. **Audit Access**: Monitor which environments are accessed and by whom
4. **Separate Environments**: Use different MCP server instances for dev/test/prod with appropriate URL restrictions

## Development

### Module Path Resolution

The server automatically discovers the module path in this order:

1. Packaged module directory (for global tool): `{assembly-dir}/module/`
2. Development Debug build: `../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/`
3. Development Release build: `../Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/`
4. Environment variable: `DATAVERSE_MODULE_PATH`

### Running from Source

```bash
# Build the main module first
dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell/Rnwood.Dataverse.Data.PowerShell.csproj

# Run the MCP server
dotnet run --project ./Rnwood.Dataverse.Data.PowerShell.McpServer/Rnwood.Dataverse.Data.PowerShell.McpServer.csproj -- --allowed-urls https://myorg.crm.dynamics.com
```

## Troubleshooting

### Connection Issues

If you encounter connection issues:

1. **Verify URL is in allowlist**: Ensure the URL you're trying to connect to is in the `--allowed-urls` list
2. **Check URL format**: URLs should be in format `https://yourorg.crm.dynamics.com` (no trailing slash)
3. **Authentication**: The auto-connection uses interactive authentication - ensure browser authentication works
4. **Saved Connections**: If using named connections, they must be to allowed URLs only

### Module Not Found

If the module cannot be found:

1. Ensure the Rnwood.Dataverse.Data.PowerShell module is built
2. Check the build output is in one of the expected locations
3. Set `DATAVERSE_MODULE_PATH` environment variable to the module directory

### Permission Issues

If you encounter "not allowed" errors:

1. Verify you're connecting to one of the allowed URLs
2. Check that the URL matches exactly (case-insensitive, but trailing slashes are normalized)
3. Ensure you're not trying to connect to a different environment

## License

MIT License - see the main repository for details.
