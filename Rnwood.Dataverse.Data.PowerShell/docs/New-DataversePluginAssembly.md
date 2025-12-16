---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# New-DataversePluginAssembly

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### SourceCode
```
New-DataversePluginAssembly -SourceCode <String> -AssemblyName <String> [-FrameworkReferences <String[]>]
 [-PackageReferences <String[]>] [-StrongNameKeyFile <String>] [-Version <String>] [-Culture <String>]
 [-OutputPath <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### SourceFile
```
New-DataversePluginAssembly -SourceFile <String> -AssemblyName <String> [-FrameworkReferences <String[]>]
 [-PackageReferences <String[]>] [-StrongNameKeyFile <String>] [-Version <String>] [-Culture <String>]
 [-OutputPath <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -AssemblyName
Name of the assembly

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

### -Culture
Assembly culture (e.g., 'neutral')

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

### -FrameworkReferences
Framework assembly references (e.g., 'System', 'System.Core')

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

### -OutputPath
Output path for the compiled assembly

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

### -PackageReferences
NuGet package references with versions (e.g., 'Microsoft.Xrm.Sdk@9.0.2')

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

### -ProgressAction
{{ Fill ProgressAction Description }}

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

### -SourceCode
C# source code to compile

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
Path to C# source file

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

### -StrongNameKeyFile
Path to strong name key file (.snk)

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

### -Version
Assembly version (e.g., '1.0.0.0')

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Byte[]
## NOTES

## RELATED LINKS
