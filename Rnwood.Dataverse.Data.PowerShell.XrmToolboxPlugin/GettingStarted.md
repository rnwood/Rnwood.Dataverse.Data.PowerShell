# Getting Started with PowerShell Scripting Plugin

Welcome to the PowerShell Console plugin for XrmToolbox! This plugin provides a powerful PowerShell console directly within XrmToolbox, pre-loaded with the `Rnwood.Dataverse.Data.PowerShell` module and automatically connected to your current Dataverse environment.

## Connection Management

The plugin automatically connects using your XrmToolbox credentials. The connection is set as default automatically.

For multi-environment scenarios, see the [Common Use-Cases](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell#common-use-cases) section in the main README.

## Keyboard Shortcuts

- **Ctrl+C**: Cancel the current command
- **Ctrl+R**: Reverse-search command history (type to filter previous commands)
- **Up/Down arrows**: Navigate command history
- **Tab**: Auto-complete inline (cmdlet names, parameters, table names, column names, etc.) - press multiple times to cycle through options
- - **Ctrl+Space**: Trigger completion with a menu

### Tab completion help you find

- **Cmdlet and function names**
  - Example: typing `Get-Dat` then pressing `Tab` will cycle to `Get-DataverseRecord` (or other matching cmdlets).

- **Parameter names**
  - Example: `Get-DataverseRecord -` then `Tab` will cycle through parameters such as `-Connection`, `-TableName`, `-Filter`, etc.

- **Table / entity logical names**
  - Example: `Get-DataverseRecord -TableName cont` then `Tab` -> `Get-DataverseRecord -TableName contact`

- **Column / attribute names for a table**
  - Example: `Get-DataverseRecord -TableName contact -Columns firstname, lastn` then `Tab` -> completes `lastname`.

## Getting Detailed Help
```powershell
# Get help for a specific cmdlet
Get-Help Get-DataverseRecord -Full

# List all available cmdlets
Get-Command -Module Rnwood.Dataverse.Data.PowerShell

# Get examples
Get-Help Get-DataverseRecord -Examples
```

The [README](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/README.md) covers basic concepts and common use-cases.

The [cmdlet docs](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/Rnwood.Dataverse.Data.PowerShell/docs) cover all available cmdlets and their parameters.

# Report Issues or Contribute
[GitHub Repo](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell)
