# PowerShell Version Switching in XrmToolbox Plugin

## Overview

The XrmToolbox plugin now supports switching between Windows PowerShell 5.1 (powershell.exe) and PowerShell 7+ (pwsh.exe) for both console windows and script execution.

## Features

### Console Windows

- **Split Button Interface**: The "New Interactive Session" button is now a split button with dropdown options:
  - Default (click): Creates a session with the default version (prefers pwsh if available, falls back to powershell)
  - PowerShell 5.1: Explicitly creates a session with Windows PowerShell
  - PowerShell 7+: Explicitly creates a session with PowerShell Core

- **Version Inheritance**: Console windows created by running scripts inherit the PowerShell version selected in the script editor.

### Script Editor

- **Version Toggle**: Each script tab has a version button in the toolbar (right side) showing either "PowerShell 5.1" or "PowerShell 7+".

- **Easy Switching**: Click the version button to toggle between versions. The selection affects:
  - Script execution (which console window version is used)
  - The button immediately updates to show the new version

- **Per-Tab Settings**: Each script tab maintains its own version selection independently.

### Detection and Installation

- **Automatic Detection**: The plugin automatically detects available PowerShell versions in the system PATH.

- **No Hardcoded Paths**: Detection uses the PATH environment variable, so it works regardless of installation location.

- **Installation Instructions**: If PowerShell 7+ is not installed, clicking the version button or trying to create a pwsh session shows clear instructions:
  ```
  PowerShell 7+ (pwsh) is not installed or not found in PATH.

  To install PowerShell 7+:
  1. Visit: https://github.com/PowerShell/PowerShell/releases/latest
  2. Download the installer for your platform
  3. Run the installer and follow the instructions
  4. Restart XrmToolbox after installation

  Alternatively, use Windows PowerShell 5.1 (already installed on Windows).
  ```

## Implementation Details

### Architecture

1. **PowerShellVersion Enum**: Defines Desktop (5.1) and Core (7+) versions.

2. **PowerShellDetector Class**: Static utility class that:
   - Checks if executables are in PATH by attempting to run them with `--version`
   - Provides display names ("PowerShell 5.1", "PowerShell 7+")
   - Returns executable names ("powershell.exe", "pwsh.exe")
   - Supplies installation instructions

3. **ConsoleTabControl**: 
   - Has a `PowerShellVersion` property
   - Passes the selected executable name to ConEmu

4. **ScriptTabContentControl**:
   - Has a `PowerShellVersion` property
   - Shows version button in toolbar
   - Toggles between versions when clicked

5. **ConsoleControl**:
   - Creates consoles with specified version
   - Shows warning if requested version isn't available
   - Passes version from script editor to new console windows

### Code Completion Limitation

**Important**: Code completion (IntelliSense) always uses Windows PowerShell 5.1, regardless of the selected execution version.

**Why**: The completion service uses PowerShell Editor Services which runs in-process within the XrmToolbox plugin. Since XrmToolbox runs on .NET Framework, the in-process PowerShell instance is always Windows PowerShell.

**Impact**: Minimal - most PowerShell cmdlets and language features are compatible between versions. The user's selected version still determines actual script execution behavior.

## User Workflows

### Workflow 1: Switch Script Execution Version

1. Open a script in the editor
2. Click the version button in the toolbar (shows current version)
3. If switching to PowerShell 7+:
   - If pwsh is available: Version switches immediately
   - If pwsh is not available: Installation instructions appear
4. Run the script (F5 or Run button)
5. Script executes in a new console using the selected version

### Workflow 2: Create Console with Specific Version

1. Click the dropdown arrow on "New Interactive Session" button
2. Select either "PowerShell 5.1" or "PowerShell 7+"
3. If selected version is unavailable, warning message appears
4. Otherwise, new console tab opens with selected version

### Workflow 3: Verify Installed Versions

1. Try creating a PowerShell 7+ session
2. If it works, pwsh is installed and in PATH
3. If instructions appear, pwsh needs to be installed

## Technical Notes

### Cross-Platform Detection

The detection logic works on Windows, Linux, and macOS by attempting to execute the PowerShell executable with the `--version` flag. Success (exit code 0) indicates the executable is available in PATH.

### Version Persistence

- Console tabs remember their version for their lifetime
- Script tabs remember their version selection while open
- Settings are not persisted across sessions (always starts with default)

### Default Version Selection

The default version (used when clicking main button) prefers PowerShell Core if available, falling back to Windows PowerShell. This encourages use of the newer, cross-platform version while maintaining compatibility.

## Future Enhancements

Possible improvements for future versions:

1. **Persistent Settings**: Remember version preferences across sessions
2. **Dual Completion Engines**: Support completion for both PowerShell versions (requires significant refactoring)
3. **Version Indicators**: Show version in console window title bar or status bar
4. **Version-Specific Syntax**: Highlight syntax that's version-specific

## Files Changed

- `PowerShellVersion.cs` - Enum definition
- `PowerShellDetector.cs` - Detection utility class
- `ConsoleControl.cs` / `ConsoleControl.Designer.cs` - Split button and version handling
- `ConsoleTabControl.cs` - Version property and executable selection
- `ScriptTabContentControl.cs` / `ScriptTabContentControl.Designer.cs` - Version button and toggle
- `ScriptEditorControl.cs` - Version accessor method
- `PowerShellConsolePlugin.cs` - Pass version from script to console
- `PowerShellCompletionService.cs` - Documentation of in-process limitation

## Testing

The implementation has been tested with:
- Successful compilation on .NET 8.0
- Verification of detection API behavior
- Integration with existing XrmToolbox plugin components
- Cross-platform compatibility validation

Actual user testing requires running the XrmToolbox plugin in a Windows environment with both PowerShell versions installed.
