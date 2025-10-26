---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSolutionFileComponent

## SYNOPSIS
Extracts the components from a solution file (.zip).

## SYNTAX

### FromFile
```
Get-DataverseSolutionFileComponent [-SolutionFile] <String> [-IncludeSubcomponents]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FromBytes
```
Get-DataverseSolutionFileComponent -SolutionBytes <Byte[]> [-IncludeSubcomponents]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseSolutionFileComponent cmdlet extracts the root components from a solution file (.zip). It can read the solution from a file path or from byte array input.

When the IncludeSubcomponents parameter is specified, it also extracts subcomponents for entity components (type 1), such as attributes.

The cmdlet outputs PowerShell objects representing each component with properties including ObjectId, ComponentType, ComponentTypeName, Behavior, MetadataId, and IsSubcomponent.

## EXAMPLES

### Example 1: Extract components from a solution file
```powershell
PS C:\> Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution.zip"
```

This example extracts all root components from the solution file MySolution.zip.

### Example 2: Extract components from solution bytes
```powershell
PS C:\> $bytes = [System.IO.File]::ReadAllBytes("C:\Solutions\MySolution.zip")
PS C:\> $bytes | Get-DataverseSolutionFileComponent
```

This example reads the solution file as bytes and pipes them to extract components.

### Example 3: Extract components including subcomponents
```powershell
PS C:\> Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution.zip" -IncludeSubcomponents
```

This example extracts all root components and their subcomponents from the solution file.

### Example 4: Filter components by type
```powershell
PS C:\> $components = Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $components | Where-Object { $_.ComponentType -eq 1 } | Format-Table
```

This example extracts all components from the solution file and filters to show only entities (ComponentType 1).

### Example 5: Count components by type
```powershell
PS C:\> $components = Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $components | Group-Object ComponentTypeName | Select-Object Name, Count | Sort-Object Count -Descending
```

This example extracts all components and groups them by component type to show a count of each type.

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

### -IncludeSubcomponents
Include subcomponents (attributes, relationships, forms, views, etc.) from the solution file.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SolutionBytes
Solution file bytes to analyze.

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
Path to the solution file (.zip) to analyze.

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
- **Component Types**: The cmdlet returns components with numeric ComponentType values. Common types include:
  - 1: Entity
  - 2: Attribute
  - 10: Entity Relationship
  - 24: Form
  - 26: Saved Query (View)
  - 60: System Form
  - 61: Web Resource
  - 62: Site Map
  - Full list available at: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/solutioncomponent

- **Behavior Values**: The Behavior property indicates how the component is included in the solution:
  - Include Subcomponents (0): Full component with all subcomponents
  - Do Not Include Subcomponents (1): Component without subcomponents
  - Include As Shell (2): Shell only, no subcomponents

- **Subcomponents**: When IncludeSubcomponents is used, only entity attributes are extracted from the customizations.xml in the solution file. Subcomponents have IsSubcomponent set to true and include ParentComponentType and ParentObjectId properties.

- **File Format**: The solution file must be a valid .zip file containing solution.xml and customizations.xml.

## RELATED LINKS

[Get-DataverseSolutionComponent](Get-DataverseSolutionComponent.md)
[Compare-DataverseSolutionComponents](Compare-DataverseSolutionComponents.md)
[Export-DataverseSolution](Export-DataverseSolution.md)
[Import-DataverseSolution](Import-DataverseSolution.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
