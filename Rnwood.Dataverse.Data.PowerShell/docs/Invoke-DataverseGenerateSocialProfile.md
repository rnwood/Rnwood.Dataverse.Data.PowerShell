---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGenerateSocialProfile

## SYNOPSIS
Contains the data to return an existing social profile record if one exists, otherwise generates a new one and returns it.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateSocialProfileRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GenerateSocialProfileRequest)

## SYNTAX

```
Invoke-DataverseGenerateSocialProfile -Connection <ServiceClient> [-Entity <PSObject>]
 [-EntityTableName <String>] [-EntityIgnoreProperties <String[]>] [-EntityLookupColumns <Hashtable>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data to return an existing social profile record if one exists, otherwise generates a new one and returns it.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGenerateSocialProfile -Connection <ServiceClient> -Entity <PSObject> -EntityTableName <String> -EntityIgnoreProperties <String[]> -EntityLookupColumns <Hashtable>
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

### -Entity
Gets or sets the social profile instance to return or generate. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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
Gets or sets the social profile instance to return or generate. Properties to ignore when converting Entity PSObject to Entity.

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
Gets or sets the social profile instance to return or generate. Hashtable specifying lookup columns for entity reference conversions in Entity.

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
Gets or sets the social profile instance to return or generate. The logical name of the table/entity type for the Entity parameter.

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
