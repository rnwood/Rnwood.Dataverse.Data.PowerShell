# Rnwood.Dataverse.Data.PowerShell XrmToolbox Plugin

This XrmToolbox plugin provides a PowerShell console with the Rnwood.Dataverse.Data.PowerShell module pre-loaded, allowing you to interact with Dataverse using PowerShell cmdlets directly from XrmToolbox.

## Features

- **Embedded PowerShell console** directly within the XrmToolbox tab using ConEmu control
- **Automatic connection bridging** - Automatically connects to the same Dataverse environment as XrmToolbox
- Automatically loads the Rnwood.Dataverse.Data.PowerShell module
- Full-featured terminal experience with ConEmu integration
- Custom XrmToolbox prompt to indicate you're working within the XrmToolbox context
- Helpful quick-start examples displayed on startup
- No need for external ConEmu installation - control is embedded via NuGet package
- Secure token passing via temporary file with restricted permissions

## Installation

### Prerequisites

1. **XrmToolbox** - Download and install from [xrmtoolbox.com](https://www.xrmtoolbox.com/)
2. **Rnwood.Dataverse.Data.PowerShell PowerShell Module** - Install from PowerShell Gallery:
   ```powershell
   Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
   ```

### Installing the Plugin

1. Open XrmToolbox
2. Go to **Tool Library** (or press `Ctrl+Alt+L`)
3. Search for "PowerShell Console" or "Rnwood.Dataverse.Data.PowerShell"
4. Click **Install**
5. Restart XrmToolbox

**OR**

1. Download the latest release DLL from the [GitHub Releases page](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/releases)
2. Copy the DLL and all its dependencies to your XrmToolbox `Plugins` folder
3. Restart XrmToolbox

## Usage

### Opening the Plugin

1. Connect to your Dataverse environment in XrmToolbox
2. Click on **Tools** menu
3. Select **PowerShell Console** (or search for it)
4. A new tab will open with an embedded PowerShell console

### Connecting to Dataverse

The PowerShell console launches with the module already loaded and **automatically connects to the same Dataverse environment as XrmToolbox**:

- The plugin extracts the connection URL and OAuth token from XrmToolbox
- The connection is established automatically using the same credentials
- The connection is available in the `$connection` variable

If automatic connection fails, you can manually connect:

```powershell
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
```

Replace `https://yourorg.crm.dynamics.com` with your actual Dataverse URL.

### Example Operations

Once connected, you can use all the Dataverse PowerShell cmdlets:

#### Query Records
```powershell
# Get all accounts
Get-DataverseRecord -Connection $connection -TableName account

# Get specific columns
Get-DataverseRecord -Connection $connection -TableName account -Columns name, accountnumber

# Filter records
Get-DataverseRecord -Connection $connection -TableName account -Filter @{ name = "Contoso" }
```

#### Create Records
```powershell
$newAccount = @{
    name = "Contoso"
    accountnumber = "ACC-001"
    revenue = 1000000
}
Set-DataverseRecord -Connection $connection -TableName account -Record $newAccount
```

#### Update Records
```powershell
$update = @{
    accountid = "guid-here"
    name = "Contoso Ltd"
}
Set-DataverseRecord -Connection $connection -TableName account -Record $update
```

#### SQL Queries
```powershell
Invoke-DataverseSql -Connection $connection -Sql @"
SELECT TOP 10 name, accountnumber, revenue
FROM account
WHERE statecode = 0
ORDER BY revenue DESC
"@
```

#### Get Help
```powershell
# List all cmdlets
Get-Command -Module Rnwood.Dataverse.Data.PowerShell

# Get help for a specific cmdlet
Get-Help Get-DataverseRecord -Full
Get-Help Set-DataverseRecord -Examples
```

## Embedded Console Technology

The plugin uses the ConEmu.Control.WinForms package to provide an embedded terminal experience directly within the XrmToolbox tab. This provides:
- Full-featured terminal rendering
- Native Windows console support
- Copy/paste functionality
- Scrollback buffer
- No external dependencies or installations required

The ConEmu control is bundled with the plugin via NuGet, so there's no need to install ConEmu separately.

## Troubleshooting

### Module Not Found

If you see an error that the module cannot be found:

1. Ensure the module is installed:
   ```powershell
   Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
   ```

2. Verify the module is available:
   ```powershell
   Get-Module -ListAvailable Rnwood.Dataverse.Data.PowerShell
   ```

3. Check your execution policy:
   ```powershell
   Get-ExecutionPolicy
   # If it's Restricted, set it to RemoteSigned
   Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

### Console Doesn't Appear or Shows Errors

- Ensure PowerShell is available in your PATH
- Try running `powershell.exe` from a command prompt to verify PowerShell works
- The plugin uses the embedded ConEmu control which requires .NET Framework 4.8
- Check Windows Event Viewer for any application errors
- Verify all plugin dependencies were copied to the XrmToolbox Plugins folder

### Connection Errors

If you have trouble connecting:

1. Verify your Dataverse URL is correct
2. Ensure you have the necessary permissions
3. Try different authentication methods:
   ```powershell
   # Interactive (browser-based)
   $connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
   
   # Username/Password
   $connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" `
       -Username "user@contoso.com" -Password "password"
   
   # Client Secret
   $connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" `
       -ClientId "your-app-id" -ClientSecret "your-secret"
   ```

## Tips and Best Practices

1. **Use Tab Completion**: PowerShell supports tab completion for cmdlet names and parameters

2. **Pipeline Support**: Chain cmdlets together:
   ```powershell
   Get-DataverseRecord -Connection $connection -TableName account -Filter @{ name = "Contoso" } |
       ForEach-Object { 
           $_.revenue *= 1.1  # Increase revenue by 10%
           Set-DataverseRecord -Connection $connection -TableName account -Record $_
       }
   ```

3. **Export Data**: Easily export to CSV:
   ```powershell
   Get-DataverseRecord -Connection $connection -TableName account |
       Export-Csv -Path accounts.csv -NoTypeInformation
   ```

4. **Use Variables**: Store commonly used filters or queries:
   ```powershell
   $activeAccountsFilter = @{ statecode = 0 }
   Get-DataverseRecord -Connection $connection -TableName account -Filter $activeAccountsFilter
   ```

## Contributing

Found a bug or have a feature request? Please open an issue on the [GitHub repository](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/issues).

## License

This plugin is part of the Rnwood.Dataverse.Data.PowerShell project and is licensed under the same terms as the main project.

## Links

- [Main Project Repository](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell)
- [PowerShell Module Documentation](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/README.md)
- [XrmToolbox Website](https://www.xrmtoolbox.com/)
- [ConEmu.Control.WinForms NuGet Package](https://www.nuget.org/packages/ConEmu.Control.WinForms)
