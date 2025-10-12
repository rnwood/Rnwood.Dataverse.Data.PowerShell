# Rnwood.Dataverse.Data.PowerShell XrmToolbox Plugin

This XrmToolbox plugin provides a PowerShell console with the Rnwood.Dataverse.Data.PowerShell module pre-loaded, allowing you to interact with Dataverse using PowerShell cmdlets directly from XrmToolbox.

## Features

- **Embedded PowerShell console** directly within the XrmToolbox tab using ConEmu control
- **Script editor with Monaco** - Modern code editor with syntax highlighting and IntelliSense
- **Intelligent code completion (LSP-based)** - Dynamic PowerShell IntelliSense using TabExpansion2 API
  - Context-aware cmdlet, parameter, and variable completion
  - Works with PowerShell 5.1+ (no PowerShell 7 requirement)
  - Automatic completion of module cmdlets and all installed modules
  - Parameter completion with inline help
- **Automatic connection bridging** - Automatically connects to the same Dataverse environment as XrmToolbox using named pipes
- **Bundled PowerShell module** - Module is included with the plugin, no separate installation required
- **Execution policy detection** - Detects and warns about restrictive PowerShell policies with clear instructions
- **Restricted Language Mode detection** - Identifies security restrictions and provides guidance
- Full-featured terminal experience with ConEmu integration
- Custom XrmToolbox prompt to indicate you're working within the XrmToolbox context
- Helpful quick-start examples displayed on startup
- No need for external ConEmu installation - control is embedded via NuGet package
- Secure connection passing via named pipes (no disk persistence)

## Installation

### Prerequisites

