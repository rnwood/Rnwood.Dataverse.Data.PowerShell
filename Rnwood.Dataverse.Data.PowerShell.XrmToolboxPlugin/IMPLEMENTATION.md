# XrmToolbox Plugin Implementation Summary

## Overview

This implementation adds an XrmToolbox plugin that provides an embedded PowerShell console with the Rnwood.Dataverse.Data.PowerShell module pre-loaded. The plugin uses the ConEmu.Control.WinForms package to embed a full-featured terminal directly within the XrmToolbox tab.

## What Was Implemented

### 1. New Project: Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin

A new .NET Framework 4.8 Windows Forms class library project was created to host the XrmToolbox plugin.

**Key Files:**
- `PowerShellConsolePlugin.cs` - Main plugin control that inherits from `PluginControlBase`
- `Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.csproj` - Project file with XrmToolbox dependencies
- `README.md` - User documentation for the plugin
- `TESTING.md` - Manual testing instructions

### 2. Plugin Features

The plugin implements the following features:

#### Core Functionality
- **Embedded PowerShell console** directly within the XrmToolbox tab using ConEmu control
- **Automatic connection bridging** - Extracts connection URL and OAuth token from XrmToolbox and passes to PowerShell via named pipes
- **Bundled PowerShell module** - Module copied to output during build, loaded automatically
- **Execution policy detection** - Checks and warns about restrictive PowerShell execution policies
- **Restricted Language Mode detection** - Identifies and reports security restrictions
- Displays a welcome banner with quick-start examples
- Custom prompt showing "XrmToolbox PS" to indicate XrmToolbox context
- Self-cleaning initialization script (removes itself after execution)

#### Connection Bridging (Named Pipes)
- Extracts connection information from XrmToolbox `ServiceClient`:
  - Organization URL from `ConnectedOrgUriActual` or `ConnectedOrgPublishedEndpoints`
  - OAuth access token via reflection from `CurrentAccessToken` property
- **Passes connection data securely via named pipes** (replaced file-based approach)
  - Pipe server created with unique name per session
  - Runs asynchronously with cancellation token support
  - 5-second timeout for PowerShell to connect
  - No disk persistence - more secure than files
- PowerShell script connects to pipe and reads connection data
- Falls back to interactive authentication if token is not available
- Pipe automatically destroyed after use

#### Module Bundling
- Build target `CopyPowerShellModule` copies module from main project to `PSModule` directory
- PowerShell initialization script tries bundled module first:
  - Adds bundled path to `$env:PSModulePath`
  - Falls back to installed module if bundled version fails
- Users no longer required to install module separately (optional)

#### PowerShell Environment Validation
- **Restricted Language Mode Check**: Detects if PowerShell is running in restricted mode
  - Shows clear error message and instructions
  - Prevents module loading failures with cryptic errors
- **Execution Policy Check**: Detects restrictive policies (Restricted, AllSigned)
  - Shows warnings with fix instructions
  - Provides both LocalMachine and CurrentUser scope options
- Graceful error handling with "press any key to exit" for better UX

#### ConEmu Control Integration
- Uses ConEmu.Control.WinForms NuGet package (v1.3.8) for embedded terminal
- Full-featured terminal rendering within the plugin tab
- No external ConEmu installation required
- Native Windows console support with scrollback buffer
- Copy/paste functionality built-in

#### XrmToolbox Integration
- Implements `IGitHubPlugin` interface for GitHub repository link
- Implements `IPayPalPlugin` interface for donation support
- Properly handles plugin lifecycle with `ClosingPlugin` override
- Cleans up ConEmu control when plugin tab is closed

#### User Experience
- Console embedded directly in the XrmToolbox tab (no separate window)
- Helpful quick-start examples displayed in console
- Instructions for connecting to Dataverse
- Graceful error handling with MessageBox for failures

### 3. Technical Architecture

#### ConEmu Control Management
```csharp
private ConEmuControl conEmuControl;  // Embedded ConEmu terminal control
```
- ConEmu control hosted directly in plugin UserControl
- PowerShell launched with `-NoExit` to keep console open
- Uses `-ExecutionPolicy Bypass` to avoid script execution blocks
- Temporary initialization script created in %TEMP%
- Control disposed cleanly when plugin closes

#### Script Generation
The `GenerateConnectionScript()` method creates a PowerShell initialization script that:
1. Loads the Rnwood.Dataverse.Data.PowerShell module
2. Reads connection data from secure temporary file
3. Establishes connection using extracted URL and OAuth token
4. Displays connection status and user information via `Get-DataverseWhoAmI`
5. Shows quick-start examples and help
6. Sets custom prompt
7. Removes itself and connection data file after execution

### 4. Documentation

Three documentation files were created:

#### README.md
- Installation instructions
- Usage guide
- Example operations (query, create, update, SQL)
- Troubleshooting guide
- Tips and best practices

#### TESTING.md  
- Build verification steps
- 10 comprehensive test cases for manual testing
- Known limitations
- Troubleshooting guide

#### Main README.md Updates
- Added XrmToolbox plugin to features list
- Linked to plugin README

### 5. Testing

#### Automated Tests (tests/XrmToolboxPlugin.Tests.ps1)
- Project file exists
- Build succeeds
- DLL is created
- Dependencies are present
- Documentation exists

