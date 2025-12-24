---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseDynamicPluginAssembly

## SYNOPSIS
Extracts source code and build metadata from a dynamic plugin assembly.

## SYNTAX

### ById (Default)
```
Get-DataverseDynamicPluginAssembly -Id <Guid> [-OutputSourceFile <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectById
```
Get-DataverseDynamicPluginAssembly -Id <Guid> [-OutputSourceFile <String>] -OutputProjectPath <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByName
```
Get-DataverseDynamicPluginAssembly -Name <String> [-OutputSourceFile <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectByName
```
Get-DataverseDynamicPluginAssembly -Name <String> [-OutputSourceFile <String>] -OutputProjectPath <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Bytes
```
Get-DataverseDynamicPluginAssembly -AssemblyBytes <Byte[]> [-OutputSourceFile <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectFromBytes
```
Get-DataverseDynamicPluginAssembly -AssemblyBytes <Byte[]> [-OutputSourceFile <String>]
 -OutputProjectPath <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### FilePath
```
Get-DataverseDynamicPluginAssembly -FilePath <String> [-OutputSourceFile <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectFromFile
```
Get-DataverseDynamicPluginAssembly -FilePath <String> [-OutputSourceFile <String>] -OutputProjectPath <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet extracts embedded metadata from plugin assemblies created with `Set-DataverseDynamicPluginAssembly`. The metadata includes the original C# source code, framework and package references, version information, culture, and the strong name key used for signing.

Dynamic plugin assemblies embed this metadata during compilation to enable:
- Source code retrieval for review or modification
- Rebuilding with the same settings (references, version, culture)
- Strong name key reuse to maintain consistent public key tokens across updates

If the assembly was not created with `Set-DataverseDynamicPluginAssembly` or does not contain embedded metadata, a warning is displayed and no output is returned.

## EXAMPLES

### Example 1: Extract metadata from an assembly file
```powershell
$metadata = Get-DataverseDynamicPluginAssembly -FilePath "C:\Plugins\MyDynamicPlugin.dll"
Write-Host "Assembly: $($metadata.AssemblyName)"
Write-Host "Version: $($metadata.Version)"
Write-Host "Public Key Token: $($metadata.PublicKeyToken)"
Write-Host "Framework References: $($metadata.FrameworkReferences -join ', ')"
Write-Host "Package References: $($metadata.PackageReferences -join ', ')"
```

Extracts and displays metadata from a plugin assembly file.

### Example 2: Extract source code from downloaded assembly
```powershell
# Download assembly from Dataverse
$assembly = Get-DataverseRecord -TableName pluginassembly -FilterValues @{ name = "MyDynamicPlugin" } -Columns content

# Decode and extract metadata
$assemblyBytes = [Convert]::FromBase64String($assembly.content)
$metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyBytes -OutputSourceFile "C:\Temp\Plugin.cs"

Write-Host "Source code saved to C:\Temp\Plugin.cs"
Write-Host "Assembly uses $($metadata.FrameworkReferences.Count) framework references"
```

Downloads a plugin assembly from Dataverse, extracts its metadata, and saves the source code to a file.

### Example 3: Verify assembly metadata before update
```powershell
# Get current assembly
$assembly = Get-DataverseRecord -TableName pluginassembly -FilterValues @{ name = "MyPlugin" } -Columns content
$bytes = [Convert]::FromBase64String($assembly.content)
$metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes

# Display current settings
Write-Host "Current version: $($metadata.Version)"
Write-Host "Current culture: $($metadata.Culture)"
Write-Host "Public key token: $($metadata.PublicKeyToken)"

# Update with modified source code but same settings
Set-DataverseDynamicPluginAssembly -SourceCode $modifiedSource -Name "MyPlugin"
```

Retrieves the current metadata from an existing assembly to verify settings before updating it.

### Example 4: Retrieve and extract metadata by name from Dataverse
```powershell
# Connect to Dataverse
$connection = Get-DataverseConnection -Url "https://org.crm.dynamics.com" -Interactive

# Retrieve plugin assembly by name and extract metadata
$metadata = Get-DataverseDynamicPluginAssembly -Connection $connection -Name "MyDynamicPlugin"

# Display metadata
Write-Host "Assembly: $($metadata.AssemblyName)"
Write-Host "Version: $($metadata.Version)"
Write-Host "Source Code Lines: $($metadata.SourceCode.Split("`n").Count)"
```

Directly retrieves a dynamic plugin assembly from Dataverse by name and extracts its metadata without manual download steps.

### Example 5: Retrieve by ID and export to VS project
```powershell
# Connect to Dataverse
$connection = Get-DataverseConnection -Url "https://org.crm.dynamics.com" -Interactive

