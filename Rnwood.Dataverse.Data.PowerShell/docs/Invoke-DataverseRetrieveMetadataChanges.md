---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveMetadataChanges

## SYNOPSIS
Contains the data that is needed to retrieve a collection of records that satisfy the specified criteria. The returns a value that can be used with this request at a later time to return information about how schema definitions have changed since the last request.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveMetadataChangesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveMetadataChangesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveMetadataChanges -Connection <ServiceClient> [-Query <EntityQueryExpression>]
 [-DeletedMetadataFilters <DeletedMetadataFilters>] [-ClientVersionStamp <String>]
 [-RetrieveAllSettings <Boolean>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to retrieve a collection of records that satisfy the specified criteria. The returns a value that can be used with this request at a later time to return information about how schema definitions have changed since the last request.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveMetadataChanges -Connection <ServiceClient> -Query <EntityQueryExpression> -DeletedMetadataFilters <DeletedMetadataFilters> -ClientVersionStamp <String> -RetrieveAllSettings <Boolean>
```

## PARAMETERS

### -ClientVersionStamp
Gets or sets a timestamp value representing when the last request was made.

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

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DeletedMetadataFilters
Gets or sets a value to filter what deleted metadata items will be returned.

```yaml
Type: DeletedMetadataFilters
Parameter Sets: (All)
Aliases:
Accepted values: Entity, Default, Attribute, Relationship, Label, OptionSet, All

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Query
Gets or sets the query representing the metadata to return.

```yaml
Type: EntityQueryExpression
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RetrieveAllSettings
For internal use only.

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