1. **XrmToolbox** - Download and install from [xrmtoolbox.com](https://www.xrmtoolbox.com/)
2. **Rnwood.Dataverse.Data.PowerShell PowerShell Module** - *(Optional)* The module is bundled with the plugin, but you can also install it from PowerShell Gallery for system-wide availability:
   ```powershell
   Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
   ```

### Installing the Plugin

#### Option 1: From XrmToolbox Tool Library (When Available)

1. Open XrmToolbox
2. Go to **Tool Library** (or press `Ctrl+Alt+L`)
3. Search for "PowerShell Console" or "Rnwood.Dataverse.Data.PowerShell"
4. Click **Install**
5. Restart XrmToolbox

#### Option 2: Manual Installation from GitHub Releases

1. Download the latest release from the [GitHub Releases page](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/releases)
2. Extract the ZIP file
3. Locate your XrmToolbox `Plugins` folder:
   - Default location: `%APPDATA%\MscrmTools\XrmToolBox\Plugins`
   - Or: `C:\Users\<YourUsername>\AppData\Roaming\MscrmTools\XrmToolBox\Plugins`
4. Copy the entire plugin folder (containing `Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.dll` and all dependencies) to the `Plugins` folder
5. Restart XrmToolbox
6. The plugin should now appear in the Tools menu

#### Option 3: Manual Installation from Local Build

If you're building from source:

1. Build the solution in Release mode:
   ```powershell
   dotnet build Rnwood.Dataverse.Data.PowerShell.sln -c Release
   ```

2. Navigate to the output directory:
   ```
   Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin\bin\Release\net48\
   ```

3. Copy the entire contents of the `net48` folder to your XrmToolbox `Plugins` folder:
   - Default location: `%APPDATA%\MscrmTools\XrmToolBox\Plugins\Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin`

4. Important files to copy include:
   - `Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.dll` (main plugin)
   - `ConEmu.*.dll` (ConEmu control libraries)
   - `PSModule` folder (bundled PowerShell module)
   - All other dependency DLLs

5. Restart XrmToolbox

#### Verifying Installation

After installation, verify the plugin is loaded:
1. Open XrmToolbox
2. Click **Tools** menu
3. Look for **PowerShell Console** in the list
4. If you don't see it, check:
   - XrmToolbox logs (Help > Logs)
   - Ensure all DLL files were copied
   - Verify .NET Framework 4.8 is installed

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
- **The connection is set as the default** - you can omit the `-Connection` parameter in cmdlets
- The connection is also available in the `$connection` variable for explicit use

**What does "default connection" mean?**

When a connection is set as default using `-SetAsDefault`, you don't need to pass `-Connection $connection` to every cmdlet:

```powershell
# Without default connection (verbose)
Get-DataverseRecord -Connection $connection -TableName account

# With default connection (simplified)
Get-DataverseRecord -TableName account
```

The plugin automatically sets the XrmToolbox connection as default, so you can use the simplified syntax immediately.

#### Manual Connection

If automatic connection fails, you can manually connect:

```powershell
# Connect and set as default
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive -SetAsDefault

# Or connect without setting as default
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
```

Replace `https://yourorg.crm.dynamics.com` with your actual Dataverse URL.

#### Working with Multiple Connections

If you need to work with multiple Dataverse environments simultaneously:

```powershell
# Primary connection (set as default for convenience)
$conn1 = Get-DataverseConnection -Url "https://org1.crm.dynamics.com" -Interactive -SetAsDefault

# Secondary connection (explicit, not default)
$conn2 = Get-DataverseConnection -Url "https://org2.crm.dynamics.com" -Interactive

# Use default connection (conn1) - no -Connection parameter needed
Get-DataverseRecord -TableName account | Select-Object -First 5

# Use secondary connection explicitly
Get-DataverseRecord -Connection $conn2 -TableName contact | Select-Object -First 5

# Copy data between environments
$accounts = Get-DataverseRecord -Connection $conn1 -TableName account -Filter @{ name = "Contoso" }
foreach ($account in $accounts) {
    Set-DataverseRecord -Connection $conn2 -TableName account -Record $account
}
```

For more advanced scenarios and examples, see the [Common Use-Cases section](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell#common-use-cases) in the main README.

### Example Operations

Once connected, you can use all the Dataverse PowerShell cmdlets. **Since the connection is set as default**, you can omit the `-Connection` parameter:

#### Query Records
```powershell
# Get all accounts (using default connection)
Get-DataverseRecord -TableName account

# Get specific columns
Get-DataverseRecord -TableName account -Columns name, accountnumber

# Filter records
Get-DataverseRecord -TableName account -Filter @{ name = "Contoso" }

# You can still use -Connection explicitly if needed
Get-DataverseRecord -Connection $connection -TableName account
```

#### Create Records
```powershell
$newAccount = @{
    name = "Contoso"
    accountnumber = "ACC-001"
    revenue = 1000000
}
# Uses default connection
Set-DataverseRecord -TableName account -Record $newAccount
```

#### Update Records
```powershell
$update = @{
    accountid = "guid-here"
    name = "Contoso Ltd"
}
# Uses default connection
Set-DataverseRecord -TableName account -Record $update
```

#### SQL Queries
```powershell
# Uses default connection
Invoke-DataverseSql -Sql @"
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

### Using the Script Editor

The plugin includes a Monaco-based script editor with intelligent code completion powered by PowerShell's TabExpansion2 API.

#### Switching to Script Editor

Click the **📝 Script Editor** button in the toolbar to switch from the console view to the script editor.

#### Code Completion Features

The script editor provides dynamic, context-aware IntelliSense:

1. **Command Completion**: Type `Get-D` and press `Ctrl+Space` to see all available cmdlets starting with "Get-D"
   - Includes cmdlets from the Dataverse module
   - Includes all built-in PowerShell cmdlets
   - Shows cmdlets from all loaded modules

2. **Parameter Completion**: Type `Get-DataverseRecord -` and press `Ctrl+Space` to see all available parameters
   - Shows parameter names with inline documentation
   - Automatically triggered after typing `-`

3. **Variable Completion**: Type `$` to see available variables
   - Shows variables defined in your script
   - Shows PowerShell automatic variables

4. **Property/Method Completion**: Type a variable followed by `.` to see properties and methods
   - Example: `$connection.` shows properties like `ConnectedOrgUniqueName`, `ConnectedOrgVersion`, etc.

5. **Trigger Characters**: Completion is automatically triggered when typing:
   - `-` (for parameters)
   - `$` (for variables)
   - `.` (for properties/methods)
   - `::` (for static members)

#### Running Scripts

- Press **F5** or click the **▶ Run** button to execute your script
- Press **Ctrl+S** to save your script
- Press **Ctrl+N** for a new script

#### Technical Details

The code completion uses PowerShell's native `TabExpansion2` API running in a background PowerShell process:
- Compatible with PowerShell 5.1+ (no PowerShell 7 required)
- Works with all installed modules and their cmdlets
- Provides real-time completion based on current script context
- Minimal performance impact with intelligent caching

## Embedded Console Technology

The plugin uses the ConEmu.Control.WinForms package to provide an embedded terminal experience directly within the XrmToolbox tab. This provides:
- Full-featured terminal rendering
- Native Windows console support
- Copy/paste functionality
- Scrollback buffer
- No external dependencies or installations required

The ConEmu control is bundled with the plugin via NuGet, so there's no need to install ConEmu separately.

The script editor uses Monaco Editor (the same editor used in VS Code) with WebView2 integration for modern editing features.

## Troubleshooting

### Execution Policy Errors

If you see warnings about execution policy:

```
WARNING: PowerShell Execution Policy may prevent scripts from running
Current policy: Restricted
```

**Fix**: Run PowerShell as Administrator and execute:
```powershell
Set-ExecutionPolicy RemoteSigned -Scope LocalMachine
```

Or for current user only:
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Restricted Language Mode Error

If you see an error about Restricted Language Mode:

```
ERROR: PowerShell is running in Restricted Language Mode
```

This security setting prevents the module from loading. To fix:
1. Check your organization's PowerShell security policies
2. Contact your IT administrator
3. May require disabling Application Control policies

### Module Not Found

The module is bundled with the plugin, so installation is optional. If you still see module errors:

1. Verify the bundled module is present in the plugin's `PSModule` folder
2. Optionally install the module system-wide:
   ```powershell
   Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
   ```

3. Check your execution policy (see above)

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

## Development and Debugging

### Prerequisites for Development

- Visual Studio 2022 or VS Code
- .NET SDK 6.0 or later
- .NET Framework 4.8 Developer Pack
- PowerShell 7+ (for scripts)

### Setting Up Development Environment

1. Clone the repository:
   ```bash
   git clone https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell.git
   cd Rnwood.Dataverse.Data.PowerShell
   ```

2. Download XrmToolbox for debugging:
   ```powershell
   .\scripts\Download-XrmToolbox.ps1
   ```
   
   This downloads XrmToolbox to `.xrmtoolbox` in the repository root.

3. Build the solution:
   ```powershell
   dotnet build Rnwood.Dataverse.Data.PowerShell.sln -c Debug
   ```

### Debugging in Visual Studio

The plugin project includes launch profiles for easy debugging:

1. Open `Rnwood.Dataverse.Data.PowerShell.sln` in Visual Studio
2. Set `Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin` as the startup project
3. Select one of the launch profiles:
   - **XrmToolbox (local)** - Uses `.xrmtoolbox\XrmToolBox.exe` from repo root
   - **XrmToolbox (custom path)** - Uses XrmToolbox from a custom installation path (edit path in Properties/launchSettings.json)
4. Press F5 to start debugging

The plugin will be automatically copied to XrmToolbox's Plugins folder before starting.

**Manual Steps (if needed)**:
1. Build the plugin in Debug configuration
2. Copy `Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin\bin\Debug\net48\*` to `%APPDATA%\MscrmTools\XrmToolBox\Plugins\Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin\`
3. Start XrmToolbox
4. In Visual Studio, go to Debug > Attach to Process
5. Select `XrmToolBox.exe`
6. Set breakpoints in the plugin code

### Debugging in VS Code

The repository includes VS Code tasks and launch configurations:

#### Using Tasks

1. Open the repository in VS Code
2. Run tasks via Terminal > Run Task:
   - **Download XrmToolbox** - Downloads XrmToolbox to `.xrmtoolbox`
   - **Build XrmToolbox Plugin** - Builds the plugin project

#### Using Launch Configuration

1. Open the repository in VS Code
2. Go to Run and Debug (Ctrl+Shift+D)
3. Select **Debug XrmToolbox Plugin** from the dropdown
4. Press F5 to start debugging

This will:
- Build the plugin
- Copy it to the local XrmToolbox installation
- Start XrmToolbox with debugger attached

**Note**: The CLR debugger in VS Code requires the C# extension and may have limitations compared to Visual Studio.

### Helper Scripts

The repository includes PowerShell scripts in the `scripts` folder:

#### Download-XrmToolbox.ps1

Downloads XrmToolbox from GitHub releases:

```powershell
# Download to default location (.xrmtoolbox)
.\scripts\Download-XrmToolbox.ps1

# Download to custom location
.\scripts\Download-XrmToolbox.ps1 -OutputPath "C:\Dev\XrmToolbox"

# Download specific version
.\scripts\Download-XrmToolbox.ps1 -Version "1.2024.9.23"
```

### Testing Changes

After making changes:

1. Build the solution:
   ```powershell
   dotnet build Rnwood.Dataverse.Data.PowerShell.sln -c Debug
   ```

2. Run automated tests:
   ```powershell
   $env:TESTMODULEPATH = "$(pwd)/Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0"
   Invoke-Pester -Path tests -Output Detailed
   ```

3. Test the plugin:
   - Manually start XrmToolbox from `.xrmtoolbox\XrmToolBox.exe`
   - Connect to a Dataverse environment
   - Launch PowerShell Console from Tools menu
   - Test your changes

### Project Structure

```
Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/
├── PowerShellConsolePlugin.cs          # Main plugin class
├── GettingStarted.md                   # Embedded help content
├── README.md                            # Plugin documentation
├── IMPLEMENTATION.md                    # Technical details
├── TESTING.md                           # Test cases
└── Properties/
    └── launchSettings.json              # Visual Studio launch profiles

scripts/
├── Download-XrmToolbox.ps1              # Downloads XrmToolbox

.vscode/
├── launch.json                          # VS Code debugging config
└── tasks.json                           # VS Code tasks
```

### Common Development Issues

**Issue**: XrmToolbox doesn't show the plugin

**Solution**:
- Verify all DLLs were copied to the Plugins folder
- Check XrmToolbox logs (Help > Logs) for errors
- Ensure .NET Framework 4.8 is installed
- Restart XrmToolbox after copying files

**Issue**: Breakpoints not hit in Visual Studio

**Solution**:
- Ensure you're building in Debug configuration
- Verify the debugger is attached to XrmToolBox.exe
- Check that PDB files are present in the Plugins folder
- Disable "Just My Code" in Debug settings

**Issue**: ConEmu control doesn't load

**Solution**:
- Verify ConEmu*.dll files are in the Plugins folder
- Check that ConEmuControl.dll.config is present
- Ensure Visual C++ Redistributable is installed



## License

This plugin is part of the Rnwood.Dataverse.Data.PowerShell project and is licensed under the same terms as the main project.

## Links

- [Main Project Repository](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell)
- [PowerShell Module Documentation](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/README.md)
- [XrmToolbox Website](https://www.xrmtoolbox.com/)
- [ConEmu.Control.WinForms NuGet Package](https://www.nuget.org/packages/ConEmu.Control.WinForms)
