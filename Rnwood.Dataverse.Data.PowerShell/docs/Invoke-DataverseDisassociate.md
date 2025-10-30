---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDisassociate

## SYNOPSIS
Contains the data that is needed to remove associations between records.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DisassociateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DisassociateRequest)

## SYNTAX

```
Invoke-DataverseDisassociate -Target <PSObject> [-Relationship <Relationship>]
 [-RelatedEntities <EntityReferenceCollection>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to remove associations between records.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDisassociate -Connection <ServiceClient> -Target <PSObject> -Relationship <Relationship> -RelatedEntities <EntityReferenceCollection>
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

### -RelatedEntities
Gets or sets the collection of entity references (references to records) to be disassociated. Required.

```yaml
Type: EntityReferenceCollection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Relationship
Get or sets the name of the relationship to be used for the disassociation. Required.

```yaml
Type: Relationship
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Target
Gets or sets the record from which the related records will be disassociated. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

### System.Management.Automation.PSObject
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
