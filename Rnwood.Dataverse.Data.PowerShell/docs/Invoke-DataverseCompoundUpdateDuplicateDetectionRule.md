---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCompoundUpdateDuplicateDetectionRule

## SYNOPSIS
Contains the data that is needed to update a duplicate rule (duplicate detection rule) and its related duplicate rule conditions.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CompoundUpdateDuplicateDetectionRuleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CompoundUpdateDuplicateDetectionRuleRequest)

## SYNTAX

```
Invoke-DataverseCompoundUpdateDuplicateDetectionRule [-Entity <PSObject>] [-EntityTableName <String>]
 [-EntityIgnoreProperties <String[]>] [-EntityLookupColumns <Hashtable>] [-ChildEntities <EntityCollection>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to update a duplicate rule (duplicate detection rule) and its related duplicate rule conditions.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCompoundUpdateDuplicateDetectionRule -Connection <ServiceClient> -Entity <PSObject> -EntityTableName <String> -EntityIgnoreProperties <String[]> -EntityLookupColumns <Hashtable> -ChildEntities <EntityCollection>
```

## PARAMETERS

### -ChildEntities
Gets or sets a collection of the duplicate rule conditions that you want updated. Required.

```yaml
Type: EntityCollection
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

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Entity
Gets or sets the duplicate rule that you want updated. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EntityIgnoreProperties
Gets or sets the duplicate rule that you want updated. Required. Properties to ignore when converting Entity PSObject to Entity.

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

### -EntityLookupColumns
Gets or sets the duplicate rule that you want updated. Required. Hashtable specifying lookup columns for entity reference conversions in Entity.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EntityTableName
Gets or sets the duplicate rule that you want updated. Required. The logical name of the table/entity type for the Entity parameter.

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