# Get assembly ID (e.g., from a previous query)
$assemblyId = [Guid]"12345678-1234-1234-1234-123456789012"

# Retrieve and export to VS project in one step
Get-DataverseDynamicPluginAssembly -Connection $connection -Id $assemblyId -OutputProjectPath "C:\Dev\MyPlugin"

# The project is ready to build
cd C:\Dev\MyPlugin
dotnet build
```

Retrieves a plugin assembly by ID from Dataverse and exports it directly to a complete Visual Studio project.

### Example 6: Export to complete Visual Studio project from bytes
```powershell
# Download assembly from Dataverse
$assembly = Get-DataverseRecord -TableName pluginassembly -FilterValues @{ name = "MyDynamicPlugin" } -Columns content

# Decode and export to VS project
$assemblyBytes = [Convert]::FromBase64String($assembly.content)
Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyBytes -OutputProjectPath "C:\Dev\MyPluginProject"

# The output directory will contain:
#   - MyDynamicPlugin.cs (source code)
#   - MyDynamicPlugin.csproj (project file targeting .NET Framework 4.6.2)
#   - MyDynamicPlugin.snk (strong name key for signing)

# Open in Visual Studio or build from command line
cd C:\Dev\MyPluginProject
dotnet build
```

Exports a complete Visual Studio project that can be opened, modified, and rebuilt. The generated project includes all necessary files and configurations to build a plugin assembly identical to the original, including the strong name key for maintaining the same PublicKeyToken.

## PARAMETERS

### -AssemblyBytes
Assembly bytes

```yaml
Type: Byte[]
Parameter Sets: Bytes, VSProjectFromBytes
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilePath
Path to assembly file

```yaml
Type: String
Parameter Sets: FilePath, VSProjectFromFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the plugin assembly

```yaml
Type: Guid
Parameter Sets: ById, VSProjectById
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
Name of the plugin assembly

```yaml
Type: String
Parameter Sets: ByName, VSProjectByName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputProjectPath
Output directory for Visual Studio project

```yaml
Type: String
Parameter Sets: VSProjectById, VSProjectByName, VSProjectFromBytes, VSProjectFromFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputSourceFile
Output path for extracted source code

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Determines how PowerShell responds to progress updates. See about_CommonParameters for details.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
### System.Byte[]
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Output Properties:**
- **AssemblyName**: The name of the assembly
- **Version**: Assembly version (e.g., "1.0.0.0")
- **Culture**: Assembly culture (e.g., "neutral")
- **PublicKeyToken**: Hex string of the public key token used for strong naming
- **SourceCode**: The original C# source code used to compile the assembly
- **FrameworkReferences**: Array of .NET Framework assembly references
- **PackageReferences**: Array of NuGet package references with versions (e.g., "Newtonsoft.Json@13.0.1")

**Metadata Format:**
The metadata is embedded at the end of the assembly file with a marker "DPLM" (Dynamic Plugin Library Metadata). The strong name key is stored as a base64-encoded string and is automatically reused when updating the assembly with `Set-DataverseDynamicPluginAssembly`.

**Use Cases:**
- Review the source code of deployed plugins
- Extract build settings to replicate the configuration
- Audit framework and package dependencies
- Retrieve the strong name key for manual builds
- Export to a complete Visual Studio project for editing and rebuilding

**Visual Studio Project Export:**
When using the `-OutputProjectPath` parameter, the cmdlet generates a complete Visual Studio project in the specified directory containing:
- **{AssemblyName}.cs**: The original C# source code
- **{AssemblyName}.csproj**: A .NET Framework 4.6.2 project file with all package and framework references configured
- **{AssemblyName}.snk**: The strong name key file used for assembly signing

The generated project can be opened in Visual Studio or built using `dotnet build`. The resulting assembly will have the same strong name and PublicKeyToken as the original, ensuring compatibility when updating plugins in Dataverse.

## RELATED LINKS

[Set-DataverseDynamicPluginAssembly](Set-DataverseDynamicPluginAssembly.md)
[Get-DataversePluginAssembly](Get-DataversePluginAssembly.md)
