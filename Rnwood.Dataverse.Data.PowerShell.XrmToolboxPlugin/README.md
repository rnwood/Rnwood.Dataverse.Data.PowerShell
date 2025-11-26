# Rnwood.Dataverse.Data.PowerShell XrmToolbox Plugin

This XrmToolbox plugin provides a PowerShell console with the Rnwood.Dataverse.Data.PowerShell module pre-loaded, allowing you to interact with Dataverse using PowerShell cmdlets directly from XrmToolbox.

## Features

- **Embedded PowerShell console** directly within the XrmToolbox tab using ConEmu control
- **Script editor** - Modern code editor with syntax highlighting and IntelliSense
- **Intelligent code completion** - Dynamic PowerShell IntelliSense using TabExpansion2 API
  - Context-aware cmdlet, parameter, and variable completion
  - Works with PowerShell 5.1+ (no PowerShell 7 requirement)
  - Automatic completion of module cmdlets and all installed modules
  - Parameter completion with inline help
- **Automatic connection bridging** - Automatically connects to the same Dataverse environment as XrmToolbox
- **Bundled PowerShell module** - Module is included with the plugin, no separate installation required

## Installation

### Prerequisites

1. **XrmToolbox** - Download and install from [xrmtoolbox.com](https://www.xrmtoolbox.com/)

### Installing the Plugin

1. Open XrmToolbox
2. Go to **Tool Library** (or press `Ctrl+Alt+L`)
3. Search for "PowerShell Scripting Workspace"
4. Click **Install**

### Connecting to Dataverse

The PowerShell console launches with the module already loaded and **automatically connects to the same Dataverse environment as XrmToolbox**:

### What's next

Once you have the plugin running in XrmToolbox, a help pane will be displayed.
