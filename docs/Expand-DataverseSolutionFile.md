# Expand-DataverseSolutionFile

## SYNOPSIS
Unpacks a Dataverse solution file using the Power Apps CLI.

## SYNTAX

```
Expand-DataverseSolutionFile [-Path] <String> [-OutputPath] <String> [-UnpackMsapp] [-PackageType <String>] 
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Expand-DataverseSolutionFile` cmdlet unpacks a Dataverse solution ZIP file into a folder structure using the Power Apps CLI (`pac solution unpack` command). This is useful for version control and manual editing of solution components.

The cmdlet automatically detects and downloads the Power Apps CLI if it's not already available in your PATH or as a .NET global tool.

## EXAMPLES

### Example 1: Unpack a solution
```powershell
Expand-DataverseSolutionFile -Path "C:\Solutions\MySolution.zip" -OutputPath "C:\Solutions\MySolution_Src"
```

Unpacks the solution file to the specified output folder.

### Example 2: Unpack a solution and extract .msapp files
```powershell
Expand-DataverseSolutionFile -Path "C:\Solutions\MySolution.zip" -OutputPath "C:\Solutions\MySolution_Src" -UnpackMsapp
```

Unpacks the solution and additionally extracts any Canvas Apps (.msapp files) found in the solution into folders with the same name.



## PARAMETERS

### -Path
Path to the solution file (.zip) to unpack.

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
Output path where the solution will be unpacked.

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

### -UnpackMsapp
Unpack .msapp files found in the solution into folders (same name without extension). Canvas App (.msapp) files are ZIP archives that can be unpacked for version control.

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

### -PackageType
Package type: 'Unmanaged' (default), 'Managed', or 'Both' for dual Managed and Unmanaged operation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Unmanaged
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
- Requires the Power Apps CLI (pac) to be installed and available in your PATH.
- Install from https://aka.ms/PowerAppsCLI
- The `-UnpackMsapp` switch is useful for version control of Canvas Apps, allowing you to see individual file changes.
- The cmdlet always uses clobber and allowDelete modes for consistent behavior.
- Use `-PackageType` to specify 'Unmanaged' (default), 'Managed', or 'Both' for dual Managed and Unmanaged operation.

## RELATED LINKS
- [Compress-DataverseSolutionFile](Compress-DataverseSolutionFile.md)
- [Export-DataverseSolution](Export-DataverseSolution.md)
- [Import-DataverseSolution](Import-DataverseSolution.md)
- [Power Apps CLI documentation](https://learn.microsoft.com/power-platform/developer/cli/introduction)
