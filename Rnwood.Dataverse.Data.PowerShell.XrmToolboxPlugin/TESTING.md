# Testing the XrmToolbox Plugin

Since the XrmToolbox plugin is a Windows Forms control that integrates with XrmToolbox, it requires manual testing in the XrmToolbox environment.

## Build Verification

To verify the plugin builds correctly:

```bash
dotnet build Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.csproj -c Release
```

## Manual Testing Steps

### Prerequisites
1. Install XrmToolbox from https://www.xrmtoolbox.com/
2. Install the Rnwood.Dataverse.Data.PowerShell PowerShell module:
   ```powershell
   Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
   ```
3. (Optional) Install ConEmu from https://conemu.github.io/

### Installation

1. Build the plugin in Release mode
2. Locate the built DLL in `Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/bin/Release/net48/`
3. Copy all DLLs from that folder to your XrmToolbox `Plugins` folder (typically `%APPDATA%\MscrmTools\XrmToolBox\Plugins\`)
4. Restart XrmToolbox

### Test Cases

#### TC1: Plugin Appears in Tool Library
**Steps:**
1. Open XrmToolbox
2. Click Tool Library (or press Ctrl+Alt+L)
3. Search for "PowerShell"

**Expected Result:**
- The plugin should appear in the list

#### TC2: Plugin Launches PowerShell Console
**Steps:**
1. Connect to a Dataverse environment in XrmToolbox
2. Open the PowerShell Console plugin from the Tools menu
3. Observe the behavior

**Expected Result:**
- A PowerShell window should open (either ConEmu if installed, or standard PowerShell console)
- The XrmToolbox tab should show a message indicating the console was launched
- The PowerShell console should display a welcome banner in cyan color
- The module should be loaded automatically

#### TC3: Module is Pre-loaded
**Steps:**
1. In the launched PowerShell console, run:
   ```powershell
   Get-Module Rnwood.Dataverse.Data.PowerShell
   ```

**Expected Result:**
- The module should be listed as loaded

#### TC4: Cmdlets are Available
**Steps:**
1. In the PowerShell console, run:
   ```powershell
   Get-Command -Module Rnwood.Dataverse.Data.PowerShell
   ```

**Expected Result:**
- All cmdlets should be listed (Get-DataverseConnection, Get-DataverseRecord, Set-DataverseRecord, etc.)

#### TC5: Connection Works
**Steps:**
1. In the PowerShell console, run:
   ```powershell
   $connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
   ```
2. Complete the authentication in the browser
3. Run:
   ```powershell
   Get-DataverseWhoAmI -Connection $connection
   ```

**Expected Result:**
- Authentication should complete successfully
- WhoAmI should return your user ID and organization ID

#### TC6: Query Records
**Steps:**
1. After connecting (TC5), run:
   ```powershell
   Get-DataverseRecord -Connection $connection -TableName systemuser -Top 5
   ```

**Expected Result:**
- Should return 5 system user records

#### TC7: Custom Prompt
**Steps:**
1. Observe the PowerShell prompt in the console

**Expected Result:**
- The prompt should be in the format: `XrmToolbox PS C:\path>`

#### TC8: ConEmu Detection (if installed)
**Steps:**
1. Install ConEmu
2. Close any open PowerShell console from the plugin
3. Open the plugin again

**Expected Result:**
- ConEmu window should open instead of standard PowerShell console

#### TC9: Process Cleanup on Tab Close
**Steps:**
1. Open the plugin (PowerShell console launches)
2. Close the XrmToolbox tab
3. Check Task Manager for PowerShell processes

**Expected Result:**
- The PowerShell process should be terminated when the tab is closed

#### TC10: Multiple Instances
**Steps:**
1. Open the plugin twice (two different tabs)

**Expected Result:**
- Two separate PowerShell consoles should open
- Each should be independent

## Known Limitations

1. **No Auto-Connection**: The plugin does not automatically connect using the XrmToolbox connection. Users must manually create a connection using `Get-DataverseConnection`.
   - This is a limitation because passing the ServiceClient connection from XrmToolbox to a new PowerShell process is complex
   - Future enhancement: Could serialize connection parameters and pass them to the PowerShell script

2. **Windows Only**: The plugin only works on Windows (requires .NET Framework 4.8 and Windows Forms)

3. **Requires Module Installation**: The PowerShell module must be installed separately

4. **Process Management**: Closing the PowerShell window will not close the XrmToolbox tab (intentional behavior)

## Troubleshooting

### Plugin Doesn't Appear in XrmToolbox
- Check that all dependencies are copied to the Plugins folder
- Check XrmToolbox logs in %APPDATA%\MscrmTools\XrmToolBox\Logs
- Ensure you're using .NET Framework 4.8 or later

### PowerShell Window Doesn't Open
- Check that powershell.exe is in your PATH
- Check Windows Event Viewer for application errors
- Try running `powershell.exe` manually to verify it works

### Module Not Found Error
- Install the module: `Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser`
- Check module is available: `Get-Module -ListAvailable Rnwood.Dataverse.Data.PowerShell`
- Check execution policy: `Get-ExecutionPolicy` (should be RemoteSigned or Unrestricted)

### ConEmu Not Detected
- Verify ConEmu is installed in one of the expected locations:
  - C:\Program Files\ConEmu\ConEmu64.exe
  - C:\Program Files (x86)\ConEmu\ConEmu.exe
  - %LOCALAPPDATA%\ConEmu\ConEmu64.exe
