---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveEntity

## SYNOPSIS
Contains the data that is needed to retrieve the definition of a table.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest)

## SYNTAX

```
Invoke-DataverseRetrieveEntity [-EntityFilters <EntityFilters>] [-LogicalName <String>] [-MetadataId <Guid>]
 [-RetrieveAsIfPublished <Boolean>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to retrieve the definition of a table.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveEntity -Connection <ServiceClient> -EntityFilters <EntityFilters> -LogicalName <String> -MetadataId <Guid> -RetrieveAsIfPublished <Boolean>
```

## PARAMETERS

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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

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

### -EntityFilters
Gets or sets a filter to control how much data for each table is retrieved. Required.

```yaml
Type: EntityFilters
Parameter Sets: (All)
Aliases:
Accepted values: Entity, Default, Attributes, Privileges, Relationships, All

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LogicalName
Gets or sets the logical name of the column to delete. Required.

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

### -MetadataId
The unique identifier for the attribute. Optional.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RetrieveAsIfPublished
Gets or sets whether to retrieve the metadata that has not been published. Required.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
