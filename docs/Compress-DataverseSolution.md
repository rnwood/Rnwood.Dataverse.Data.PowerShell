# Compress-DataverseSolution

## SYNOPSIS
Packs a Dataverse solution folder using the Power Apps CLI.

## SYNTAX

```
Compress-DataverseSolution [-Path] <String> [-OutputPath] <String> [-PacVersion <String>] [-WhatIf] [-Confirm] 
 [<CommonParameters>]
```

## DESCRIPTION
The `Compress-DataverseSolution` cmdlet packs an unpacked Dataverse solution folder into a solution ZIP file using the Power Apps CLI (`pac solution pack` command). This is the reverse operation of `Expand-DataverseSolution` and is typically used after editing solution components.

The cmdlet automatically detects and downloads the Power Apps CLI if it's not already available in your PATH or as a .NET global tool.

## EXAMPLES

### Example 1: Pack a solution
```powershell
Compress-DataverseSolution -Path "C:\Solutions\MySolution_Src" -OutputPath "C:\Solutions\MySolution.zip"
```

Packs the solution folder into a ZIP file.

### Example 2: Pack a solution with specific PAC CLI version
```powershell
Compress-DataverseSolution -Path "C:\Solutions\MySolution_Src" -OutputPath "C:\Solutions\MySolution.zip" -PacVersion "1.31.6"
```

Packs the solution using a specific version of the PAC CLI. Any folders with `.msapp` extension are automatically zipped into .msapp files before packing.

## PARAMETERS

### -Path
Path to the solution folder to pack.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath
Output path for the packed solution file (.zip).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PacVersion
PAC CLI version to use (e.g., '1.31.6'). If not specified, uses the latest version.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None (latest version)
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
Default value: None
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
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### None

## NOTES
- Requires the Power Apps CLI (pac). The cmdlet automatically downloads it from NuGet without requiring .NET SDK.
- The Power Apps CLI package is available at: https://www.nuget.org/packages/Microsoft.PowerApps.CLI.Tool
- Canvas App folders (with `.msapp` extension) are automatically detected and packed into .msapp files.
- A temporary copy of the solution folder is created when .msapp folders are found, so the original folder is not modified.
- Use `-PacVersion` to specify a particular PAC CLI version. If omitted, the latest version is used.
- PAC CLI from PATH is ignored - always uses the downloaded version for consistency.

## RELATED LINKS
- [Expand-DataverseSolution](Expand-DataverseSolution.md)
- [Export-DataverseSolution](Export-DataverseSolution.md)
- [Import-DataverseSolution](Import-DataverseSolution.md)
- [Power Apps CLI documentation](https://learn.microsoft.com/power-platform/developer/cli/introduction)
