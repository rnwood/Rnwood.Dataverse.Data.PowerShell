---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCreatePolymorphicLookupAttribute

## SYNOPSIS
Contains the data to create a multi-table lookup column.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreatePolymorphicLookupAttributeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreatePolymorphicLookupAttributeRequest?view=dataverse-sdk-latest)

## SYNTAX

```
Invoke-DataverseCreatePolymorphicLookupAttribute [-Lookup <LookupAttributeMetadata>]
 [-OneToManyRelationships <OneToManyRelationshipMetadata[]>] [-SolutionUniqueName <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data to create a multi-table lookup column.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCreatePolymorphicLookupAttribute -Connection <ServiceClient> -Lookup <LookupAttributeMetadata> -OneToManyRelationships <OneToManyRelationshipMetadata[]> -SolutionUniqueName <String>
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

### -Lookup
Gets or sets the lookup column definition that stores a reference to the related row.

```yaml
Type: LookupAttributeMetadata
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OneToManyRelationships
Gets or sets the metadata array for the one-to-many relationships to the Account and Contact tables. Required.

```yaml
Type: OneToManyRelationshipMetadata[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SolutionUniqueName
Gets or sets the name of the unmanaged solution to which you want to add this column. Optional.

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