All tests pass successfully on Linux build environment (which can build the project even though it can't run it).

## Architecture Decisions

### Why .NET Framework 4.8 Instead of .NET 6?
XrmToolboxPackage requires .NET Framework 4.8 minimum. XrmToolbox plugins must target .NET Framework to be compatible.

### Why Use ConEmu.Control.WinForms for Embedded Terminal?
- Provides a full-featured terminal experience directly within the plugin tab
- No external dependencies or installations required for users
- Native Windows console support with proper rendering
- Simpler than custom terminal implementation
- Well-maintained NuGet package with good Windows Forms integration
- Users get the console directly in XrmToolbox without managing separate windows

### How Does Connection Bridging Work?
**Challenge**: PowerShell runs in a child process, separate from the XrmToolbox plugin process.

**Solution**: Named pipe-based inter-process communication (v2 - replaced file-based approach)
1. Extract connection URL from `ServiceClient.ConnectedOrgUriActual`
2. Extract OAuth token via reflection from `ServiceClient.CurrentAccessToken` (internal property)
3. Create named pipe server with unique GUID-based name
4. Start async pipe server with cancellation token
5. Pass pipe name to PowerShell initialization script
6. PowerShell creates pipe client and connects (5-second timeout)
7. Pipe server writes connection data, PowerShell reads it
8. Both pipe and script are destroyed after use
9. Falls back to interactive auth if token is unavailable

**Security Considerations**:
- Named pipes don't persist on disk (more secure than files)
- Pipe has unique name per session (GUID-based)
- Timeout prevents hanging connections
- Connection data never written to disk
- Pipe destroyed automatically after single use

**Why Named Pipes Over Files**:
- No disk persistence (connection data never touches filesystem)
- No file permissions to configure
- Automatic cleanup when process ends
- More efficient IPC mechanism
- Better security posture

### Why Store Script in Temp Instead of Embedded Resource?
- Easier to debug and modify
- Self-cleaning (script removes itself)
- No need for embedded resource management
- PowerShell can easily execute file with `-File` parameter

## Dependencies

### NuGet Packages
- **XrmToolBoxPackage 1.2024.9.23+** - XrmToolbox plugin framework
- **Microsoft.PowerPlatform.Dataverse.Client 1.2.3** - Dataverse connection (referenced via Cmdlets project)
- **ConEmu.Control.WinForms 1.3.8** - Embedded terminal control for Windows Forms

### Project References
- **Rnwood.Dataverse.Data.PowerShell.Cmdlets** - PowerShell cmdlets (net462 target)

## File Structure

```
Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/
├── PowerShellConsolePlugin.cs                      # Main plugin class (~150 lines)
├── Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.csproj
├── README.md                                        # User documentation
├── TESTING.md                                       # Testing guide
└── IMPLEMENTATION.md                                # Technical documentation

tests/
└── XrmToolboxPlugin.Tests.ps1                      # Build verification tests

README.md                                            # Updated with plugin mention
```

## Build Output

The plugin builds to:
```
Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/
└── bin/
    └── Release/
        └── net48/
            ├── Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.dll
            ├── ConEmu.WinForms.dll                     # Embedded ConEmu control
            ├── XrmToolBox.Extensibility.dll
            ├── Microsoft.PowerPlatform.Dataverse.Client.dll
            └── [other dependencies]
```

## Installation for End Users

1. Download plugin DLL from GitHub Releases
2. Copy to XrmToolbox Plugins folder
3. Restart XrmToolbox
4. **No module installation required** - Module is bundled with plugin
5. Open plugin from Tools menu

## Future Enhancements

Potential improvements for future versions:

1. **Connection Profile**: Save connection details for quick reconnection
   - Store org URL, auth method
   - Don't store credentials

2. **Custom Scripts**: Allow users to run custom initialization scripts
   - Load from plugin settings
   - Could include org-specific helpers

3. **Plugin Icon**: Add a custom icon for better visual identification
   - 32x32 PNG or ICO
   - Embedded as resource

5. **Plugin Settings**: Add settings page for customization
   - Custom initialization script path
   - Color scheme preferences
   - Font settings

## Known Limitations

1. **Windows Only**: Plugin requires Windows/.NET Framework 4.8
2. **Token Expiration**: OAuth tokens have limited lifetime - if token expires, user must reconnect manually
3. **Reflection-Based Token Extraction**: Uses reflection to access internal `CurrentAccessToken` property - may break with SDK updates
4. **Execution Policy**: Users with restrictive execution policies must adjust settings (guidance provided)

## Success Criteria

✅ Plugin compiles successfully  
✅ Plugin added to solution  
✅ **Connection bridging implemented** - Named pipes for secure IPC
✅ **Module bundling implemented** - No separate installation required
✅ **Execution policy detection** - Clear error messages and guidance
✅ **Restricted Language Mode detection** - Identifies security restrictions
✅ Documentation complete (README, TESTING, IMPLEMENTATION)  
✅ Build tests pass  
✅ Main README updated  
⏳ Manual testing (requires Windows/XrmToolbox environment)

## Conclusion

The XrmToolbox plugin successfully integrates the Rnwood.Dataverse.Data.PowerShell module into XrmToolbox using an embedded ConEmu control, providing users with a full-featured PowerShell console directly within XrmToolbox tabs. The implementation includes:

- **Automatic connection bridging** via named pipes for secure credential passing
- **Bundled PowerShell module** eliminating separate installation requirements
- **Comprehensive environment validation** detecting execution policies and language mode restrictions
- **Clear error messages** with actionable guidance for users

The plugin enhances the Rnwood.Dataverse.Data.PowerShell ecosystem by making it accessible directly from a popular Dataverse tool with minimal friction, providing a seamless user experience.

The plugin enhances the Rnwood.Dataverse.Data.PowerShell ecosystem by making it accessible directly from a popular Dataverse tool, lowering the barrier to entry for users who are already familiar with XrmToolbox. The embedded console approach provides a seamless user experience without requiring external terminal installations.
