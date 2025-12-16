---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataversePluginAssemblyFromSource

## SYNOPSIS
Compiles C# source code into a plugin assembly, uploads to Dataverse, and automatically manages plugin types.

## SYNTAX

### SourceCode
```
Set-DataversePluginAssemblyFromSource -SourceCode <String> -Name <String> [-FrameworkReferences <String[]>]
 [-PackageReferences <String[]>] [-StrongNameKeyFile <String>] [-Version <String>] [-Culture <String>]
 [-IsolationMode <PluginAssemblyIsolationMode>] [-Description <String>] [-PassThru] [-Connection <ServiceClient>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### SourceFile
```
Set-DataversePluginAssemblyFromSource -SourceFile <String> -Name <String> [-FrameworkReferences <String[]>]
 [-PackageReferences <String[]>] [-StrongNameKeyFile <String>] [-Version <String>] [-Culture <String>]
 [-IsolationMode <PluginAssemblyIsolationMode>] [-Description <String>] [-PassThru] [-Connection <ServiceClient>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet compiles C# source code into a plugin assembly using the Roslyn compiler, embeds build metadata (source code, references, strong name) in the assembly, uploads it to Dataverse, and automatically discovers and manages plugin types. Plugin types are classes that implement the `Microsoft.Xrm.Sdk.IPlugin` interface.

Key features:
- **Create or Update**: If an assembly with the same name exists, it is updated; otherwise, a new one is created
- **Auto-detect plugin types**: Automatically finds all classes implementing `IPlugin` and creates/updates plugin type records
- **Reuse existing settings**: If not specified, version, culture, strong name key, and references are reused from the existing assembly
- **Generate strong name key**: If no key is specified and no existing assembly has one, a new key is automatically generated
- **Embed metadata**: Source code and build settings are embedded in the assembly for later extraction
- **Error on no plugins**: Throws an error if no plugin types are found in the source code

## EXAMPLES

### Example 1: Create a simple plugin assembly
```powershell
$sourceCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyPlugins
{
    public class AccountPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Plugin logic here
        }
    }
}
"@

Set-DataversePluginAssemblyFromSource -SourceCode $sourceCode -Name "MyPlugins"
```

Compiles the source code, generates a strong name key, and uploads the assembly to Dataverse. A plugin type record is automatically created for `MyPlugins.AccountPlugin`.

### Example 2: Update an existing assembly with new source code
```powershell
Set-DataversePluginAssemblyFromSource -SourceFile "C:\Plugins\MyPlugins.cs" -Name "MyPlugins" -Version "2.0.0.0"
```

Reads source code from a file, compiles it with version 2.0.0.0, and updates the existing "MyPlugins" assembly. Plugin types are automatically added/removed based on changes in the source code.

### Example 3: Specify package references
```powershell
$sourceCode = @"
using Newtonsoft.Json;
using Microsoft.Xrm.Sdk;
// ... plugin code using Newtonsoft.Json
"@

Set-DataversePluginAssemblyFromSource -SourceCode $sourceCode -Name "JsonPlugin" `
    -PackageReferences "Newtonsoft.Json@13.0.1", "Microsoft.Xrm.Sdk@9.0.2"
```

Compiles the source code with the specified NuGet package references.

## PARAMETERS

### -SourceCode
C# source code to compile.

```yaml
Type: String
Parameter Sets: SourceCode
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SourceFile
Path to a C# source file to compile.

```yaml
Type: String
Parameter Sets: SourceFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name of the plugin assembly.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FrameworkReferences
Framework assembly references (e.g., 'System', 'System.Core'). If not specified, existing references are reused from the assembly if it already exists.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PackageReferences
NuGet package references with versions (e.g., 'Microsoft.Xrm.Sdk@9.0.2'). If not specified, existing references are reused from the assembly if it already exists.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StrongNameKeyFile
Path to strong name key file (.snk). If not specified, an existing key is reused if the assembly exists, or a new key is automatically generated.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None (auto-generated or reused)
Accept pipeline input: False
Accept wildcard characters: False
```

### -Version
Assembly version (e.g., '1.0.0.0'). If not specified, the existing assembly version is reused or '1.0.0.0' is used.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: "1.0.0.0" or existing version
Accept pipeline input: False
Accept wildcard characters: False
```

### -Culture
Assembly culture. If not specified, the existing assembly culture is reused or 'neutral' is used.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: "neutral" or existing culture
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsolationMode
Isolation mode for the plugin assembly.

```yaml
Type: PluginAssemblyIsolationMode
Parameter Sets: (All)
Aliases:
Accepted values: None, Sandbox, External

Required: False
Position: Named
Default value: Sandbox
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
Description of the assembly.

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

### -PassThru
If specified, the created/updated assembly is written to the pipeline as a PSObject.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
Dataverse connection obtained from Get-DataverseConnection. If not specified, uses the default connection.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Default connection
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### PSObject (when -PassThru is specified)

## NOTES
- Source code must contain at least one class implementing `Microsoft.Xrm.Sdk.IPlugin`, otherwise an error is thrown
- Plugin types are automatically created/removed based on classes implementing `IPlugin`
- Build metadata (source code, references, strong name info) is embedded in the assembly and can be extracted using `Get-DataversePluginAssemblySource`
- Strong name key files are automatically generated if not specified and not already present in an existing assembly

## RELATED LINKS

[Get-DataversePluginAssemblySource](Get-DataversePluginAssemblySource.md)
[Get-DataversePluginAssembly](Get-DataversePluginAssembly.md)
[Set-DataversePluginAssembly](Set-DataversePluginAssembly.md)
