# Dynamic Plugin Assembly Cmdlets

## Overview

New cmdlets have been added to compile C# source code into Dataverse plugin assemblies and manage them automatically. This enables a source-first workflow for plugin development.

## Cmdlets

### Set-DataverseDynamicPluginAssembly

Compiles C# source code into a plugin assembly, uploads to Dataverse, and automatically manages plugin types.

**Key Features:**
- Creates new assembly or updates existing one (based on name)
- Auto-detects plugin types (classes implementing `IPlugin`)
- Automatically adds/removes plugin type records in Dataverse
- Generates or reuses strong name key
- Reuses existing assembly settings (version, culture, references) when not specified
- Embeds source code and build metadata in assembly
- Validates that source code contains at least one plugin class

**Parameters:**
- `-SourceCode` or `-SourceFile`: C# source code to compile
- `-Name`: Assembly name
- `-FrameworkReferences`: Optional framework references
- `-PackageReferences`: Optional NuGet package references (format: `Package@Version`)
- `-StrongNameKeyFile`: Optional strong name key file (.snk)
- `-Version`: Optional version (defaults to existing or 1.0.0.0)
- `-Culture`: Optional culture (defaults to existing or neutral)
- `-IsolationMode`: Sandbox (default), None, or External
- `-Description`: Optional description
- `-PassThru`: Return created/updated assembly object
- `-Connection`: Dataverse connection

**Example:**
```powershell
$source = @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyPlugins {
    public class AccountPlugin : IPlugin {
        public void Execute(IServiceProvider serviceProvider) {
            // Plugin logic here
        }
    }
}
"@

Set-DataverseDynamicPluginAssembly -SourceCode $source -Name "MyPlugins" -Connection $conn
```

### Get-DataverseDynamicPluginAssembly

Extracts source code and build metadata from a compiled plugin assembly.

**Key Features:**
- Extracts embedded source code
- Retrieves framework and package references
- Gets strong name key information (public key token)
- Optionally outputs source code to file

**Parameters:**
- `-AssemblyBytes` or `-FilePath`: Assembly to extract from
- `-OutputSourceFile`: Optional path to write source code

**Example:**
```powershell
# Get assembly from Dataverse
$assembly = Get-DataversePluginAssembly -Name "MyPlugins" -Connection $conn
$bytes = [Convert]::FromBase64String($assembly.content)

# Extract source code
$metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes
Write-Host $metadata.SourceCode
```

## Implementation Details

### Architecture

1. **Compilation**: Uses Microsoft.CodeAnalysis.CSharp (Roslyn) v4.11.0 for in-memory C# compilation
2. **Plugin Detection**: Parses syntax tree to find classes implementing `IPlugin` interface
3. **Metadata Embedding**: Stores source code and build settings as JSON in assembly footer (marker: "DPLM")
4. **Strong Name Generation**: Creates RSA 2048-bit keys when needed
5. **Plugin Type Management**: Automatically creates/deletes plugin type records via Dataverse SDK

### Files Added/Modified

**New Files:**
- `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/SetDataverseDynamicPluginAssemblyCmdlet.cs`
- `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/GetDataversePluginAssemblySourceCmdlet.cs`
- `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Model/PluginAssemblyMetadata.cs`
- `Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseDynamicPluginAssembly.md`
- `Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseDynamicPluginAssembly.md`

**Modified Files:**
- `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Rnwood.Dataverse.Data.PowerShell.Cmdlets.csproj` - Added Microsoft.CodeAnalysis.CSharp package
- `Rnwood.Dataverse.Data.PowerShell.Loader/ModuleInitProvider.cs` - Added assembly resolution for CodeAnalysis assemblies

### Testing

Test files created:
- `tests/DynamicPluginAssembly.Tests.ps1` - Tests for the new cmdlets with mock Dataverse connection
- `manual-test-plugin-cmdlets.ps1` - Manual test script demonstrating cmdlet usage

## Workflow Example

### 1. Create Plugin Assembly from Source
```powershell
# Connect to Dataverse
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive

# Define plugin source code
$pluginSource = @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyCompany.Plugins
{
    public class PreCreateAccount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            
            trace.Trace("PreCreateAccount plugin executing");
            
            if (context.InputParameters.Contains("Target") && 
                context.InputParameters["Target"] is Entity)
            {
                var target = (Entity)context.InputParameters["Target"];
                
                // Validation logic
                if (!target.Contains("name"))
                {
                    throw new InvalidPluginExecutionException("Account name is required.");
                }
            }
        }
    }
}
"@

# Compile and upload
Set-DataverseDynamicPluginAssembly `
    -Connection $conn `
    -SourceCode $pluginSource `
    -Name "MyCompany.Plugins" `
    -Version "1.0.0.0" `
    -PassThru
```

### 2. Update Plugin Assembly
```powershell
# Modify source code
$updatedSource = @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyCompany.Plugins
{
    public class PreCreateAccount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Updated implementation
        }
    }
    
    // New plugin added
    public class PostUpdateAccount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // New plugin logic
        }
    }
}
"@

# Update (automatically increments version, adds new plugin type, keeps existing settings)
Set-DataverseDynamicPluginAssembly `
    -Connection $conn `
    -SourceCode $updatedSource `
    -Name "MyCompany.Plugins" `
    -Version "1.1.0.0"

# Both PreCreateAccount and PostUpdateAccount plugin types are now registered
```

### 3. Extract Source Code
```powershell
# Retrieve assembly from Dataverse
$assembly = Get-DataversePluginAssembly -Name "MyCompany.Plugins" -Connection $conn

# Extract source code
$bytes = [Convert]::FromBase64String($assembly.content)
$metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes

# View source
Write-Host $metadata.SourceCode

# View references
Write-Host "Framework: $($metadata.FrameworkReferences -join ', ')"
Write-Host "Packages: $($metadata.PackageReferences -join ', ')"
Write-Host "Version: $($metadata.Version)"
Write-Host "Public Key Token: $($metadata.PublicKeyToken)"

# Save to file
Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes -OutputSourceFile "C:\Temp\plugin-source.cs"
```

## Benefits

1. **Source Control**: Plugin source code can be version controlled and reviewed
2. **Automated Management**: Plugin types are automatically discovered and synchronized
3. **Round-Tripping**: Source code can be extracted from assemblies for editing
4. **Simplified Workflow**: No need for separate build tools or Visual Studio
5. **Repeatable Builds**: All build metadata is preserved for reproducible compilation
6. **Smart Defaults**: Reuses existing settings to minimize configuration

## Limitations

1. Single-file plugins only (no multi-file project support)
2. Package references must be already loaded in PowerShell session
3. Strong name keys are auto-generated but can't be extracted from existing assemblies
4. No support for embedded resources beyond the metadata footer
5. Compilation errors show line numbers but not in-IDE experience

## Future Enhancements

Potential improvements:
- Multi-file project support
- NuGet package auto-download
- Strong name key extraction/reuse
- IL weaving for custom attributes
- Integration with CI/CD pipelines
- Visual Studio Code extension
