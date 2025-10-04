---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCreateManyToMany

## SYNOPSIS
Contains the data that is needed to create a new Many-to-Many (N:N) table relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateManyToManyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateManyToManyRequest)

## SYNTAX

```
Invoke-DataverseCreateManyToMany -Connection <ServiceClient> [-IntersectEntitySchemaName <String>]
 [-ManyToManyRelationship <ManyToManyRelationshipMetadata>] [-SolutionUniqueName <String>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to create a new Many-to-Many (N:N) table relationship.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCreateManyToMany -Connection <ServiceClient> -IntersectEntitySchemaName <String> -ManyToManyRelationship <ManyToManyRelationshipMetadata> -SolutionUniqueName <String>
```

## PARAMETERS

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

### -IntersectEntitySchemaName
Gets or sets the name of the intersect entity to be created for this entity relationship. Required.

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

### -ManyToManyRelationship
Gets or sets the definition of the Many-to-Many table relationship to be created. Required.

```yaml
Type: ManyToManyRelationshipMetadata
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SolutionUniqueName
Gets or sets the name of the unmanaged solution you want to add this many-to-many entity relationship to. Optional.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
