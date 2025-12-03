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
- **Script Gallery** - Community-driven script sharing via GitHub Gists
  - Browse and search public scripts tagged with #rnwdataversepowershell
  - Open scripts directly in the editor
  - Save scripts to GitHub Gists (new or update existing)
  - Share your scripts with the community
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

## Using the Script Gallery

The Script Gallery tab allows you to browse and share PowerShell scripts with the community:

### Browsing Scripts

1. Click on the **Script Gallery** tab in the right panel
2. The gallery will automatically load public scripts from GitHub Gists tagged with #rnwdataversepowershell
3. Click on a script to view its details
4. Double-click or click **Open** to open the script in a new editor tab

### Saving Scripts to GitHub Gist

1. Write or edit your script in the editor
2. Click **Save to Gist** in the editor toolbar
3. Fill in the required information:
   - **Description**: A description of your script (must include #rnwdataversepowershell)
   - **File Name**: The filename for your script (must end with .ps1)
   - **Visibility**: Public (visible in gallery) or Private
   - **GitHub Token**: Your GitHub Personal Access Token (see below)
4. If opening a script from the gallery, you can choose to update the existing gist

### Creating a GitHub Personal Access Token

To save scripts to GitHub Gists, you need a Personal Access Token:

1. Go to [https://github.com/settings/tokens](https://github.com/settings/tokens)
2. Click **Generate new token** → **Generate new token (classic)**
3. Give your token a descriptive name (e.g., "XrmToolbox Script Gallery")
4. Select the **gist** scope
5. Click **Generate token**
6. Copy the token and paste it in the Save to Gist dialog

**Important**: Keep your token secure and never share it publicly.

### What's next

Once you have the plugin running in XrmToolbox, a help pane will be displayed.
