---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Compare-DataverseSolution

## SYNOPSIS
Compares a solution file with the state of that solution in the target environment.

## SYNTAX

### FromFile
```
Compare-DataverseSolution [-SolutionFile] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FromBytes
```
Compare-DataverseSolution -SolutionBytes <Byte[]> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Compare-DataverseSolution cmdlet compares a solution file (ZIP) with the state of that solution in a Dataverse environment. It outputs an item for each component and subcomponent showing whether the component has been added, removed, or modified between the file and the environment.

The cmdlet takes into account the rootcomponentbehavior field in the solutioncomponent table:
- 0 = Include Subcomponents (Full component)
- 1 = Do Not Include Subcomponents
- 2 = Include As Shell (Shell only, no subcomponents)

If a component changes from full (behavior 0) to shell (behavior 2), this counts as a modification where subcomponents would be removed.

## EXAMPLES

### Example 1: Compare a solution file with the target environment
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> Compare-DataverseSolution -Connection $conn -SolutionFile "C:\Solutions\MySolution_1_0_0_0.zip"
```

This example compares the MySolution solution file with the state of the solution in the target environment and outputs the differences.

### Example 2: Filter results to show only added components
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $results = Compare-DataverseSolution -Connection $conn -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $results | Where-Object { $_.Status -eq "Added" } | Format-Table
```

This example shows only the components that have been added to the solution file but don't exist in the environment.

### Example 3: Compare using solution bytes from Export-DataverseSolution
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $solutionBytes = Export-DataverseSolution -Connection $conn -SolutionName "MySolution" -PassThru
PS C:\> Compare-DataverseSolution -Connection $conn -SolutionBytes $solutionBytes
```

This example exports a solution as bytes and then compares it with the same solution in the environment.

### Example 4: Identify behavior changes
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $results = Compare-DataverseSolution -Connection $conn -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $results | Where-Object { $_.FileBehavior -ne $_.EnvironmentBehavior } | Format-Table
```

This example shows components where the behavior has changed (e.g., from full component to shell).

### Example 5: Export comparison results to CSV
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $results = Compare-DataverseSolution -Connection $conn -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $results | Export-Csv -Path "C:\Reports\solution-comparison.csv" -NoTypeInformation
```

This example exports the comparison results to a CSV file for further analysis.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -SolutionBytes
Solution file bytes to compare.

```yaml
Type: Byte[]
Parameter Sets: FromBytes
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SolutionFile
Path to the solution file (.zip) to compare.

```yaml
Type: String
Parameter Sets: FromFile
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Standard PowerShell preference parameter that controls the display of progress information. This cmdlet does not emit progress directly but respects this parameter if passed.

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

### System.Byte[]
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- If the solution does not exist in the target environment, all components will be marked as "Added"
- Components that exist in both the file and environment are marked as "Modified" because this cmdlet cannot perform deep inspection of component definitions to detect actual content changes - it only compares component presence and behavior
- Behavior changes (e.g., full component to shell component) are detected and the component is marked as "Modified"
- The cmdlet uses the solutioncomponent table to query components in the target environment

## RELATED LINKS

[Export-DataverseSolution](Export-DataverseSolution.md)
[Import-DataverseSolution](Import-DataverseSolution.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
